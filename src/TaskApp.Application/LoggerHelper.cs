using Microsoft.Extensions.Logging;

namespace TaskApp.Application.Services;

public partial class LoggerHelper
{

    private readonly ILogger _logger;
    private readonly string _itemType;

    public LoggerHelper(ILogger logger, string itemType)
    {
        _logger = logger;
        _itemType = itemType;
    }

    public static LoggerHelper For(ILogger logger, string itemType) => new LoggerHelper(logger, itemType);

    public void ItemCreating(string description)
    {
        LogItemCreating(_logger, _itemType, description);
    }

    public void CouldNotCreate(Exception? exception, string description)
    {
        LogCouldNotCreate(_logger, exception, _itemType, description);
    }

    public void ItemCreated(int itemId)
    {
        LogItemCreated(_logger, _itemType, itemId);
    }

    public void ItemModifying(int itemId)
    {
        LogItemModifying(_logger, _itemType, itemId);
    }

    public void ItemModified(int itemId)
    {
        LogItemModified(_logger, _itemType, itemId);
    }

    public void ItemRetrieving(int itemId)
    {
        LogItemReading(_logger, _itemType, itemId);
    }

    public void FindItems(string filter)
    {
        LogFindItems(_logger, _itemType, filter);
    }

    public void Error(Exception? exception, string description, int itemId)
    {
        LogErrorDescription(_logger, exception, _itemType, itemId, description);
    }

    public void ItemDeleting(int itemId)
    {
        LogItemDeleting(_logger, _itemType, itemId);
    }

    public void ItemDeleted(int itemId)
    {
        LogItemDeleted(_logger, _itemType, itemId);
    }

    public void ItemNotFound( int itemId)
    {
        LogItemNotFound(_logger, _itemType, itemId);
    }

    [LoggerMessage(Level = LogLevel.Warning, EventId = 20100, Message = "{itemType} with ID {itemId} not found")]
    private static partial void LogItemNotFound(ILogger logger, string itemType, int itemId);

    [LoggerMessage(Level = LogLevel.Error, EventId = 30101, Message = "Could not create {itemType}: {description}")]
    private static partial void LogCouldNotCreate(ILogger logger, Exception? exception, string itemType, string description);

    [LoggerMessage(Level = LogLevel.Error, EventId = 30102, Message = "An error occured with {itemType}[{itemId}] : {description}")]
    private static partial void LogErrorDescription(ILogger logger, Exception? exception, string itemType, int itemId, string description);

    [LoggerMessage(Level = LogLevel.Debug, EventId = 10102, Message = "Reading {itemType} with ID {itemId}")]
    private static partial void LogItemReading(ILogger logger, string itemType, int itemId);

    [LoggerMessage(Level = LogLevel.Debug, EventId = 10103, Message = "Reading from {itemType} with filter [{filter}]")]
    private static partial void LogFindItems(ILogger logger, string itemType, string filter);

    [LoggerMessage(Level = LogLevel.Debug, EventId = 10104, Message = "Created new {itemType} with ID {itemId}")]
    private static partial void LogItemCreated(ILogger logger, string itemType, int itemId);

    [LoggerMessage(Level = LogLevel.Debug, EventId = 10105, Message = "Creating new {itemType}: {description}")]
    private static partial void LogItemCreating(ILogger logger, string itemType, string description);

    [LoggerMessage(Level = LogLevel.Debug, EventId = 10106, Message = "Modifying {itemType} with ID {itemId}")]
    private static partial void LogItemModifying(ILogger logger, string itemType, int itemId);

    [LoggerMessage(Level = LogLevel.Debug, EventId = 10107, Message = "Modified {itemType} with ID {itemId}")]
    private static partial void LogItemModified(ILogger logger, string itemType, int itemId);

    [LoggerMessage(Level = LogLevel.Debug, EventId = 10108, Message = "Deleting {itemType} with ID {itemId}")]
    private static partial void LogItemDeleting(ILogger logger, string itemType, int itemId);

    [LoggerMessage(Level = LogLevel.Debug, EventId = 10109, Message = "Deleted {itemType} with ID {itemId}")]
    private static partial void LogItemDeleted(ILogger logger, string itemType, int itemId);
}
