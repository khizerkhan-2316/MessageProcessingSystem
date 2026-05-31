using MessageProcessingSystem.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessingSystem.Shared.Interfaces
{
    public interface IMessagePublisher
    {
        public Task PublishAsync(Message message);
    }
}
