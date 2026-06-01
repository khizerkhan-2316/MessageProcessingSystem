using MessageProcessingSystem.Consumer.DataAccess;
using MessageProcessingSystem.Consumer.Interfaces;
using MessageProcessingSystem.Consumer.Options;
using MessageProcessingSystem.Consumer.RabbitMq;
using MessageProcessingSystem.Consumer.Services;
using MessageProcessingSystem.Shared.Interfaces;
using MessageProcessingSystem.Shared.Options;
using MessageProcessingSystem.Shared.Messaging;
using Microsoft.EntityFrameworkCore;

var rabbitMqOptions = new RabbitMqOptions
{
    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
    QueueName = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE") ?? "message_queue",
    UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest",
    Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
};

var databaseOptions = new DatabaseOptions
{
    ConnectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
        ?? "Server=message-mssql,1433;Database=MessageProcessingSystem;User Id=sa;Password=Cefemivo+f113;TrustServerCertificate=True;"
};

var dbContextOptions = new DbContextOptionsBuilder<MessageDbContext>()
    .UseSqlServer(databaseOptions.ConnectionString)
    .Options;

await using var dbContext = new MessageDbContext(dbContextOptions);


await dbContext.Database.EnsureCreatedAsync();

IMessageReceiver messageReceiver = new RabbitMqMessageReceiver(rabbitMqOptions);
IMessagePublisher messagePublisher = new RabbitMqMessagePublisher(rabbitMqOptions);
IMessageRepository messageRepository = new DatabaseMessageRepository(dbContext);

IMessageProcessingService messageProcessingService = new MessageProcessingService(
    messagePublisher,
    messageRepository);

IConsumerService consumerService = new ConsumerService(
    messageReceiver,
    messageProcessingService);

Console.WriteLine("Consumer started.");
Console.WriteLine($"RabbitMQ host: {rabbitMqOptions.HostName}");
Console.WriteLine($"RabbitMQ queue: {rabbitMqOptions.QueueName}");
Console.WriteLine("Database provider: SQL Server");

while (true)
{
    try
    {
        await consumerService.RunAsync();
    }
    catch (Exception exception)
    {
        Console.WriteLine($"Consumer error: {exception.Message}");
    }

    await Task.Delay(1000);
}