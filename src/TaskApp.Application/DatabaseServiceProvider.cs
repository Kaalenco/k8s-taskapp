using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TaskApp.Api.Data;

namespace TaskApp.Application;

public static class DatabaseServiceProvider
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbProvider = (configuration["Database:Provider"] ?? string.Empty).ToLowerInvariant();

        var connectionString = dbProvider switch
        {
            "mysql" => configuration.GetValue<string>("MYSQL_TASKAPP_CONNECTION")
                ?? configuration.GetConnectionString("MySqlConnection"),
            "sqlserver" => configuration.GetValue<string>("SQLSERVER_TASKAPP_CONNECTION")
                ?? configuration.GetConnectionString("SqlServerConnection"),
            _ => throw new InvalidOperationException(
                $"'Database:Provider' must be 'mysql' or 'sqlserver' (was '{dbProvider}'). " +
                "Set it in appsettings.json or via the Database__Provider environment variable.")
        } ?? throw new InvalidOperationException(
            $"No connection string found for provider '{dbProvider}'. " +
            "Set MYSQL_TASKAPP_CONNECTION / SQLSERVER_TASKAPP_CONNECTION or the matching ConnectionStrings entry.");

        services.AddDbContext<TaskDbContext>(options =>
        {
            if (dbProvider == "mysql")
                options.UseMySQL(connectionString);
            else
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
        });

        return services;
    }
}
