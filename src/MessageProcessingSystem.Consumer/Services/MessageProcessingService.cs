using MessageProcessingSystem.Consumer.DataAccess;
using MessageProcessingSystem.Consumer.Interfaces;
using MessageProcessingSystem.Shared.Interfaces;
using MessageProcessingSystem.Shared.Models;

namespace MessageProcessingSystem.Consumer.Services;

public class MessageProcessingService : IMessageProcessingService
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMessageRepository _messageRepository;
    private readonly TimeProvider _timeProvider;

    public MessageProcessingService(
        IMessagePublisher messagePublisher,
        IMessageRepository messageRepository,
        TimeProvider? timeProvider = null)
    {
        _messagePublisher = messagePublisher;
        _messageRepository = messageRepository;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task ProcessAsync(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (IsOlderThanOneMinute(message))
        {
            return;
        }

        if (HasEvenSeconds(message))
        {
            await _messageRepository.SaveAsync(message);
            return;
        }

        message.Counter++;
        await _messagePublisher.PublishAsync(message);
    }

    private bool IsOlderThanOneMinute(Message message)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var timestamp = message.Timestamp.ToUniversalTime();

        return now - timestamp > TimeSpan.FromMinutes(1);
    }

    private static bool HasEvenSeconds(Message message)
    {
        return message.Timestamp.Second % 2 == 0;
    }
}