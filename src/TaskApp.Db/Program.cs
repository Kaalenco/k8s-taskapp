using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskApp.Api.Data;

namespace TaskApp.Db;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var provider = DatabaseProviderParser.Parse(args);
        if (provider is null)
        {
            await Console.Error.WriteLineAsync(
                "error: --database <mysql|sqlserver> is required.");
            return 1;
        }

        using var host = CreateHostBuilder(args, provider.Value).Build();
        return await RunMigrationsAsync(host.Services);
    }

    private static IHostBuilder CreateHostBuilder(string[] args, DatabaseProvider provider) =>
        Host.CreateDefaultBuilder(args)            
            .ConfigureAppConfiguration(config =>
            {
                ConfigurationProvider.Initialize(config);
            })
            .ConfigureServices((context, services) =>
            {
                var (envVar, csKey) = DatabaseProviderParser.ConnectionKeys(provider);

                var connectionString =
                    context.Configuration.GetValue<string>(envVar)
                    ?? context.Configuration.GetConnectionString(csKey);

                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException(
                        $"No connection string found. Set '{envVar}' or 'ConnectionStrings:{csKey}'.");

                services.AddDbContext<TaskDbContext>(options =>
                {
                    if (provider == DatabaseProvider.MySql)
                        options.UseMySQL(connectionString, b => b.MigrationsAssembly("TaskApp.Db"));
                    else
                        options.UseSqlServer(connectionString, b => b.MigrationsAssembly("TaskApp.Db"));
                });
            });

    private static async Task<int> RunMigrationsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TaskDbContext>>();
        var context = scope.ServiceProvider.GetRequiredService<TaskDbContext>();

        try
        {
            logger.LogInformation("Applying pending migrations to the database.");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Database migration failed.");
            return 1;
        }
    }
}
