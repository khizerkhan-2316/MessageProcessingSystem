using MessageProcessingSystem.Producer.Options;
using MessageProcessingSystem.Producer.Services;
using MessageProcessingSystem.UnitTests.Shared.Fakes;

namespace MessageProcessingSystem.Tests.UnitTests.Producer;

[TestFixture]
public class ProducerServiceTests
{
    private FakeMessagePublisher _publisher = null!;

    [SetUp]
    public void SetUp()
    {
        _publisher = new FakeMessagePublisher();
    }

    [Test]
    public async Task ProduceOnceAsync_ShouldPublishOneMessage()
    {
        var options = new ProducerOptions
        {
            MinDelayMilliseconds = 1000,
            MaxDelayMilliseconds = 5000
        };

        var service = new ProducerService(_publisher, options);

        await service.ProduceOnceAsync();

        Assert.That(_publisher.PublishedMessages, Has.Count.EqualTo(1));
        Assert.That(_publisher.PublishedMessages[0].Counter, Is.EqualTo(0));
        Assert.That(_publisher.PublishedMessages[0].Timestamp, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public async Task RunAsync_ShouldPublishMessagesUntilCancelled()
    {
        var options = new ProducerOptions
        {
            MinDelayMilliseconds = 1,
            MaxDelayMilliseconds = 2
        };

        var service = new ProducerService(_publisher, options);

        using var cts = new CancellationTokenSource();

        var runTask = service.RunAsync(cts.Token);

        await Task.Delay(20);

        cts.Cancel();

        try
        {
            await runTask;
        }
        catch (TaskCanceledException)
        {

        }

        Assert.That(_publisher.PublishedMessages.Count, Is.GreaterThanOrEqualTo(1));
    }
}