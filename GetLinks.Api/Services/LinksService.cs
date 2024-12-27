using GetLinks.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace GetLinks.Api.Services
{
    public class LinksService
    {
        readonly string _pattern = @"\b(?:href|src)\s*=\s*(['""]?)(.*?)\1";

        private static bool IsValidUrl(string? url)
        {
            if(!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult))
            {
                return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
            }

            return false;
        }

        public async Task<IActionResult> SearchByUrl(SearchPostRequest request)
        {
            if (!IsValidUrl(request.Url))
            {
                return new BadRequestObjectResult("The request URL is empty, null, or not valid.");
            }

            string input = "";
            try
            {
                using var client = new HttpClient();
                var result = await client.GetAsync(request.Url);
                input = await result.Content.ReadAsStringAsync();
            }
            catch
            {
                return new BadRequestObjectResult("The request URL is not valid.");
            }           

            var response = new SearchPostResponse();

            if (string.IsNullOrWhiteSpace(input))
            {
                return new OkObjectResult(response);
            }

            var matches = Regex.Matches(input, _pattern, RegexOptions.IgnoreCase);

            var urls = await Task.Factory.StartNew(() => ProcessMatches(matches, request.Url!, request.GetOnlyWithExtensions, request.Extensions));
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

            var urls = await Task.Factory.StartNew(() => ProcessMatches(matches, null, request.GetOnlyWithExtensions, request.Extensions));
            response.Urls = urls;

            return new OkObjectResult(response);
        }
        
        
        private static List<string> ProcessMatches(MatchCollection matches, string? baseUrl, bool onlyWithExtensions, IEnumerable<string>? extensions)
        {
            List<string> results = [];
            bool hasExtensions = extensions != null && extensions.Any();            

            foreach (Match match in matches)
            {
                var link = match.Groups[2].Value;

                if (onlyWithExtensions)
                {
                    var extension = Path.GetExtension(link);

                    if (string.IsNullOrWhiteSpace(extension) || (hasExtensions && !extensions!.Contains(extension)))
                        continue;
                }

                link = ProcessLink(link, baseUrl);

                results.Add(link);
            }

            return results;
        }

        private static string ProcessLink(string link, string? baseUrl)
        {
            var baseUrlIsNull = baseUrl == null;

            if (!baseUrlIsNull && (link.StartsWith('#') || link.StartsWith('/')))
            {
                if (link.StartsWith('/'))
                    link = link.Remove(0, 1);

                link = Path.Combine(baseUrl!, link);
            }
            else
            {
                var options = new UriCreationOptions();
                var created = Uri.TryCreate(link, options, out Uri? uri);
                var isAbsoluteUri = created && uri!.IsAbsoluteUri;

                if (!isAbsoluteUri && !baseUrlIsNull)
                    link = Path.Combine(baseUrl!, link);
            }

            return link;
        }
    }    
}
