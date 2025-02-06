using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using HtmlAgilityPack;
using NLog;
using GwentCardDownloader.Models;

namespace GwentCardDownloader.Services
{
    public class JsonDataDownloader
    {
        private readonly HttpClient _httpClient;
        private readonly string _pageUrl;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public JsonDataDownloader(string pageUrl)
        {
            _httpClient = new HttpClient();
            _pageUrl = pageUrl;
        }

        public async Task<List<Card>> DownloadCardDataAsync()
        {
            try
            {
                // Fetch the HTML content from the target page
                var response = await _httpClient.GetAsync(_pageUrl);
                response.EnsureSuccessStatusCode();
                var htmlContent = await response.Content.ReadAsStringAsync();

                // Load the HTML into HtmlAgilityPack
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                // Locate the script or element containing the JSON data.
                var scriptNode = htmlDoc.DocumentNode.SelectSingleNode("//script[contains(text(),'cardData')]");
                if (scriptNode == null)
                {
                    throw new Exception("Unable to locate the JSON data in the page.");
                }

                // Extract the JSON string from the script node.
                string scriptText = scriptNode.InnerText;
                string jsonString = ExtractJsonFromScript(scriptText);

                // Deserialize the JSON into a list of Card objects.
                var cardData = JsonSerializer.Deserialize<List<Card>>(jsonString);
                return cardData;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred while downloading card data.");
                throw;
            }
        }

        private string ExtractJsonFromScript(string scriptText)
        {
            try
            {
                // Implement extraction logic.
                // Example (pseudocode):
                // var match = Regex.Match(scriptText, @"cardData\s*=\s*(\{.*\});");
                // return match.Groups[1].Value;
                throw new NotImplementedException("JSON extraction logic needs to be implemented.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred while extracting JSON from script.");
                throw;
            }
        }
    }
}
