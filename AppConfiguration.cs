using Microsoft.Data.SqlClient;

namespace IMDBImport;

public sealed class AppConfiguration
{
    public string AdminConnectionString { get; }
    public string AppConnectionString { get; }
    public string DefaultDataDirectory { get; }
    public string SqlDirectory { get; }
    public string WorkspaceDirectory { get; }

    private AppConfiguration(
        string adminConnectionString,
        string appConnectionString,
        string defaultDataDirectory,
        string sqlDirectory,
        string workspaceDirectory)
    {
        AdminConnectionString = EnsureSqlClientDefaults(adminConnectionString);
        AppConnectionString = EnsureSqlClientDefaults(appConnectionString);
        DefaultDataDirectory = defaultDataDirectory;
        SqlDirectory = sqlDirectory;
        WorkspaceDirectory = workspaceDirectory;
    }

    public static AppConfiguration Load(string baseDirectory, string workspaceDirectory)
    {
        string adminConnection = Environment.GetEnvironmentVariable("IMDB_ADMIN_CONNECTION")
            ?? "Server=localhost;Database=master;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;";

        string appConnection = Environment.GetEnvironmentVariable("IMDB_APP_CONNECTION")
            ?? "Server=localhost;Database=IMDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;";

        string dataDirectory = Environment.GetEnvironmentVariable("IMDB_DATA_DIRECTORY")
            ?? Path.Combine(workspaceDirectory, "data");

        return new AppConfiguration(
            adminConnection,
            appConnection,
            dataDirectory,
            Path.Combine(workspaceDirectory, "sql"),
            workspaceDirectory);
    }

    private static string EnsureSqlClientDefaults(string connectionString)
    {
        SqlConnectionStringBuilder builder = new(connectionString)
        {
            Encrypt = false,
            TrustServerCertificate = true
        };

        return builder.ConnectionString;
    }
}
