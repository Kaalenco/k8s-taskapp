using Microsoft.Extensions.Logging;
using Moq;
using TaskApp.Application.Services;

namespace TaskApp.UnitTests;

#pragma warning disable CA1874 // using of literals

// This unit test class verifies the functionality of the LoggerHelper class used in the TaskService.
// It ensures that the logging methods are called correctly when retrieving, creating, and updating tasks.
// The tests use a Moq mock logger to verify that log calls are made with the correct level, event ID,
// and message content. This helps ensure that the logging behavior of the TaskService is working as
// intended, providing valuable insights into the application's operations and aiding in debugging.
public class WithLoggerhelper
{
    private Mock<ILogger> _mockLogger = null!;
    private LoggerHelper _sut = null!;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger>();
        _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _sut = LoggerHelper.For(_mockLogger.Object, "Task");
    }

    [Test]
    public void ItemCreating_LogsDebugWithDescription()
    {
        _sut.ItemCreating("Buy milk");

        VerifyLog(LogLevel.Debug, new EventId(10105),
            msg => msg.Contains("Task") && msg.Contains("Buy milk"));
    }

    [Test]
    public void ItemCreated_LogsDebugWithItemId()
    {
        _sut.ItemCreated(42);

        VerifyLog(LogLevel.Debug, new EventId(10104),
            msg => msg.Contains("Task") && msg.Contains("42"));
    }

    [Test]
    public void CouldNotCreate_LogsErrorWithExceptionAndDescription()
    {
        var ex = new InvalidOperationException("db error");
        _sut.CouldNotCreate(ex, "duplicate title");

        VerifyLog(LogLevel.Error, new EventId(30101),
            msg => msg.Contains("Task") && msg.Contains("duplicate title"),
            exception: ex);
    }

    [Test]
    public void CouldNotCreate_WithNullException_LogsError()
    {
        _sut.CouldNotCreate(null, "bad input");

        VerifyLog(LogLevel.Error, new EventId(30101),
            msg => msg.Contains("bad input"),
            exception: null);
    }

    [Test]
    public void ItemModifying_LogsDebugWithItemId()
    {
        _sut.ItemModifying(7);

        VerifyLog(LogLevel.Debug, new EventId(10106),
            msg => msg.Contains("Task") && msg.Contains('7'));
    }

    [Test]
    public void ItemModified_LogsDebugWithItemId()
    {
        _sut.ItemModified(7);

        VerifyLog(LogLevel.Debug, new EventId(10107),
            msg => msg.Contains("Task") && msg.Contains('7'));
    }

    [Test]
    public void ItemRetrieving_LogsDebugWithItemId()
    {
        _sut.ItemRetrieving(3);

        VerifyLog(LogLevel.Debug, new EventId(10102),
            msg => msg.Contains("Task") && msg.Contains('3'));
    }

    [Test]
    public void FindItems_LogsDebugWithFilter()
    {
        _sut.FindItems("status=open");

        VerifyLog(LogLevel.Debug, new EventId(10103),
            msg => msg.Contains("Task") && msg.Contains("status=open"));
    }

    [Test]
    public void Error_LogsErrorWithExceptionAndDescription()
    {
        var ex = new TimeoutException("timeout");
        _sut.Error(ex, "failed to save", 99);

        VerifyLog(LogLevel.Error, new EventId(30102),
            msg => msg.Contains("Task") && msg.Contains("99") && msg.Contains("failed to save"),
            exception: ex);
    }

    [Test]
    public void ItemDeleting_LogsDebugWithItemId()
    {
        _sut.ItemDeleting(5);

        VerifyLog(LogLevel.Debug, new EventId(10108),
            msg => msg.Contains("Task") && msg.Contains('5'));
    }

    [Test]
    public void ItemDeleted_LogsDebugWithItemId()
    {
        _sut.ItemDeleted(5);

        VerifyLog(LogLevel.Debug, new EventId(10109),
            msg => msg.Contains("Task") && msg.Contains('5'));
    }

    [Test]
    public void ItemNotFound_LogsWarningWithItemId()
    {
        _sut.ItemNotFound(12);

        VerifyLog(LogLevel.Warning, new EventId(20100),
            msg => msg.Contains("Task") && msg.Contains("12"));
    }

    [Test]
    public void For_UsesSuppliedItemType()
    {
        var helper = LoggerHelper.For(_mockLogger.Object, "Order");
        helper.ItemNotFound(1);

        VerifyLog(LogLevel.Warning, new EventId(20100),
            msg => msg.Contains("Order"));
    }

    // Helper: verifies ILogger.Log was called once with matching level, eventId, message, and exception.
    private void VerifyLog(
        LogLevel level,
        EventId eventId,
        Func<string, bool> messageMatch,
        Exception? exception = null)
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                eventId,
                It.Is<It.IsAnyType>((state, _) => messageMatch(state.ToString()!)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
