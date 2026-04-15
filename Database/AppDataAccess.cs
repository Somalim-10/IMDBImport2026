using IMDBImport.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace IMDBImport.Database;

public sealed class AppDataAccess
{
    private readonly string _connectionString;

    public AppDataAccess(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<TitleSearchResult>> SearchTitlesAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        List<TitleSearchResult> results = new();

        await using SqlConnection connection = new(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new("dbo.usp_SearchTitles", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@SearchTerm", searchTerm);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new TitleSearchResult(
                reader.GetString(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetInt32(3),
                reader.IsDBNull(4) ? null : reader.GetInt32(4)));
        }

        return results;
    }

    public async Task<List<PersonSearchResult>> SearchPeopleAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        List<PersonSearchResult> results = new();

        await using SqlConnection connection = new(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new("dbo.usp_SearchPeople", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@SearchTerm", searchTerm);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new PersonSearchResult(
                reader.GetString(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetInt32(2),
                reader.IsDBNull(3) ? null : reader.GetInt32(3)));
        }

        return results;
    }

    public async Task<string> AddTitleAsync(TitleEditInput input, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = new(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new("dbo.usp_AddTitle", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        AddTitleParameters(command, input);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToString(result) ?? string.Empty;
    }

    public async Task UpdateTitleAsync(string tconst, TitleEditInput input, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = new(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new("dbo.usp_UpdateTitle", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@TConst", tconst);
        AddTitleParameters(command, input);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteTitleAsync(string tconst, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = new(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new("dbo.usp_DeleteTitle", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@TConst", tconst);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<string> AddPersonAsync(PersonEditInput input, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = new(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new("dbo.usp_AddPerson", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@PrimaryName", input.PrimaryName);
        command.Parameters.AddWithValue("@BirthYear", (object?)input.BirthYear ?? DBNull.Value);
        command.Parameters.AddWithValue("@DeathYear", (object?)input.DeathYear ?? DBNull.Value);

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToString(result) ?? string.Empty;
    }

    private static void AddTitleParameters(SqlCommand command, TitleEditInput input)
    {
        command.Parameters.AddWithValue("@TitleType", input.TitleType);
        command.Parameters.AddWithValue("@PrimaryTitle", input.PrimaryTitle);
        command.Parameters.AddWithValue("@OriginalTitle", input.OriginalTitle);
        command.Parameters.AddWithValue("@IsAdult", input.IsAdult);
        command.Parameters.AddWithValue("@StartYear", (object?)input.StartYear ?? DBNull.Value);
        command.Parameters.AddWithValue("@EndYear", (object?)input.EndYear ?? DBNull.Value);
        command.Parameters.AddWithValue("@RuntimeMinutes", (object?)input.RuntimeMinutes ?? DBNull.Value);
    }
}
