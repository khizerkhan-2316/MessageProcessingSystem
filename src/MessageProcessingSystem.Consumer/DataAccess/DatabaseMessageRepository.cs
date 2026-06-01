using MessageProcessingSystem.Shared.Models;

namespace MessageProcessingSystem.Consumer.DataAccess;

public class DatabaseMessageRepository : IMessageRepository
{
    private readonly MessageDbContext _dbContext;

    public DatabaseMessageRepository(MessageDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveAsync(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        await _dbContext.Messages.AddAsync(message);
        await _dbContext.SaveChangesAsync();
    }
}