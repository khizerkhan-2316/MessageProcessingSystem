using MessageProcessingSystem.Consumer.Interfaces;
using MessageProcessingSystem.Consumer.Services;
using MessageProcessingSystem.Shared.Models;
using NSubstitute;
using NUnit.Framework;

namespace MessageProcessingSystem.UnitTests.Consumer;

[TestFixture]
public class ConsumerServiceTests
{
    private IMessageReceiver _messageReceiver = null!;
    private IMessageProcessingService _messageProcessingService = null!;
    private ConsumerService _uut = null!;

    [SetUp]
    public void SetUp()
    {
        _messageReceiver = Substitute.For<IMessageReceiver>();
        _messageProcessingService = Substitute.For<IMessageProcessingService>();

        _uut = new ConsumerService(
            _messageReceiver,
            _messageProcessingService);
    }

    // Testkoncept: Funktionel black-box test
    // Når ConsumerService modtager en besked, skal den sendes videre
    // til IMessageProcessingService.
    [Test]
    public async Task RunAsync_WhenMessageIsReceived_ShouldProcessMessage()
    {
        var message = new Message
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Counter = 1
        };

        _messageReceiver
            .ReceiveAsync()
            .Returns(message);

        await _uut.RunAsync();

        await _messageProcessingService
            .Received(1)
            .ProcessAsync(Arg.Is<Message>(m =>
                m.Id == message.Id &&
                m.Timestamp == message.Timestamp &&
                m.Counter == message.Counter));
    }

    // Testkoncept: Collaboration test
    // Vi tester at ConsumerService kalder receiveren præcis én gang,
    // når én besked skal behandles.
    [Test]
    public async Task RunAsync_ShouldReceiveExactlyOneMessage()
    {
        var message = new Message
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Counter = 1
        };

        _messageReceiver
            .ReceiveAsync()
            .Returns(message);

        await _uut.RunAsync();

        await _messageReceiver
            .Received(1)
            .ReceiveAsync();
    }

    // Testkoncept: ZOMBIE - Zero/null
    // Hvis receiveren ikke finder en besked, returnerer den null.
    // ConsumerService skal ikke sende null videre til processing.
    [Test]
    public async Task RunAsync_WhenNoMessageIsReceived_ShouldNotProcessMessage()
    {
        _messageReceiver
            .ReceiveAsync()
            .Returns((Message?)null);

        await _uut.RunAsync();

        await _messageProcessingService
            .DidNotReceive()
            .ProcessAsync(Arg.Any<Message>());
    }

    // Testkoncept: White-box dependency test
    // Hvis receiveren fejler, skal ConsumerService ikke skjule fejlen.
    // Processing må ikke kaldes.
    [Test]
    public void RunAsync_WhenReceiverThrows_ShouldPropagateException()
    {
        _messageReceiver
            .ReceiveAsync()
            .Returns<Task<Message?>>(_ => throw new InvalidOperationException("Receive failed"));

        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _uut.RunAsync());

        Assert.That(exception!.Message, Is.EqualTo("Receive failed"));

        _messageProcessingService
            .DidNotReceive()
            .ProcessAsync(Arg.Any<Message>());
    }

    // Testkoncept: White-box dependency test
    // Hvis processing fejler, skal ConsumerService ikke skjule fejlen.
    [Test]
    public void RunAsync_WhenProcessingThrows_ShouldPropagateException()
    {
        var message = new Message
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Counter = 2
        };

        _messageReceiver
            .ReceiveAsync()
            .Returns(message);

        _messageProcessingService
            .ProcessAsync(Arg.Any<Message>())
            .Returns<Task>(_ => throw new InvalidOperationException("Processing failed"));

        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _uut.RunAsync());

        Assert.That(exception!.Message, Is.EqualTo("Processing failed"));
    }

    // Testkoncept: Black-box collaboration test
    // Den besked, der kommer fra receiveren, skal være den samme,
    // der gives videre til processing-servicen.
    [Test]
    public async Task RunAsync_ShouldPassReceivedMessageToProcessingService()
    {
        var message = new Message
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Counter = 42
        };

        _messageReceiver
            .ReceiveAsync()
            .Returns(message);

        await _uut.RunAsync();

        await _messageProcessingService
            .Received(1)
            .ProcessAsync(Arg.Is<Message>(m =>
                m.Id == message.Id &&
                m.Timestamp == message.Timestamp &&
                m.Counter == message.Counter));
    }
}