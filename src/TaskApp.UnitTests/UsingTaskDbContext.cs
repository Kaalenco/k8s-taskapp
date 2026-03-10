using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskApp.Api.Data;
using TaskApp.Application.Models;
using TaskApp.Application.Services;

namespace TaskApp.UnitTests;

/// <summary>
/// Integration tests for TaskService against a real MySQL database.
/// The database connection is read from the ConnectionStrings__DefaultConnection environment variable.
/// Tests are tagged [Category("Integration")] and are intended to run via docker-compose
/// (see Dockerfile.integration). They are skipped automatically when no connection string is present.
/// </summary>
public class UsingTaskDbContextWithTaskService
{
    private TaskDbContext _dbContext = null!;
    private ITaskService _taskService = null!;

    [SetUp]
    public void Setup()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            Assert.Ignore("ConnectionStrings__DefaultConnection is not set — skipping integration tests.");

        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseMySQL(connectionString)
            .Options;

        _dbContext = new TaskDbContext(options);
        _dbContext.Database.EnsureCreated();

        // Clean table before each test for isolation
        _dbContext.Tasks.RemoveRange(_dbContext.Tasks);
        _dbContext.SaveChanges();

        var mockLogger = new Mock<ILogger<TaskService>>();
        _taskService = new TaskService(_dbContext, mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    // ── GetAllTasks ──────────────────────────────────────────────────────────

    [Test]
    [Category("Integration")]
    public async Task GetAllTasks_WhenTableIsEmpty_ReturnsEmptyList()
    {
        var result = await _taskService.GetAllTasks();

        Assert.That(result, Is.Empty);
    }

    [Test]
    [Category("Integration")]
    public async Task GetAllTasks_WhenTasksExist_ReturnsAllTasks()
    {
        await _taskService.CreateTask(new TaskModel { Title = "Task A", Description = "Desc A" });
        await _taskService.CreateTask(new TaskModel { Title = "Task B", Description = "Desc B" });

        var result = await _taskService.GetAllTasks();

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    // ── GetTask ──────────────────────────────────────────────────────────────

    [Test]
    [Category("Integration")]
    public async Task GetTask_WhenTaskDoesNotExist_ReturnsNull()
    {
        var result = await _taskService.GetTask(99999);

        Assert.That(result, Is.Null);
    }

    [Test]
    [Category("Integration")]
    public async Task GetTask_WhenTaskExists_ReturnsCorrectTask()
    {
        var created = await _taskService.CreateTask(new TaskModel { Title = "My Task", Description = "Details" });

        var result = await _taskService.GetTask(created!.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Title, Is.EqualTo("My Task"));
        Assert.That(result.Description, Is.EqualTo("Details"));
    }

    // ── CreateTask ───────────────────────────────────────────────────────────

    [Test]
    [Category("Integration")]
    public async Task CreateTask_ReturnsTaskWithAssignedId()
    {
        var result = await _taskService.CreateTask(new TaskModel { Title = "New Task", Description = "Desc" });

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.GreaterThan(0));
        Assert.That(result.Title, Is.EqualTo("New Task"));
    }

    [Test]
    [Category("Integration")]
    public async Task CreateTask_PersiststaskSoItAppearsInGetAll()
    {
        await _taskService.CreateTask(new TaskModel { Title = "Persisted Task", Description = "" });

        var all = await _taskService.GetAllTasks();

        Assert.That(all.Any(t => t.Title == "Persisted Task"), Is.True);
    }

    [Test]
    [Category("Integration")]
    public async Task CreateTask_SetsCreatedAtTimestamp()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var result = await _taskService.CreateTask(new TaskModel { Title = "Timestamped", Description = "" });

        Assert.That(result!.CreatedAt, Is.GreaterThan(before));
    }

    // ── DeleteTask ───────────────────────────────────────────────────────────

    [Test]
    [Category("Integration")]
    public async Task DeleteTask_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        var result = await _taskService.DeleteTask(99999);

        Assert.That(result, Is.EqualTo(UpdateResult.NotFound));
    }

    [Test]
    [Category("Integration")]
    public async Task DeleteTask_WhenTaskExists_ReturnsSuccess()
    {
        var created = await _taskService.CreateTask(new TaskModel { Title = "To Delete", Description = "" });

        var result = await _taskService.DeleteTask(created!.Id);

        Assert.That(result, Is.EqualTo(UpdateResult.Success));
    }

    [Test]
    [Category("Integration")]
    public async Task DeleteTask_WhenTaskExists_RemovesItFromDatabase()
    {
        var created = await _taskService.CreateTask(new TaskModel { Title = "Gone", Description = "" });

        await _taskService.DeleteTask(created!.Id);

        var found = await _taskService.GetTask(created.Id);
        Assert.That(found, Is.Null);
    }

    // ── UpdateTask ───────────────────────────────────────────────────────────

    [Test]
    [Category("Integration")]
    public async Task UpdateTask_WhenIdMismatch_ReturnsBadData()
    {
        var result = await _taskService.UpdateTask(1, new TaskModel { Id = 2, Title = "X" });

        Assert.That(result, Is.EqualTo(UpdateResult.BadData));
    }

    [Test]
    [Category("Integration")]
    public async Task UpdateTask_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        var result = await _taskService.UpdateTask(99999, new TaskModel { Id = 99999, Title = "Ghost" });

        Assert.That(result, Is.EqualTo(UpdateResult.NotFound));
    }

    [Test]
    [Category("Integration")]
    public async Task UpdateTask_WhenTaskExists_ReturnsSuccessAndPersistsChanges()
    {
        var created = await _taskService.CreateTask(new TaskModel { Title = "Original", Description = "Old" });
        var updated = new TaskModel { Id = created!.Id, Title = "Updated", Description = "New" };

        var result = await _taskService.UpdateTask(created.Id, updated);

        Assert.That(result, Is.EqualTo(UpdateResult.Success));
        var fetched = await _taskService.GetTask(created.Id);
        Assert.That(fetched!.Title, Is.EqualTo("Updated"));
        Assert.That(fetched.Description, Is.EqualTo("New"));
    }
}
