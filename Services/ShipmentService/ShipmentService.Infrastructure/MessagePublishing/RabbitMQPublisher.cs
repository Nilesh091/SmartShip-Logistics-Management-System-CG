using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ShipmentService.Infrastructure.MessagePublishing;

public class RabbitMQPublisher : IAsyncDisposable
{
  private readonly string _hostname;
  private IConnection? _connection;
  private IChannel? _channel;

  public RabbitMQPublisher(string hostname = "localhost")
  {
    _hostname = hostname;
  }

  private async Task EnsureConnectionAsync()
  {
    try
    {
      if (_connection == null || !_connection.IsOpen)
      {
        var factory = new ConnectionFactory
        {
          HostName = _hostname,
          AutomaticRecoveryEnabled = true,
          NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = factory.CreateConnectionAsync().Result;
      }

      if (_channel == null || _channel.IsClosed)
      {
        _channel = _connection!.CreateChannelAsync().Result;
      }
    }
    catch (Exception ex)
    {
      throw new InvalidOperationException($"Failed to connect to RabbitMQ at {_hostname}", ex);
    }
  }

  public async Task PublishAsync<T>(string queueName, T message) where T : class
  {
    await EnsureConnectionAsync();

    try
    {
      // Convert PascalCase to kebab-case for exchange and routing key
      // Example: ShipmentStatusChangedEvent -> shipment.status.changed
      var exchange = "shipment-events";
      var routingKey = ConvertToKebabCase(typeof(T).Name.Replace("Event", ""));

      // Declare the exchange (idempotent)
      await _channel!.ExchangeDeclareAsync(
          exchange: exchange,
          type: "topic",
          durable: true,
          autoDelete: false,
          arguments: null
      );

      var json = JsonSerializer.Serialize(message);
      var body = Encoding.UTF8.GetBytes(json);

      await _channel.BasicPublishAsync(
          exchange: exchange,
          routingKey: routingKey,
          mandatory: false,
          body: body
      );
    }
    catch (Exception ex)
    {
      throw new InvalidOperationException($"Failed to publish message of type '{typeof(T).Name}'", ex);
    }
  }

  private string ConvertToKebabCase(string input)
  {
    // Convert PascalCase to kebab-case
    // ShipmentStatusChanged -> shipment.status.changed
    var result = System.Text.RegularExpressions.Regex.Replace(
        input,
        "(?<!^)([A-Z])",
        ".$1"
    ).ToLower();
    return result;
  }

  public async ValueTask DisposeAsync()
  {
    if (_channel != null && _channel.IsOpen)
    {
      await _channel.CloseAsync();
      _channel.Dispose();
    }

    if (_connection != null && _connection.IsOpen)
    {
      await _connection.CloseAsync();
      _connection.Dispose();
    }
  }
}
