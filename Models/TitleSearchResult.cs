namespace IMDBImport.Models;

public sealed record TitleSearchResult(
    string TConst,
    string PrimaryTitle,
    string? TitleType,
    int? StartYear,
    int? RuntimeMinutes)
{
    public override string ToString()
    {
        string year = StartYear?.ToString() ?? "?";
        string runtime = RuntimeMinutes?.ToString() ?? "?";
        return $"{TConst} | {PrimaryTitle} | {TitleType ?? "unknown"} | {year} | {runtime} min";
    }
}
