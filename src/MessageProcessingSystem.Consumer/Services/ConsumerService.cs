using MessageProcessingSystem.Consumer.Interfaces;

namespace MessageProcessingSystem.Consumer.Services;

public class ConsumerService : IConsumerService
{
    private readonly IMessageReceiver _messageReceiver;
    private readonly IMessageProcessingService _messageProcessingService;

    public ConsumerService(
        IMessageReceiver messageReceiver,
        IMessageProcessingService messageProcessingService)
    {
        _messageReceiver = messageReceiver;
        _messageProcessingService = messageProcessingService;
    }

    public async Task RunAsync()
    {
        var message = await _messageReceiver.ReceiveAsync();

        if (message is null)
        {
            return;
        }

        await _messageProcessingService.ProcessAsync(message);
    }
}