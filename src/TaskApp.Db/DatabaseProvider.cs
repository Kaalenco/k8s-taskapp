namespace TaskApp.Db;

internal enum DatabaseProvider { MySql, SqlServer }

internal static class DatabaseProviderParser
{
    internal static DatabaseProvider? Parse(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (!args[i].Equals("--database", StringComparison.OrdinalIgnoreCase))
                continue;

            return args[i + 1].ToLowerInvariant() switch
            {
                "mysql"     => DatabaseProvider.MySql,
                "sqlserver" => DatabaseProvider.SqlServer,
                _           => null
            };
        }
        return null;
    }

    internal static (string EnvVar, string CsKey) ConnectionKeys(DatabaseProvider provider) =>
        provider switch
        {
            DatabaseProvider.MySql     => ("MYSQL_TASKAPP_CONNECTION",     "MySqlConnection"),
            DatabaseProvider.SqlServer => ("SQLSERVER_TASKAPP_CONNECTION", "SqlServerConnection"),
            _                          => throw new InvalidOperationException($"Unknown provider: {provider}")
        };
}
