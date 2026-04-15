using IMDBImport.Database;
using IMDBImport.Models;

namespace IMDBImport;

public sealed class ImdbConsoleApp
{
    private readonly AppConfiguration _configuration;
    private readonly DatabaseScriptRunner _scriptRunner;
    private readonly ImdbBulkImporter _bulkImporter;
    private readonly AppDataAccess _appDataAccess;

    public ImdbConsoleApp(AppConfiguration configuration)
    {
        _configuration = configuration;
        _scriptRunner = new DatabaseScriptRunner();
        _bulkImporter = new ImdbBulkImporter();
        _appDataAccess = new AppDataAccess(configuration.AppConnectionString);
    }

    public async Task RunAsync()
    {
        Console.WriteLine("IMDB mandatory assignment");
        Console.WriteLine($"Workspace: {_configuration.WorkspaceDirectory}");
        Console.WriteLine($"Default data directory: {_configuration.DefaultDataDirectory}");
        Console.WriteLine();

        while (true)
        {
            PrintMenu();
            string? choice = Console.ReadLine()?.Trim();

            try
            {
                switch (choice)
                {
                    case "1":
                        await InitializeDatabaseAsync();
                        break;
                    case "2":
                        await ImportMandatoryDataAsync();
                        break;
                    case "3":
                        await SearchTitlesAsync();
                        break;
                    case "4":
                        await SearchPeopleAsync();
                        break;
                    case "5":
                        await AddTitleAsync();
                        break;
                    case "6":
                        await AddPersonAsync();
                        break;
                    case "7":
                        await UpdateTitleAsync();
                        break;
                    case "8":
                        await DeleteTitleAsync();
                        break;
                    case "9":
                        Console.WriteLine("Closing application.");
                        return;
                    default:
                        Console.WriteLine("Choose a number from 1 to 9.");
                        break;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Operation failed: {exception.Message}");
            }

            Console.WriteLine();
        }
    }

    private void PrintMenu()
    {
        Console.WriteLine("1. Initialize database objects");
        Console.WriteLine("2. Import mandatory IMDb files");
        Console.WriteLine("3. Search titles");
        Console.WriteLine("4. Search people");
        Console.WriteLine("5. Add title");
        Console.WriteLine("6. Add person");
        Console.WriteLine("7. Update title");
        Console.WriteLine("8. Delete title");
        Console.WriteLine("9. Exit");
        Console.Write("Choose an option: ");
    }

    private async Task InitializeDatabaseAsync()
    {
        string schemaScript = Path.Combine(_configuration.SqlDirectory, "01_schema.sql");
        string securityScript = Path.Combine(_configuration.SqlDirectory, "02_security.sql");

        Console.WriteLine("Running schema script...");
        await _scriptRunner.ExecuteFileAsync(_configuration.AdminConnectionString, schemaScript);

        Console.WriteLine("Running security script...");
        await _scriptRunner.ExecuteFileAsync(_configuration.AdminConnectionString, securityScript);

        Console.WriteLine("Database objects are ready.");
    }

    private async Task ImportMandatoryDataAsync()
    {
        Console.Write($"Data directory [{_configuration.DefaultDataDirectory}]: ");
        string? input = Console.ReadLine();
        string dataDirectory = string.IsNullOrWhiteSpace(input) ? _configuration.DefaultDataDirectory : input.Trim();

        await _bulkImporter.ImportMandatoryFilesAsync(_configuration.AdminConnectionString, dataDirectory);
    }

    private async Task SearchTitlesAsync()
    {
        Console.Write("Title search (* is allowed as wildcard): ");
        string searchTerm = ReadRequired();

        List<TitleSearchResult> results = await _appDataAccess.SearchTitlesAsync(searchTerm);
        if (results.Count == 0)
        {
            Console.WriteLine("No titles found.");
            return;
        }

        foreach (TitleSearchResult result in results.Take(50))
        {
            Console.WriteLine(result);
        }

        if (results.Count > 50)
        {
            Console.WriteLine($"Showing first 50 of {results.Count} titles.");
        }
    }

    private async Task SearchPeopleAsync()
    {
        Console.Write("Person search (* is allowed as wildcard): ");
        string searchTerm = ReadRequired();

        List<PersonSearchResult> results = await _appDataAccess.SearchPeopleAsync(searchTerm);
        if (results.Count == 0)
        {
            Console.WriteLine("No people found.");
            return;
        }

        foreach (PersonSearchResult result in results.Take(50))
        {
            Console.WriteLine(result);
        }

        if (results.Count > 50)
        {
            Console.WriteLine($"Showing first 50 of {results.Count} people.");
        }
    }

    private async Task AddTitleAsync()
    {
        TitleEditInput input = ReadTitleInput();
        string tconst = await _appDataAccess.AddTitleAsync(input);
        Console.WriteLine($"Title created with id {tconst}");
    }

    private async Task AddPersonAsync()
    {
        Console.Write("Primary name: ");
        string primaryName = ReadRequired();

        Console.Write("Birth year (optional): ");
        int? birthYear = ReadNullableInt();

        Console.Write("Death year (optional): ");
        int? deathYear = ReadNullableInt();

        string nconst = await _appDataAccess.AddPersonAsync(new PersonEditInput(primaryName, birthYear, deathYear));
        Console.WriteLine($"Person created with id {nconst}");
    }

    private async Task UpdateTitleAsync()
    {
        Console.Write("TConst to update: ");
        string tconst = ReadRequired();

        TitleEditInput input = ReadTitleInput();
        await _appDataAccess.UpdateTitleAsync(tconst, input);
        Console.WriteLine("Title updated.");
    }

    private async Task DeleteTitleAsync()
    {
        Console.Write("TConst to delete: ");
        string tconst = ReadRequired();

        await _appDataAccess.DeleteTitleAsync(tconst);
        Console.WriteLine("Title deleted.");
    }

    private static TitleEditInput ReadTitleInput()
    {
        Console.Write("Title type: ");
        string titleType = ReadRequired();

        Console.Write("Primary title: ");
        string primaryTitle = ReadRequired();

        Console.Write("Original title: ");
        string originalTitle = ReadRequired();

        Console.Write("Is adult? (y/n): ");
        bool isAdult = ReadBool();

        Console.Write("Start year (optional): ");
        int? startYear = ReadNullableInt();

        Console.Write("End year (optional): ");
        int? endYear = ReadNullableInt();

        Console.Write("Runtime in minutes (optional): ");
        int? runtimeMinutes = ReadNullableInt();

        return new TitleEditInput(titleType, primaryTitle, originalTitle, isAdult, startYear, endYear, runtimeMinutes);
    }

    private static string ReadRequired()
    {
        while (true)
        {
            string? value = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            Console.Write("Value is required. Try again: ");
        }
    }

    private static int? ReadNullableInt()
    {
        while (true)
        {
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (int.TryParse(input.Trim(), out int value))
            {
                return value;
            }

            Console.Write("Enter a valid number or leave empty: ");
        }
    }

    private static bool ReadBool()
    {
        while (true)
        {
            string? input = Console.ReadLine();
            if (string.Equals(input, "y", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(input, "n", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Console.Write("Type y or n: ");
        }
    }
}
