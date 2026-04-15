namespace IMDBImport.Models;

public sealed record PersonEditInput(
    string PrimaryName,
    int? BirthYear,
    int? DeathYear);
