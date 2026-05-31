using MessageProcessingSystem.Producer.Interfaces;
using MessageProcessingSystem.Producer.Options;
using MessageProcessingSystem.Shared.Interfaces;
using MessageProcessingSystem.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessingSystem.Producer.Services
{
    public class ProducerService : IProducerService
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly ProducerOptions _producerOptions;

        public ProducerService(
            IMessagePublisher messagePublisher,
            ProducerOptions producerOptions)
        {
            _messagePublisher = messagePublisher;
            _producerOptions = producerOptions;
        }
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {

                await ProduceOnceAsync();

                var delay = Random.Shared.Next(
                    _producerOptions.MinDelayMilliseconds,
                    _producerOptions.MaxDelayMilliseconds);

                await Task.Delay(delay, cancellationToken);
            }
        }

        public async Task ProduceOnceAsync()
        {
            var message = CreateMessage();

            await _messagePublisher.PublishAsync(message);
        }

        private Message CreateMessage()
        {
            return new Message
            {
                Timestamp = DateTime.UtcNow,
                Counter = 0
            };

        }


    }
}
