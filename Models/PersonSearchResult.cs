namespace IMDBImport.Models;

public sealed record PersonSearchResult(
    string NConst,
    string PrimaryName,
    int? BirthYear,
    int? DeathYear)
{
    public override string ToString()
    {
        string birth = BirthYear?.ToString() ?? "?";
        string death = DeathYear?.ToString() ?? "?";
        return $"{NConst} | {PrimaryName} | born {birth} | died {death}";
    }
}
