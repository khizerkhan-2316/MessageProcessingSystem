using MessageProcessingSystem.Shared.Models;

namespace MessageProcessingSystem.Consumer.Interfaces;

public interface IMessageReceiver
{
    Task<Message?> ReceiveAsync();
}