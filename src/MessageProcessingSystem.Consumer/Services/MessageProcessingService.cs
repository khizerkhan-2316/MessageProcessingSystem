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

        //Here is the question? - Should the timestamp be updated?
        //because the messages that gets requeded don't get stored in db, as the timestamp is uneven second still.
        // should it be updated?

        // We update the timestamp on requeue so the message gets a new second value,
        // allowing it to eventually land on an even second and be stored with counter > 0
        message.Timestamp = _timeProvider.GetUtcNow().UtcDateTime;

        await _messagePublisher.PublishAsync(message);
    }

    private bool IsOlderThanOneMinute(Message message)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var timestamp = DateTime.SpecifyKind(message.Timestamp, DateTimeKind.Utc);
        return now - timestamp > TimeSpan.FromMinutes(1);
    }

    private static bool HasEvenSeconds(Message message)
    {
        return message.Timestamp.Second % 2 == 0;
    }
}