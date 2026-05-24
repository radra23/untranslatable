namespace Untranslatable.Api.Models
{
    /// <summary>API response DTO for an untranslatable word.</summary>
    public record UntranslatableWordDto
    {
        public string Language { get; init; } = string.Empty;
        public string Word { get; init; } = string.Empty;
        public string Meaning { get; init; } = string.Empty;
    }
}
