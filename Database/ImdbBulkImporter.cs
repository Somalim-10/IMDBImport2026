using Microsoft.Data.SqlClient;
using System.Data;

namespace IMDBImport.Database;

public sealed class ImdbBulkImporter
{
    private const int BatchSize = 5000;

    public async Task ImportMandatoryFilesAsync(string adminConnectionString, string dataDirectory, CancellationToken cancellationToken = default)
    {
        string titleBasicsPath = Path.Combine(dataDirectory, "title.basics.tsv");
        string nameBasicsPath = Path.Combine(dataDirectory, "name.basics.tsv");
        string titleCrewPath = Path.Combine(dataDirectory, "title.crew.tsv");

        EnsureFileExists(titleBasicsPath);
        EnsureFileExists(nameBasicsPath);
        EnsureFileExists(titleCrewPath);

        string imdbConnectionString = BuildConnectionStringForDatabase(adminConnectionString, "IMDB");

        await using SqlConnection connection = new(imdbConnectionString);
        await connection.OpenAsync(cancellationToken);

        await ClearStagingTablesAsync(connection, cancellationToken);

        Console.WriteLine("Importing title.basics.tsv to staging...");
        await BulkCopyFileAsync(
            connection,
            titleBasicsPath,
            "staging.TitleBasicsImport",
            BuildTitleBasicsTable(),
            MapTitleBasicsRow,
            cancellationToken);

        Console.WriteLine("Importing name.basics.tsv to staging...");
        await BulkCopyFileAsync(
            connection,
            nameBasicsPath,
            "staging.NameBasicsImport",
            BuildNameBasicsTable(),
            MapNameBasicsRow,
            cancellationToken);

        Console.WriteLine("Importing title.crew.tsv to staging...");
        await BulkCopyFileAsync(
            connection,
            titleCrewPath,
            "staging.TitleCrewImport",
            BuildTitleCrewTable(),
            MapTitleCrewRow,
            cancellationToken);

        Console.WriteLine("Rebuilding normalized tables from staging...");
        await using SqlCommand rebuildCommand = new("EXEC dbo.usp_RebuildNormalizedDataFromStaging;", connection)
        {
            CommandTimeout = 0
        };

        await rebuildCommand.ExecuteNonQueryAsync(cancellationToken);
        Console.WriteLine("Import completed.");
    }

    private static async Task ClearStagingTablesAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            TRUNCATE TABLE staging.TitleCrewImport;
            TRUNCATE TABLE staging.NameBasicsImport;
            TRUNCATE TABLE staging.TitleBasicsImport;
            """;

        await using SqlCommand command = new(sql, connection)
        {
            CommandTimeout = 0
        };

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task BulkCopyFileAsync(
        SqlConnection connection,
        string filePath,
        string destinationTable,
        DataTable table,
        Func<string[], object?[]> mapper,
        CancellationToken cancellationToken)
    {
        using SqlBulkCopy bulkCopy = new(connection)
        {
            DestinationTableName = destinationTable,
            BatchSize = BatchSize,
            BulkCopyTimeout = 0
        };

        int importedRows = 0;

        foreach (string line in File.ReadLines(filePath).Skip(1))
        {
            string[] parts = line.Split('\t');
            table.Rows.Add(mapper(parts));

            if (table.Rows.Count >= BatchSize)
            {
                await bulkCopy.WriteToServerAsync(table, cancellationToken);
                importedRows += table.Rows.Count;
                Console.WriteLine($"{Path.GetFileName(filePath)}: {importedRows:N0} rows imported");
                table.Clear();
            }
        }

        if (table.Rows.Count > 0)
        {
            await bulkCopy.WriteToServerAsync(table, cancellationToken);
            importedRows += table.Rows.Count;
            table.Clear();
        }

        Console.WriteLine($"{Path.GetFileName(filePath)}: {importedRows:N0} rows imported in total");
    }

    private static DataTable BuildTitleBasicsTable()
    {
        DataTable table = new();
        table.Columns.Add("TConst", typeof(string));
        table.Columns.Add("TitleType", typeof(string));
        table.Columns.Add("PrimaryTitle", typeof(string));
        table.Columns.Add("OriginalTitle", typeof(string));
        table.Columns.Add("IsAdult", typeof(string));
        table.Columns.Add("StartYear", typeof(string));
        table.Columns.Add("EndYear", typeof(string));
        table.Columns.Add("RuntimeMinutes", typeof(string));
        table.Columns.Add("GenresCsv", typeof(string));
        return table;
    }

    private static DataTable BuildNameBasicsTable()
    {
        DataTable table = new();
        table.Columns.Add("NConst", typeof(string));
        table.Columns.Add("PrimaryName", typeof(string));
        table.Columns.Add("BirthYear", typeof(string));
        table.Columns.Add("DeathYear", typeof(string));
        table.Columns.Add("PrimaryProfessionCsv", typeof(string));
        table.Columns.Add("KnownForTitlesCsv", typeof(string));
        return table;
    }

    private static DataTable BuildTitleCrewTable()
    {
        DataTable table = new();
        table.Columns.Add("TConst", typeof(string));
        table.Columns.Add("DirectorsCsv", typeof(string));
        table.Columns.Add("WritersCsv", typeof(string));
        return table;
    }

    private static object?[] MapTitleBasicsRow(string[] parts)
    {
        if (parts.Length != 9)
        {
            throw new InvalidOperationException($"Invalid title.basics row with {parts.Length} columns.");
        }

        return parts.Select(NormalizeValue).ToArray();
    }

    private static object?[] MapNameBasicsRow(string[] parts)
    {
        if (parts.Length != 6)
        {
            throw new InvalidOperationException($"Invalid name.basics row with {parts.Length} columns.");
        }

        return parts.Select(NormalizeValue).ToArray();
    }

    private static object?[] MapTitleCrewRow(string[] parts)
    {
        if (parts.Length != 3)
        {
            throw new InvalidOperationException($"Invalid title.crew row with {parts.Length} columns.");
        }

        return parts.Select(NormalizeValue).ToArray();
    }

    private static object NormalizeValue(string value)
    {
        return value == "\\N" ? DBNull.Value : value;
    }

    private static void EnsureFileExists(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Mandatory IMDb file not found: {path}", path);
        }
    }

    private static string BuildConnectionStringForDatabase(string connectionString, string databaseName)
    {
        SqlConnectionStringBuilder builder = new(connectionString)
        {
            InitialCatalog = databaseName
        };

        return builder.ConnectionString;
    }
}
