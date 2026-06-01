namespace MessageProcessingSystem.Shared.Options;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string QueueName { get; set; } = "message_queue";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}