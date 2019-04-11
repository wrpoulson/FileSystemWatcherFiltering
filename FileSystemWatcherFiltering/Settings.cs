using System;
using System.Collections.Generic;
using System.Text;

namespace FileSystemWatcherFiltering
{
  public class Settings
  {
    public string WatcherPath { get; set; }

    public string QueueName { get; set; }

    public string ExchangeName { get; set; }

    public string RoutingKey { get; set; }
  }
}
