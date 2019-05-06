using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;

namespace FileWatcherQueuePublisher
{
  public class DirectoryWatcher
  {
    private readonly string _watcherPath;
    private Settings _settings;
    private RabbitMqPublisher _publisher;
    private BlockingCollection<string> _fileEventMessages;
    private Regex regex = new Regex(@"(VFTP\\XPEDITOR\\EDITEST\\RealTimeHIM\\FileSystemWatcherLimitations_XP9631\\1_WatchAllTheDirectories\\TheDrop\\L\d{3}\\CLIENTFTP\\DROPOFF\\CLAIMS\\RealTime)");

    private int count = 0;

    public DirectoryWatcher(Settings settings)
    {
      _settings = settings;
      _publisher = new RabbitMqPublisher(settings).DeclareQueue();
      _fileEventMessages = new BlockingCollection<string>();
      _watcherPath = settings.WatcherPath;
    }
    public DirectoryWatcher(Settings settings, string watcherPath)
    {
      _settings = settings;
      _publisher = new RabbitMqPublisher(settings).DeclareQueue();
      _fileEventMessages = new BlockingCollection<string>();
      _watcherPath = watcherPath;
    }

    public void WatchFilesAndChill()
    {
      FileSystemWatcher watcher = new FileSystemWatcher();
      watcher.Path = _watcherPath;
      watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
      watcher.Created += new FileSystemEventHandler(OnChanged);
      watcher.IncludeSubdirectories = true;
      watcher.EnableRaisingEvents = true;
      //watcher.InternalBufferSize = 64;

      RunConcurrentQueueing();

      Console.WriteLine($" Watching {_watcherPath}");
      Log.Information($"Watching started on {_watcherPath}");

      Console.WriteLine("\n Press 'q' to close application.\n");
      while (Console.Read() != 'q') ;

      Log.Information($"Watching ended on {_watcherPath}, {count} files observed");
    }

    private void RunConcurrentQueueing()
    {
      Task.Run(() =>
      {
        while (!_fileEventMessages.IsCompleted)
        {
          try
          {
            var nextMessage = _fileEventMessages.Take();
            _publisher.SendMessage(nextMessage);
            Log.Information(nextMessage);
          }
          catch (InvalidOperationException)
          {
            Console.WriteLine("Adding was completed!");
            break;
          }
          catch(Exception ex)
          {
            Log.Error(ex, "Error queueing file event.");
          }
        }
      });
    }

    private void OnChanged(object source, FileSystemEventArgs e)
    {
      if (regex.IsMatch(e.FullPath))
      {
        count++;
        _fileEventMessages.Add($"File {e.ChangeType}: {e.FullPath}");
      }
    }
  }
}
