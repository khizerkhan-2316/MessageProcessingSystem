using MessageProcessingSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace MessageProcessingSystem.Consumer.DataAccess;

public class MessageDbContext : DbContext
{
    public MessageDbContext(DbContextOptions<MessageDbContext> options)
        : base(options)
    {
    }

    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages");

            entity.HasKey(message => message.Id);

            entity.Property(message => message.Id)
                .IsRequired();

            entity.Property(message => message.Timestamp)
                .IsRequired();

            entity.Property(message => message.Counter)
                .IsRequired();
        });
    }
}