using MessageProcessingSystem.Shared.Models;

namespace MessageProcessingSystem.Consumer.Interfaces;

public interface IMessageProcessingService
{
    Task ProcessAsync(Message message);
}