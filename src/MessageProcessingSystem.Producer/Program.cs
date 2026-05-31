using DotNetEnv;
using MessageProcessingSystem.Producer.Interfaces;
using MessageProcessingSystem.Producer.Options;
using MessageProcessingSystem.Producer.Services;
using MessageProcessingSystem.Shared.Interfaces;
using MessageProcessingSystem.Shared.Messaging;
using MessageProcessingSystem.Shared.Options;
using Microsoft.Extensions.DependencyInjection;

namespace MessageProcessingSystem.Producer;

internal class Program
{
    static async Task Main(string[] args)
    {
        Env.Load();

        var services = new ServiceCollection();

        services.AddSingleton(new ProducerOptions
        {
            MinDelayMilliseconds = 1000,
            MaxDelayMilliseconds = 5000
        });

        services.AddSingleton(new RabbitMqOptions
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
            QueueName = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE") ?? "message_queue",
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS") ?? "guest"
        });

        services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();
        services.AddSingleton<IProducerService, ProducerService>();

        var serviceProvider = services.BuildServiceProvider();

        var producerService =
            serviceProvider.GetRequiredService<IProducerService>();

        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        Console.WriteLine("Producer started");

        await producerService.RunAsync(cts.Token);
    }
}