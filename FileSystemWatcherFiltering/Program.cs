using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;using Microsoft.Extensions.Configuration;
using Serilog;

namespace FileSystemWatcherFiltering
{
  class Program
  {
    static void Main(string[] args)
    {
      Log.Logger = new LoggerConfiguration().WriteTo.File($"{Environment.UserName}.{typeof(Program).Assembly.GetName().Name}.log").CreateLogger();
      Settings settings = GetSettings();
      Log.Information($"Settings on launch: {Newtonsoft.Json.JsonConvert.SerializeObject(settings)}");

      WatchFilesAndChill(settings);
    }

    private static void WatchFilesAndChill(Settings settings)
    {
      FileSystemWatcher watcher = new FileSystemWatcher();
      watcher.Path = settings.WatcherPath;
      watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
      watcher.Created += new FileSystemEventHandler(OnChanged);
      watcher.IncludeSubdirectories = true;

      watcher.EnableRaisingEvents = true;

      Console.WriteLine($"Watching {settings.WatcherPath}");
      Console.WriteLine("\nPress 'q' to close application.\n");
      while (Console.Read() != 'q') ;
    }

    public static void OnChanged(object source, FileSystemEventArgs e)
    {
      Regex regex = new Regex(@"(VFTP\\XPEDITOR\\L\d{3}\\CLIENTFTP\\DROPOFF\\CLAIMS\\RealTime)");

      if (regex.IsMatch(e.FullPath))
      {
        Log.Information($"File received: {e.FullPath} via {e.ChangeType}");
      }
    }

    private static Settings GetSettings()
    {
      var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

      IConfigurationRoot configuration = builder.Build();
      return configuration.GetSection(nameof(Settings)).Get<Settings>();
    }

    private class Settings
    {
      public string WatcherPath { get; set; }
    }
  }
}
