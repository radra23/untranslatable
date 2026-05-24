namespace Untranslatable.Data
{
    /// <summary>Domain model for an untranslatable word from the JSON data file.</summary>
    public record UntranslatableWord
    {
        public string Language { get; init; } = string.Empty;
        public string Word { get; init; } = string.Empty;
        public string Meaning { get; init; } = string.Empty;
    }
}
