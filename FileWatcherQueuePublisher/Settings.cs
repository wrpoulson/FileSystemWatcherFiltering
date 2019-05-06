using System;
using System.Collections.Generic;
using System.Text;

namespace FileWatcherQueuePublisher
{
  public class Settings
  {
    public string WatcherPath { get; set; }

    public string QueueName { get; set; }

    public string ExchangeName { get; set; }

    public string RoutingKey { get; set; }

    public bool EnableThreading { get; set; }

    public int MaxThreadCount { get; set; }
  }
}
