using MessageProcessingSystem.Shared.Models;

namespace MessageProcessingSystem.Consumer.DataAccess;

public interface IMessageRepository
{
    Task SaveAsync(Message message);
}