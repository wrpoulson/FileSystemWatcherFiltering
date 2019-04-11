using System;
using System.IO;
using System.Text.RegularExpressions;
using Serilog;

namespace FileSystemWatcherFiltering
{
  public class DirectoryWatcher
  {
    private Regex regex = new Regex(@"(VFTP\\XPEDITOR\\L\d{3}\\CLIENTFTP\\DROPOFF\\CLAIMS\\RealTime)");

    public void WatchFilesAndChill(Settings settings)
    {
      FileSystemWatcher watcher = new FileSystemWatcher();
      watcher.Path = settings.WatcherPath;
      watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
      watcher.Created += new FileSystemEventHandler(OnChanged);
      watcher.IncludeSubdirectories = true;
      watcher.EnableRaisingEvents = true;

      Console.WriteLine($" Watching {settings.WatcherPath}");
      Console.WriteLine("\n Press 'q' to close application.\n");
      while (Console.Read() != 'q') ;
    }

    private void OnChanged(object source, FileSystemEventArgs e)
    {
      if (regex.IsMatch(e.FullPath))
      {
        Log.Information($"File received: {e.FullPath} via {e.ChangeType}");
      }
    }
  }
}
