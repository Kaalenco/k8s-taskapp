using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TaskApp.Api.Data;

namespace TaskApp.Db;

/// <summary>
/// Provides a <see cref="TaskDbContext"/> for EF Core design-time tooling
/// (dotnet ef migrations add / dotnet ef migrations remove).
/// Pass <c>-- --database mysql|sqlserver</c> to select the provider; defaults to MySQL.
/// </summary>
internal sealed class TaskDbContextDesignTimeFactory : IDesignTimeDbContextFactory<TaskDbContext>
{
    public TaskDbContext CreateDbContext(string[] args)
    {
        var config = ConfigurationProvider.Value;
        var provider = DatabaseProviderParser.Parse(args) ?? DatabaseProvider.MySql;

        var (envVar, csKey) = DatabaseProviderParser.ConnectionKeys(provider);

        var connectionString =
            config.GetValue<string>(envVar)
            ?? config.GetConnectionString(csKey)
            ?? throw new InvalidOperationException(
                $"No connection string found. Set '{envVar}' or 'ConnectionStrings:{csKey}'.");

        var options = new DbContextOptionsBuilder<TaskDbContext>();

        if (provider == DatabaseProvider.MySql)
            options.UseMySQL(connectionString, b => b.MigrationsAssembly("TaskApp.Db"));
        else
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly("TaskApp.Db"));

        return new TaskDbContext(options.Options);
    }
}
