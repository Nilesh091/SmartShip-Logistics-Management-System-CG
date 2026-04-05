using Microsoft.Extensions.Logging;
using ShipmentService.Application.MessagePublishing;

namespace ShipmentService.Infrastructure.MessagePublishing;

public class MessagePublisher : IMessagePublisher
{
  private readonly RabbitMQPublisher _rabbitMQPublisher;
  private readonly ILogger<MessagePublisher>? _logger;

  public MessagePublisher(string hostname = "localhost", ILogger<MessagePublisher>? logger = null)
  {
    _rabbitMQPublisher = new RabbitMQPublisher(hostname);
    _logger = logger;
  }

  public async Task PublishAsync<T>(T message) where T : class
  {
    try
    {
      var queueName = GetQueueName<T>();
      _logger?.LogInformation("Publishing message of type {MessageType} to queue {QueueName}", typeof(T).Name, queueName);

      await _rabbitMQPublisher.PublishAsync(queueName, message);

      _logger?.LogInformation("Message published successfully to queue {QueueName}", queueName);
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, "Failed to publish message of type {MessageType}", typeof(T).Name);
      throw;
    }
  }

  private string GetQueueName<T>() where T : class
  {
    var typeName = typeof(T).Name;

    // Convert PascalCase to kebab-case and add -events suffix
    var queueName = System.Text.RegularExpressions.Regex.Replace(
        typeName,
        "(?<!^)([A-Z])",
        "-$1"
    ).ToLower();

    return $"{queueName}-events";
  }
}
