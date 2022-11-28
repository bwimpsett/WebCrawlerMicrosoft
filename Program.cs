/*
 * Assumptions made:
 * 1. I didn't count pure numbers as word. (e.g. 1986 is NOT counted as a word.)
 * 2. Any words ending with a grammatical character (e.g. Microsoft. ).
 * I removed the character and counted it with the rest of the words.
 * The only exception would be for a single ' (e.g. Gates' )
 * 3. I only used the paragraphs contained within the 'History' section.
 * I didn't use any of the headers or image captions.
*/

using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace WebCrawlerMicrosoft;

internal class Program
{
    private static void Main()
    {
        int numberOfWordsToReturn = 10;
        Console.Write("Enter the number of words you want to return (Default is 10): ");
        var input1 = Console.ReadLine();
        if (input1 != null && int.TryParse(input1, out int result))
        {
            numberOfWordsToReturn = result;
        }

        List<string> wordsToExclude = new();
        Console.WriteLine("Enter the words you would like to exclude. Separated by a comma:");
        var input2 = Console.ReadLine();
        if (input2 != null) { wordsToExclude.AddRange(input2.Split(",")); }
        Console.WriteLine();

        string htmlString = TruncateData(GetData());
        string rawData = GetParagraphs(htmlString);
        string sanitizedText = SanitizeText(rawData);
        var wordCount = WordCount(sanitizedText);

        PrintResults(wordCount, numberOfWordsToReturn, wordsToExclude);
    }

    private static string GetData()
    {
        HttpClient client = new() { BaseAddress = new Uri("https://en.wikipedia.org/"), };
        var response = client.GetAsync("https://en.wikipedia.org/wiki/Microsoft");
        return response.Result.Content.ReadAsStringAsync().Result;
    }

    private static string TruncateData(string strHtml)
    {
        var iStart = strHtml.IndexOf("<h2><span class=\"mw-headline\" id=\"History\">History</span></h2>", StringComparison.Ordinal);
        var retVal = strHtml.Remove(0, iStart);
        var iEnd = retVal.IndexOf("<h2><span class=\"mw-headline\" id=\"Corporate_affairs\">Corporate affairs</span></h2>", StringComparison.Ordinal);
        retVal = retVal.Remove(iEnd);
        return retVal;
    }

    private static string GetParagraphs(string result)
    {
        HtmlDocument resultHtmlDocument = new();
        resultHtmlDocument.LoadHtml(result);
        List<HtmlNode> paragraphs = resultHtmlDocument.DocumentNode.Descendants().Where(x => x.Name == "p").ToList();
        StringBuilder sb = new();
        foreach (var paragraph in paragraphs)
        {
            sb.AppendLine(paragraph.InnerText);
        }
        return sb.ToString();
    }

    private static string SanitizeText(string rawText)
    {
        var replacement = Regex.Replace(rawText, "[;:#&,()\"“”]+", " ");
        replacement = Regex.Replace(replacement, @"\.\s", " ");
        replacement = replacement.Replace(Environment.NewLine, " ");
        return replacement;
    }

    private static Dictionary<string, int> WordCount(string htmlText)
    {
        Dictionary<string, int> wordFrequency = new();
        foreach (var word in htmlText.Split(' '))
        {
            string wt = word.Trim();
            if (!Regex.Match(wt, "[A-Za-z]").Success)
            {
                continue;
            }
            var iKey = wordFrequency.Keys.FirstOrDefault(x => x == wt);
            if (iKey != null)
            {
                wordFrequency[wt] += 1;
            }
            else
            {
                wordFrequency.Add(wt, 1);
            }
        }
        return wordFrequency;
    }

    private static void PrintResults(Dictionary<string, int> dict, int numberOfWords, List<string> excludedWords)
    {
        var dictWc = dict.OrderByDescending(x => x.Value);
        var i = 0;
        Console.WriteLine("__________________________________________");
        Console.WriteLine("| Words               | # of occurrences |");
        Console.WriteLine("|_____________________|__________________|");
        foreach (var item in dictWc)
        {
            if (i >= numberOfWords) break;
            if (excludedWords.Contains(item.Key)) continue;
            Console.WriteLine("| " + item.Key.PadRight(20) + "| " + item.Value.ToString().PadRight(17) + "|");
            i++;
        }
        Console.WriteLine("|_____________________|__________________|");
    }
}