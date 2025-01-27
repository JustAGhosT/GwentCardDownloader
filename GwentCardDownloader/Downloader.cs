using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Linq;
using System.Drawing;
using System.Net.Http.Headers;
using System.Threading;
using NLog;
using ShellProgressBar;

namespace GwentCardDownloader
{
    public class Downloader
    {
        private readonly HttpClient client;
        private readonly string baseUrl;
        private readonly string imageFolder;
        private readonly int delay;
        private readonly Logger logger;
        private readonly string resumeFilePath;
        private int currentCard;
        private int totalCards;

        public Downloader(string baseUrl, string imageFolder, int delay, Logger logger, string resumeFilePath)
        {
            this.client = new HttpClient();
            this.baseUrl = baseUrl;
            this.imageFolder = imageFolder;
            this.delay = delay;
            this.logger = logger;
            this.resumeFilePath = resumeFilePath;
            this.currentCard = 0;
            this.totalCards = 0;

            // Set User-Agent header
            client.DefaultRequestHeaders.UserAgent.ParseAdd("GwentCardDownloader/1.0");
        }

        public async Task DownloadAllCards()
        {
            // Get the main page content
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(baseUrl);

            // Find all card containers
            var cardNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'gwent-card')]");

            if (cardNodes == null)
            {
                throw new Exception("No cards found on the page.");
            }

            totalCards = cardNodes.Count;

            var progressBarOptions = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                BackgroundColor = ConsoleColor.DarkGray,
                ProgressCharacter = '─'
            };

            var downloadedCards = new HashSet<string>(File.Exists(resumeFilePath) ? File.ReadAllLines(resumeFilePath) : Array.Empty<string>());

            using (var progressBar = new ProgressBar(totalCards, "Downloading cards", progressBarOptions))
            {
                var tasks = cardNodes.Select(async cardNode =>
                {
                    Interlocked.Increment(ref currentCard);

                    try
                    {
                        // Extract card details
                        var cardId = cardNode.GetAttributeValue("data-card-id", "");
                        var cardName = cardNode.SelectSingleNode(".//div[contains(@class, 'card-name')]")?.InnerText.Trim();

                        if (string.IsNullOrEmpty(cardId) || string.IsNullOrEmpty(cardName))
                        {
                            logger.Warn("Skipping card - missing information");
                            return;
                        }

                        // Clean filename
                        string safeFileName = string.Join("_", cardName.Split(Path.GetInvalidFileNameChars()));
                        string filePath = Path.Combine(imageFolder, $"{safeFileName}.png");

                        // Skip if file already exists
                        if (File.Exists(filePath) || downloadedCards.Contains(cardId))
                        {
                            logger.Info($"Skipping existing card: {cardName}");
                            return;
                        }

                        // Construct image URL (you might need to adjust this based on the website structure)
                        string imageUrl = $"https://gwent.one/image/card/low/{cardId}.jpg";

                        // Download the image
                        await DownloadImage(imageUrl, filePath);

                        // Verify the image
                        if (!VerifyImage(filePath))
                        {
                            logger.Warn($"Image verification failed for {cardName}, retrying...");
                            await DownloadImage(imageUrl, filePath);
                            if (!VerifyImage(filePath))
                            {
                                logger.Error($"Image verification failed for {cardName} after retrying");
                                return;
                            }
                        }

                        logger.Info($"Downloaded ({currentCard}/{totalCards}): {cardName}");

                        // Add to resume file
                        File.AppendAllLines(resumeFilePath, new[] { cardId });

                        // Add a small delay to be nice to the server
                        await Task.Delay(delay);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Error processing card");
                    }
                    finally
                    {
                        progressBar.Tick();
                    }
                }).ToList();

                await Task.WhenAll(tasks);
            }
        }

        private async Task DownloadImage(string imageUrl, string filePath)
        {
            int maxRetries = 3;
            int retryDelay = 2000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var imageBytes = await client.GetByteArrayAsync(imageUrl);
                    await File.WriteAllBytesAsync(filePath, imageBytes);
                    return;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Failed to download image from {imageUrl}, attempt {attempt} of {maxRetries}");
                    if (attempt == maxRetries)
                    {
                        throw;
                    }
                    await Task.Delay(retryDelay);
                    retryDelay *= 2; // Exponential backoff
                }
            }
        }

        private bool VerifyImage(string filePath)
        {
            try
            {
                using (var image = Image.FromFile(filePath))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task DownloadCardImageAsync(Card card, DownloadProgress progress, CancellationToken cancellationToken)
        {
            try
            {
                string imageUrl = card.GetImageUrl();
                string localPath = card.GetLocalPath();

                await DownloadImage(imageUrl, localPath);

                if (!VerifyImage(localPath))
                {
                    throw new Exception("Image verification failed");
                }

                card.IsDownloaded = true;
                card.DownloadDate = DateTime.Now;
                progress.UpdateProgress(card.Id, "Downloaded", 100);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to download image for card {card.Id}");
                throw;
            }
        }
    }
}
