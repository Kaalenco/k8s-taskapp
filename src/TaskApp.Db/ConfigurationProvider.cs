using Microsoft.Extensions.Configuration;

namespace TaskApp.Db;

internal sealed class ConfigurationProvider
{
    public static IConfigurationBuilder Initialize(IConfigurationBuilder builder)
    {
        return builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<TaskDbContextDesignTimeFactory>();
    }

    private static readonly Lazy<IConfiguration> lazyConfig = new(() =>
    {
        var config = new ConfigurationBuilder();
        return Initialize(config).Build();
    });
    public static IConfiguration Value => lazyConfig.Value;
}
