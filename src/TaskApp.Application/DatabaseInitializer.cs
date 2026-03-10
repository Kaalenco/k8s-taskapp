using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using TaskApp.Api.Data;
using TaskApp.Api.Data.Models;

namespace TaskApp.Application;

public static class DatabaseInitializer
{
    public static void Initialize(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TaskDbContext>>();

        try
        {
            if (!context.Tasks.Any())
            {
                logger.LogInformation("Seeding database with sample tasks");
                context.Tasks.AddRange(
                    new TaskItem { Title = "Sample Task 1", Description = "This is a sample task", Status = "pending" },
                    new TaskItem { Title = "Sample Task 2", Description = "Another sample task", Status = "completed" },
                    new TaskItem { Title = "Complete Documentation", Description = "Finish the API documentation", Status = "in-progress" }
                );
                context.SaveChanges();
                logger.LogInformation("Database seeding completed");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database seeding failed — schema may not be ready yet (migration runner may still be pending)");
        }
    }
}
