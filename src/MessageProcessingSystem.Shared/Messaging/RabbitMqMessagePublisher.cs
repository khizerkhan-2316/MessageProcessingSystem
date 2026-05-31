using System.Text;
using System.Text.Json;
using MessageProcessingSystem.Shared.Interfaces;
using MessageProcessingSystem.Shared.Models;
using MessageProcessingSystem.Shared.Options;
using RabbitMQ.Client;

namespace MessageProcessingSystem.Shared.Messaging;

public class RabbitMqMessagePublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqMessagePublisher(RabbitMqOptions options)
    {
        _options = options;
    }

    public async Task PublishAsync(Message message)
    {
        if (_connection is null || _channel is null)
        {
            await InitializeAsync();
        }

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await _channel!.BasicPublishAsync(
            exchange: "",
            routingKey: _options.QueueName,
            mandatory: false,
            body: body);
    }

    private async Task InitializeAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        const int maxRetries = 10;
        const int delayMilliseconds = 3000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.QueueDeclareAsync(
                    queue: _options.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                Console.WriteLine("Connected to RabbitMQ");
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                Console.WriteLine(
                    $"RabbitMQ not ready. Retrying {attempt}/{maxRetries}... {ex.Message}");

                await Task.Delay(delayMilliseconds);
            }
        }

        throw new Exception("Could not connect to RabbitMQ after retries.");
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