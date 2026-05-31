using MessageProcessingSystem.Shared.Interfaces;
using MessageProcessingSystem.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessingSystem.UnitTests.Shared.Fakes
{
    public class FakeMessagePublisher : IMessagePublisher
    {
        public List<Message> PublishedMessages { get; } = new();
        public Task PublishAsync(Message message)
        {
            PublishedMessages.Add(message);
            return Task.CompletedTask;
        }
    }
}
