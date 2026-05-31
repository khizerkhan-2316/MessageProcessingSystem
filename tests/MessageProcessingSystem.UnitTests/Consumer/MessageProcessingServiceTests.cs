using MessageProcessingSystem.Consumer.DataAccess;
using MessageProcessingSystem.Consumer.Services;
using MessageProcessingSystem.Shared.Interfaces;
using MessageProcessingSystem.Shared.Models;
using NSubstitute;
using NUnit.Framework;

namespace MessageProcessingSystem.UnitTests.Consumer;

[TestFixture]
public class MessageProcessingServiceTests
{
    private IMessagePublisher _messagePublisher = null!;
    private IMessageRepository _messageRepository = null!;
    private FixedTimeProvider _timeProvider = null!;
    private MessageProcessingService _uut = null!;

    private readonly DateTime _now = new(2026, 1, 1, 12, 10, 10, DateTimeKind.Utc);

    [SetUp]
    public void SetUp()
    {
        _messagePublisher = Substitute.For<IMessagePublisher>();
        _messageRepository = Substitute.For<IMessageRepository>();
        _timeProvider = new FixedTimeProvider(_now);

        _uut = new MessageProcessingService(
            _messagePublisher,
            _messageRepository,
            _timeProvider);
    }

    // Testkoncept: Black-box test + ZOMBIE "Zero/null"
    // Vi tester ugyldigt input. En null-message skal afvises.
    [Test]
    public void ProcessAsync_WhenMessageIsNull_ShouldThrowArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _uut.ProcessAsync(null!));
    }

    // Testkoncept: Boundary Value Analysis
    // 61 sekunder er over 1 minut og skal derfor kasseres.
    [Test]
    public async Task ProcessAsync_WhenMessageIsOlderThanOneMinute_ShouldDiscardMessage()
    {
        var message = CreateMessage(_now.AddSeconds(-61), counter: 5);

        await _uut.ProcessAsync(message);

        await _messageRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<Message>());

        await _messagePublisher
            .DidNotReceive()
            .PublishAsync(Arg.Any<Message>());
    }

    // Testkoncept: Boundary Value Analysis
    // Præcis 60 sekunder er ikke over 1 minut.
    // Sekundet er 10, altså lige, så beskeden skal gemmes.
    [Test]
    public async Task ProcessAsync_WhenMessageIsExactlyOneMinuteOldAndSecondIsEven_ShouldSaveMessage()
    {
        var message = CreateMessage(_now.AddSeconds(-60), counter: 3);

        await _uut.ProcessAsync(message);

        await _messageRepository
            .Received(1)
            .SaveAsync(Arg.Is<Message>(m =>
                m.Id == message.Id &&
                m.Timestamp == message.Timestamp &&
                m.Counter == 3));

        await _messagePublisher
            .DidNotReceive()
            .PublishAsync(Arg.Any<Message>());
    }

    // Testkoncept: Boundary Value Analysis
    // 59 sekunder er lige under grænsen.
    // Sekundet er lige, så beskeden skal gemmes.
    [Test]
    public async Task ProcessAsync_WhenMessageIsJustUnderOneMinuteOldAndSecondIsEven_ShouldSaveMessage()
    {
        var timestamp = new DateTime(2026, 1, 1, 12, 9, 12, DateTimeKind.Utc);
        var message = CreateMessage(timestamp, counter: 2);

        await _uut.ProcessAsync(message);

        await _messageRepository
            .Received(1)
            .SaveAsync(Arg.Is<Message>(m =>
                m.Id == message.Id &&
                m.Timestamp == message.Timestamp &&
                m.Counter == 2));

        await _messagePublisher
            .DidNotReceive()
            .PublishAsync(Arg.Any<Message>());
    }

    // Testkoncept: Equivalence Partitioning
    // Besked under 1 minut + lige sekund => gem i database.
    [Test]
    public async Task ProcessAsync_WhenMessageIsUnderOneMinuteOldAndSecondIsEven_ShouldSaveMessage()
    {
        var timestamp = new DateTime(2026, 1, 1, 12, 9, 20, DateTimeKind.Utc);
        var message = CreateMessage(timestamp, counter: 7);

        await _uut.ProcessAsync(message);

        await _messageRepository
            .Received(1)
            .SaveAsync(Arg.Is<Message>(m =>
                m.Id == message.Id &&
                m.Timestamp == message.Timestamp &&
                m.Counter == 7));

        await _messagePublisher
            .DidNotReceive()
            .PublishAsync(Arg.Any<Message>());
    }

    // Testkoncept: Equivalence Partitioning
    // Besked under 1 minut + ulige sekund => requeue med counter + 1.
    [Test]
    public async Task ProcessAsync_WhenMessageIsUnderOneMinuteOldAndSecondIsOdd_ShouldRequeueWithIncrementedCounter()
    {
        var timestamp = new DateTime(2026, 1, 1, 12, 9, 11, DateTimeKind.Utc);
        var message = CreateMessage(timestamp, counter: 4);

        await _uut.ProcessAsync(message);

        await _messagePublisher
            .Received(1)
            .PublishAsync(Arg.Is<Message>(m =>
                m.Id == message.Id &&
                m.Timestamp == message.Timestamp &&
                m.Counter == 5));

        await _messageRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<Message>());
    }

    // Testkoncept: ZOMBIE - Zero
    [Test]
    public async Task ProcessAsync_WhenOddSecondAndCounterIsZero_ShouldRequeueWithCounterOne()
    {
        var timestamp = new DateTime(2026, 1, 1, 12, 9, 11, DateTimeKind.Utc);
        var message = CreateMessage(timestamp, counter: 0);

        await _uut.ProcessAsync(message);

        await _messagePublisher
            .Received(1)
            .PublishAsync(Arg.Is<Message>(m => m.Counter == 1));
    }

    // Testkoncept: ZOMBIE - One
    [Test]
    public async Task ProcessAsync_WhenOddSecondAndCounterIsOne_ShouldRequeueWithCounterTwo()
    {
        var timestamp = new DateTime(2026, 1, 1, 12, 9, 11, DateTimeKind.Utc);
        var message = CreateMessage(timestamp, counter: 1);

        await _uut.ProcessAsync(message);

        await _messagePublisher
            .Received(1)
            .PublishAsync(Arg.Is<Message>(m => m.Counter == 2));
    }

    // Testkoncept: ZOMBIE - Many
    [Test]
    public async Task ProcessAsync_WhenOddSecondAndCounterIsMany_ShouldRequeueWithCounterIncremented()
    {
        var timestamp = new DateTime(2026, 1, 1, 12, 9, 11, DateTimeKind.Utc);
        var message = CreateMessage(timestamp, counter: 41);

        await _uut.ProcessAsync(message);

        await _messagePublisher
            .Received(1)
            .PublishAsync(Arg.Is<Message>(m => m.Counter == 42));
    }

    // Testkoncept: Black-box + branch awareness
    // Gammel besked skal kasseres uanset om sekundet er lige.
    [Test]
    public async Task ProcessAsync_WhenMessageIsOldAndSecondIsEven_ShouldDiscardMessage()
    {
        var timestamp = new DateTime(2026, 1, 1, 12, 8, 10, DateTimeKind.Utc);
        var message = CreateMessage(timestamp, counter: 1);

        await _uut.ProcessAsync(message);

        await _messageRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<Message>());

        await _messagePublisher
            .DidNotReceive()
            .PublishAsync(Arg.Any<Message>());
    }

    // Testkoncept: Dependency test + exception path
    [Test]
    public void ProcessAsync_WhenRepositoryFails_ShouldPropagateException()
    {
        var timestamp = new DateTime(2026, 1, 1, 12, 9, 20, DateTimeKind.Utc);
        var message = CreateMessage(timestamp, counter: 1);

        _messageRepository
            .SaveAsync(Arg.Any<Message>())
            .Returns<Task>(_ => throw new InvalidOperationException("Database failed"));

        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _uut.ProcessAsync(message));

        Assert.That(exception!.Message, Is.EqualTo("Database failed"));
    }

    // Testkoncept: Dependency test + exception path
    [Test]
    public void ProcessAsync_WhenPublisherFails_ShouldPropagateException()
    {
        var timestamp = new DateTime(2026, 1, 1, 12, 9, 11, DateTimeKind.Utc);
        var message = CreateMessage(timestamp, counter: 1);

        _messagePublisher
            .PublishAsync(Arg.Any<Message>())
            .Returns<Task>(_ => throw new InvalidOperationException("RabbitMQ failed"));

        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _uut.ProcessAsync(message));

        Assert.That(exception!.Message, Is.EqualTo("RabbitMQ failed"));
    }

    private static Message CreateMessage(DateTime timestamp, int counter)
    {
        return new Message
        {
            Id = Guid.NewGuid(),
            Timestamp = timestamp,
            Counter = counter
        };
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTime utcNow)
        {
            _utcNow = new DateTimeOffset(utcNow, TimeSpan.Zero);
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }
}