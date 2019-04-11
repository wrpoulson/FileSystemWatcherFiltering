
using System;
using System.Text;
using RabbitMQ.Client;

namespace FileSystemWatcherFiltering
{
  public class RabbitMqPublisher
  {
    public string QueueName { get; set; }

    public string ExchangeName { get; set; }

    public string RoutingKey { get; set; }

    public RabbitMqPublisher(Settings settings)
    {
      QueueName = settings.QueueName;
      ExchangeName = settings.ExchangeName;
      RoutingKey = settings.RoutingKey;
    }

    public RabbitMqPublisher DeclareQueue()
    {
      var factory = new ConnectionFactory() { HostName = "localhost" };
      using (var connection = factory.CreateConnection())
      using (var channel = connection.CreateModel())
      {
        channel.QueueDeclare(queue: QueueName,
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);
      }

      return this;
    }

    public void SendMessage(string message)
    {
      var factory = new ConnectionFactory() { HostName = "localhost" };
      using (var connection = factory.CreateConnection())
      using (var channel = connection.CreateModel())
      {
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: ExchangeName,
                             routingKey: RoutingKey,
                             basicProperties: null,
                             body: body);
        //Console.WriteLine(" [x] Sent {0}", message);
      }
    }
  }
}
