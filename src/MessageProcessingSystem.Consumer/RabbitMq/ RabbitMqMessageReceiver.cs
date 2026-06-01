using System.Text;
using System.Text.Json;
using MessageProcessingSystem.Consumer.Interfaces;
using MessageProcessingSystem.Shared.Models;
using MessageProcessingSystem.Shared.Options;
using RabbitMQ.Client;

namespace MessageProcessingSystem.Consumer.RabbitMq;

public class RabbitMqMessageReceiver : IMessageReceiver, IAsyncDisposable
{
    private readonly RabbitMqOptions _rabbitMqOptions;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqMessageReceiver(RabbitMqOptions rabbitMqOptions)
    {
        _rabbitMqOptions = rabbitMqOptions;
    }

    public async Task<Message?> ReceiveAsync()
    {
        if (_connection is null || _channel is null)
        {
            await InitializeAsync();
        }

        var result = await _channel!.BasicGetAsync(
            queue: _rabbitMqOptions.QueueName,
            autoAck: true);

        if (result is null)
        {
            return null;
        }

        var body = result.Body.ToArray();
        var json = Encoding.UTF8.GetString(body);

        return JsonSerializer.Deserialize<Message>(json);
    }

    private async Task InitializeAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqOptions.HostName,
            Port = _rabbitMqOptions.Port,
            UserName = _rabbitMqOptions.UserName,
            Password = _rabbitMqOptions.Password
        };

        const int maxRetries = 10;
        const int delayMilliseconds = 3000;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.QueueDeclareAsync(
                    queue: _rabbitMqOptions.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                Console.WriteLine("Consumer connected to RabbitMQ.");
                return;
            }
            catch (Exception exception) when (attempt < maxRetries)
            {
                Console.WriteLine(
                    $"RabbitMQ not ready. Retrying {attempt}/{maxRetries}... {exception.Message}");

                await Task.Delay(delayMilliseconds);
            }
        }

        throw new InvalidOperationException("Could not connect to RabbitMQ after retries.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}