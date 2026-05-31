using System.Text;
using System.Text.Json;
using MessageProcessingSystem.Consumer.Interfaces;
using MessageProcessingSystem.Shared.Models;
using MessageProcessingSystem.Shared.Options;
using RabbitMQ.Client;

namespace MessageProcessingSystem.Consumer.RabbitMq;

public class RabbitMqMessageReceiver : IMessageReceiver
{
	private readonly RabbitMqOptions _rabbitMqOptions;

	public RabbitMqMessageReceiver(RabbitMqOptions rabbitMqOptions)
	{
		_rabbitMqOptions = rabbitMqOptions;
	}

	public Task<Message?> ReceiveAsync()
	{
		var factory = new ConnectionFactory
		{
			HostName = _rabbitMqOptions.HostName,
			UserName = _rabbitMqOptions.UserName,
			Password = _rabbitMqOptions.Password
		};

		using var connection = factory.CreateConnection();
		using var channel = connection.CreateModel();

		channel.QueueDeclare(
			queue: _rabbitMqOptions.QueueName,
			durable: true,
			exclusive: false,
			autoDelete: false,
			arguments: null);

		var result = channel.BasicGet(
			queue: _rabbitMqOptions.QueueName,
			autoAck: true);

		if (result is null)
		{
			return Task.FromResult<Message?>(null);
		}

		var body = result.Body.ToArray();
		var json = Encoding.UTF8.GetString(body);

		var message = JsonSerializer.Deserialize<Message>(json);

		return Task.FromResult(message);
	}
}