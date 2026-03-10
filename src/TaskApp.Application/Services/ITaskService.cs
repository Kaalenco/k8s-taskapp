using TaskApp.Application.Models;

namespace TaskApp.Application.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskModel>> GetAllTasks();
    Task<TaskModel?> GetTask(int id);
    Task<TaskModel?> CreateTask(TaskModel task);
    Task<(UpdateResult result, TaskModel? task)> UpdateTask(int id, TaskModel task);
    Task<UpdateResult> DeleteTask(int id);
}
