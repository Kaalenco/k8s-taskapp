using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using TaskApp.Api.Data;
using TaskApp.Application.Models;

namespace TaskApp.Application.Services;

public class TaskService(TaskDbContext context, ILogger<TaskService> logger) : ITaskService
{
    public async Task<IEnumerable<TaskModel>> GetAllTasks()
    {
        var logHelper = LoggerHelper.For(logger, nameof(TaskModel));
        logHelper.FindItems("");
        var tasks = await context.Tasks.ToListAsync();
        return tasks.Select(TaskModel.FromEntity);
    }

    public async Task<TaskModel?> GetTask(int id)
    {
        var logHelper = LoggerHelper.For(logger, nameof(TaskModel));
        logHelper.ItemRetrieving(id);
        var task = await context.Tasks.FindAsync(id);
        if (task == null)
        {
            logHelper.ItemNotFound(id);
            return null;
        }
        return TaskModel.FromEntity(task);
    }

    public async Task<TaskModel?> CreateTask(TaskModel task)
    {
        var logHelper = LoggerHelper.For(logger, nameof(TaskModel));

        logHelper.ItemCreating(task.Title);
        var entity = task.ToEntityModel();
        entity.CreatedAt = DateTime.UtcNow;
        context.Tasks.Add(entity);
        try
        {
            var modified = await context.SaveChangesAsync();
            if (modified == 0)
            {
                logHelper.CouldNotCreate(null, task.Title);
                return null;
            }
            else
            {
                logHelper.ItemCreated(entity.Id);
            }
            return TaskModel.FromEntity(entity);
        }
        catch (Exception ex)
        {
            logHelper.CouldNotCreate(ex, task.Title);
            return null;
        }
    }

    public async Task<(UpdateResult result, TaskModel? task)> UpdateTask(int id, TaskModel task)
    {
        var logHelper = LoggerHelper.For(logger, nameof(TaskModel));

        if (id != task.Id) return (UpdateResult.BadData, task);

        logHelper.ItemModifying(id);
        var existingTask = await context.Tasks.FindAsync(id);
        if (existingTask == null) return (UpdateResult.NotFound, null);
        task.CopyTo(existingTask);

        try
        {
            var modified = await context.SaveChangesAsync();
            if (modified == 0)
            {
                return (UpdateResult.NoChanges, TaskModel.FromEntity(existingTask));
            }
            logHelper.ItemModified( id);
            return (UpdateResult.Success, TaskModel.FromEntity(existingTask));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!await TaskExists(id))
            {
                logHelper.ItemNotFound(id);
                return (UpdateResult.NotFound, null);
            }
            else
            {
                logHelper.Error(ex, "Concurrency error while updating.", id);
                return (UpdateResult.Error, null);
            }
        }
    }

    public async Task<UpdateResult> DeleteTask(int id)
    {
        var logHelper = LoggerHelper.For(logger, nameof(TaskModel));
        logHelper.ItemDeleting(id);

        var task = await context.Tasks.FindAsync(id);
        if (task == null)
        {
            logHelper.ItemNotFound(id);
            return UpdateResult.NotFound;
        }
        context.Tasks.Remove(task);
        await context.SaveChangesAsync();
        logHelper.ItemDeleted(id);
        return UpdateResult.Success;
    }

    private async Task<bool> TaskExists(int id)
    {
        return await context.Tasks.AnyAsync(e => e.Id == id);
    }
}

public enum UpdateResult
{
    None,
    BadData,
    Success,
    NotFound,
    NoChanges,
    Error
}
