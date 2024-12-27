namespace GetLinks.Api.Models
{
    public class SearchPostContentRequest
    {
        public string? Content { get; set; }
        public bool GetOnlyWithExtensions { get; set; } = true;
        public IEnumerable<string>? Extensions { get; set; } = null;
    }
}
