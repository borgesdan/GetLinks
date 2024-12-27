namespace GetLinks.Api.Models
{
    public class SearchPostRequest
    {
        public string? Url { get; set; }
        public bool GetOnlyWithExtensions { get; set; } = true;
        public IEnumerable<string>? Extensions { get; set; } = null;
    }
}
