using Microsoft.Extensions.DependencyInjection;

namespace TaskApp.Application.Services;

public static class TaskServiceInitializer
{
    public static IServiceCollection AddTaskService(this IServiceCollection services)
    {
         services.AddScoped<ITaskService, TaskService>();
         return services;
    }
}
