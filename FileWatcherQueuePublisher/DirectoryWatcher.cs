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
    private Settings _settings;
    private RabbitMqPublisher publisher;
    private Regex regex = new Regex(@"(VFTP\\XPEDITOR\\L\d{3}\\CLIENTFTP\\DROPOFF\\CLAIMS\\RealTime)");
    int count = 0;

    public DirectoryWatcher(Settings settings)
    {
      _settings = settings;
      publisher = new RabbitMqPublisher(settings).DeclareQueue();
    }

    private BlockingCollection<string> fileEventMessages = new BlockingCollection<string>();

    public void WatchFilesAndChill()
    {
      FileSystemWatcher watcher = new FileSystemWatcher();
      watcher.Path = _settings.WatcherPath;
      watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
      watcher.Created += new FileSystemEventHandler(OnChanged);
      watcher.IncludeSubdirectories = true;
      watcher.EnableRaisingEvents = true;
      //watcher.InternalBufferSize = 64;

      RunConcurrentQueueing();

      Console.WriteLine($" Watching {_settings.WatcherPath}");
      Log.Information($"Watching started on {_settings.WatcherPath}");

      Console.WriteLine("\n Press 'q' to close application.\n");
      while (Console.Read() != 'q') ;

      Log.Information($"Watching ended on {_settings.WatcherPath}, {count} files observed");
    }

    private void RunConcurrentQueueing()
    {
      Task.Run(() =>
      {
        while (!fileEventMessages.IsCompleted)
        {
          try
          {
            var nextMessage = fileEventMessages.Take();
            publisher.SendMessage(nextMessage);
            Log.Information(nextMessage);
          }
          catch (InvalidOperationException)
          {
            Console.WriteLine("Adding was completed!");
            break;
          }
        }

        Console.WriteLine("\r\nNo more items to take. Press the Enter key to exit.");
      });
    }

    private void OnChanged(object source, FileSystemEventArgs e)
    {
      if (regex.IsMatch(e.FullPath))
      {
        count++;
        fileEventMessages.Add($"File received: {e.FullPath} via {e.ChangeType}");
      }
    }
  }
}
