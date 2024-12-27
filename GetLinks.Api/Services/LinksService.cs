using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace GetLinks.Api.Services
{
    public class SearchPostContentRequest
    {
        public string? Content { get; set; }
        public bool GetOnlyWithExtensions { get; set; } = true;
    }

    public class SearchPostRequest
    {
        public string? Url { get; set; }
        public bool GetOnlyWithExtensions { get; set; } = true;
    }

    public class SearchPostResponse
    {
        public ICollection<string> Urls { get; set; } = [];
    }

    public class LinksService
    {
       // readonly string _pattern = @"<a[^>]*\shref\s*=\s*(['""]?)(.*?)\1[^>]*>";
        readonly string _pattern = @"\b(?:href|src)\s*=\s*(['""]?)(.*?)\1";

        public async Task<IActionResult> SearchByUrl(SearchPostRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
                return new BadRequestObjectResult("The request URL is empty or null.");

            using var client = new HttpClient();
            var result = await client.GetAsync(request.Url);            
            var input = await result.Content.ReadAsStringAsync();

            var response = new SearchPostResponse();

            if (string.IsNullOrWhiteSpace(input))
            {
                return new OkObjectResult(response);
            }

            var matches = Regex.Matches(input, _pattern, RegexOptions.IgnoreCase);

            var urls = await Task.Factory.StartNew(() => ProcessMatches(matches, request.Url!, request.GetOnlyWithExtensions));
            response.Urls = urls;

            return new OkObjectResult(response);
        }

        public async Task<IActionResult> SearchByContent(SearchPostContentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return new BadRequestObjectResult("The request URL is empty or null.");

            var response = new SearchPostResponse();

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return new OkObjectResult(response);
            }

            var matches = Regex.Matches(request.Content, _pattern, RegexOptions.IgnoreCase);

            var urls = await Task.Factory.StartNew(() => ProcessMatches(matches, null, request.GetOnlyWithExtensions));
            response.Urls = urls;

            return new OkObjectResult(response);
        }
        
        
        private static List<string> ProcessMatches(MatchCollection matches, string? baseUrl, bool onlyWithExtensions)
        {
            List<string> results = [];

            foreach (Match match in matches)
            {
                var link = match.Groups[2].Value;

                if (onlyWithExtensions && string.IsNullOrWhiteSpace(Path.GetExtension(link)))
                    continue;

                var baseIsNull = baseUrl == null;                

                if (!baseIsNull && (link.StartsWith('#') || link.StartsWith('/')))
                {
                    if (link.StartsWith('/'))
                        link = link.Remove(0, 1);

                    link = Path.Combine(baseUrl!, link);
                }
                else
                {
                    var options = new UriCreationOptions();
                    var created = Uri.TryCreate(link, options, out Uri? uri);

                    if (created && !uri!.IsAbsoluteUri && baseIsNull)
                        link = Path.Combine(baseUrl!, link);
                }

                results.Add(link);
            }

            return results;
        }
    }    
}
