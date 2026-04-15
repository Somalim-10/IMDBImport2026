using Microsoft.Data.SqlClient;
using System.Text;

namespace IMDBImport.Database;

public sealed class DatabaseScriptRunner
{
    public async Task ExecuteFileAsync(string connectionString, string scriptPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException($"SQL script not found: {scriptPath}", scriptPath);
        }

        string script = await File.ReadAllTextAsync(scriptPath, cancellationToken);
        await ExecuteScriptAsync(connectionString, script, cancellationToken);
    }

    public async Task ExecuteScriptAsync(string connectionString, string script, CancellationToken cancellationToken = default)
    {
        List<string> batches = SplitBatches(script);

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (string batch in batches)
        {
            await using SqlCommand command = new(batch, connection)
            {
                CommandTimeout = 0
            };

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static List<string> SplitBatches(string script)
    {
        List<string> batches = new();
        StringBuilder current = new();

        using StringReader reader = new(script);
        string? line;

        while ((line = reader.ReadLine()) is not null)
        {
            if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                AddBatchIfNeeded(batches, current);
                current.Clear();
                continue;
            }

            current.AppendLine(line);
        }

        AddBatchIfNeeded(batches, current);
        return batches;
    }

    private static void AddBatchIfNeeded(List<string> batches, StringBuilder builder)
    {
        if (!string.IsNullOrWhiteSpace(builder.ToString()))
        {
            batches.Add(builder.ToString());
        }
    }
}
