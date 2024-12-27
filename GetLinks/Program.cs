using System.Text.RegularExpressions;

using (var client = new HttpClient()) // WebClient class inherits IDisposable
{
    var result = await client.GetStringAsync("https://www.cacp.app.br/pdfs-de-estudos/");
    //var content = await result.Content.ReadAsStringAsync();

    var content = File.ReadAllText("D:/html.txt");   

    string input = content;
    //string pattern = @"<a\s+[^>]*href=[""']([^""']+)[""']";
    string pattern = @"<a[^>]*\shref\s*=\s*(['""]?)(.*?)\1[^>]*>";
    Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
    MatchCollection matches = regex.Matches(input);

    foreach (Match match in matches)
    {
        Console.WriteLine(match.Groups[2].Value);
    }
}