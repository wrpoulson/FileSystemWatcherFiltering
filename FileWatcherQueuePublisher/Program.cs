using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace FileWatcherQueuePublisher
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.Title = typeof(Program).Assembly.GetName().Name;
      Log.Logger = new LoggerConfiguration()
        .WriteTo.File($"C:\\RealTimeHimFileDropTest\\Logs\\{DateTime.Now.ToString("MM_dd_yyyy")}_{Environment.UserName}.{typeof(Program).Assembly.GetName().Name}.log")
        .CreateLogger();

      var settings = GetSettings();

      if (settings.EnableThreading)
      {
        //var watchers = new List<DirectoryWatcher>();
        Random rng = new Random();
        for (var i = 1; i < settings.MaxThreadCount + 1; i++)
        {
          var watcherPath = Path.Combine(settings.WatcherPath, $"L{i.ToString("D3")}");
          new Thread(() =>
            new DirectoryWatcher(settings, watcherPath).WatchFilesAndChill()
          ).Start();
        }
      }
      else
      {
        DirectoryWatcher watcher = new DirectoryWatcher(settings);
        watcher.WatchFilesAndChill();
      }
    }

    private static Settings GetSettings()
    {
      var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

      IConfigurationRoot configuration = builder.Build();
      var settings = configuration.GetSection(nameof(Settings)).Get<Settings>();
      Log.Information($"Settings on launch: {Newtonsoft.Json.JsonConvert.SerializeObject(settings)}");
      return settings;
    }
  }
}
