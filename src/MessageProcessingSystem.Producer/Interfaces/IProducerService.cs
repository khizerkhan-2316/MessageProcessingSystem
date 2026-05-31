using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessingSystem.Producer.Interfaces
{
    internal interface IProducerService
    {
        public Task RunAsync(CancellationToken cancellationToken);
    }
}
