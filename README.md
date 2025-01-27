# Gwent Card Downloader

This repository contains a .NET Core console application to download Gwent cards. The application uses `HttpClient` for web requests and `HtmlAgilityPack` for HTML parsing.

## Instructions

### 1. Create a new .NET Core Console application

Run the following command to create a new .NET Core Console application:

```sh
dotnet new console -n GwentCardDownloader
```

### 2. Add the HtmlAgilityPack NuGet package

You can add the HtmlAgilityPack NuGet package using one of the following methods:

#### Using Visual Studio's NuGet Package Manager

1. Right-click on the project in the Solution Explorer.
2. Select "Manage NuGet Packages".
3. Search for "HtmlAgilityPack" and install it.

#### Using the command line

Run the following command to add the HtmlAgilityPack NuGet package:

```sh
dotnet add package HtmlAgilityPack
```

### 3. Add the System.Drawing.Common NuGet package

Run the following command to add the System.Drawing.Common NuGet package:

```sh
dotnet add package System.Drawing.Common
```

### 4. Add the ShellProgressBar NuGet package

Run the following command to add the ShellProgressBar NuGet package:

```sh
dotnet add package ShellProgressBar
```

### 5. Add the NLog NuGet package

Run the following command to add the NLog NuGet package:

```sh
dotnet add package NLog
```

### 6. Add the Serilog NuGet package

Run the following command to add the Serilog NuGet package:

```sh
dotnet add package Serilog
```

### 7. Create the `Card` class

Create a new file named `Card.cs` in the `GwentCardDownloader` directory and add the following code:

```csharp
using System;

namespace GwentCardDownloader
{
    public class Card
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Faction { get; set; }
        public string ImageUrl { get; set; }
        public bool IsDownloaded { get; set; }
        public DateTime? DownloadDate { get; set; }
        public int RetryCount { get; set; }
        public string LocalPath { get; set; }

        public Card(string id, string name, string faction, string imageUrl, bool isDownloaded, DateTime? downloadDate, int retryCount, string localPath)
        {
            Id = id;
            Name = name;
            Faction = faction;
            ImageUrl = imageUrl;
            IsDownloaded = isDownloaded;
            DownloadDate = downloadDate;
            RetryCount = retryCount;
            LocalPath = localPath;
        }
    }
}
```

### 8. Replace the content of `Program.cs` with the provided code

Replace the content of the `Program.cs` file in your project with the following code:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace GwentCardDownloader
{
    class Program
    {
        private const string DefaultBaseUrl = "https://gwent.one/en/cards/";
        private const string DefaultImageFolder = "gwent_cards";
        private const int DefaultDelay = 100;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static string resumeFilePath = "resume.txt";

        static async Task Main(string[] args)
        {
            string baseUrl = DefaultBaseUrl;
            string imageFolder = DefaultImageFolder;
            int delay = DefaultDelay;

            if (args.Length > 0)
            {
                baseUrl = args.ElementAtOrDefault(0) ?? DefaultBaseUrl;
                imageFolder = args.ElementAtOrDefault(1) ?? DefaultImageFolder;
                delay = int.TryParse(args.ElementAtOrDefault(2), out int parsedDelay) ? parsedDelay : DefaultDelay;
            }

            // Create directory if it doesn't exist
            Directory.CreateDirectory(imageFolder);

            // Configure logging
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "logfile.txt" };
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;

            try
            {
                logger.Info("Starting Gwent card download...");

                var downloaderConfig = new DownloaderConfig
                {
                    BaseUrl = baseUrl,
                    ImageFolder = imageFolder,
                    Delay = delay,
                    MaxRetries = 3,
                    MaxConcurrentDownloads = 5,
                    SkipExisting = true,
                    Quality = ImageQuality.Low,
                    UserAgent = "GwentCardDownloader/1.0",
                    Headers = new Dictionary<string, string>()
                };

                var downloader = new Downloader(downloaderConfig.BaseUrl, downloaderConfig.ImageFolder, downloaderConfig.Delay, new Logger(), resumeFilePath);
                await downloader.DownloadAllCards();

                logger.Info("Download completed!");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
```

### 9. Create the `DownloadManager` class

Create a new file named `DownloadManager.cs` in the `GwentCardDownloader` directory and add the following code:

```csharp
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace GwentCardDownloader
{
    public class DownloadManager
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentDictionary<string, Card> _cards;
        private readonly DownloaderConfig _config;
        private readonly Logger _logger;

        public DownloadManager(DownloaderConfig config, Logger logger)
        {
            _semaphore = new SemaphoreSlim(config.MaxConcurrentDownloads);
            _cards = new ConcurrentDictionary<string, Card>();
            _config = config;
            _logger = logger;
        }

        public async Task DownloadCardsAsync(IEnumerable<Card> cards, CancellationToken cancellationToken)
        {
            using var progress = new DownloadProgress(cards.Count());

            var tasks = cards.Select(card => ProcessCardAsync(card, progress, cancellationToken));
            await Task.WhenAll(tasks);
        }

        private async Task ProcessCardAsync(Card card, DownloadProgress progress, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_config.SkipExisting && File.Exists(card.LocalPath))
                {
                    progress.UpdateProgress(card.Id, "Skipped - Already exists", 100);
                    return;
                }

                await DownloadWithRetryAsync(card, progress, cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task DownloadWithRetryAsync(Card card, DownloadProgress progress, CancellationToken cancellationToken)
        {
            var policy = Policy
                .Handle<HttpRequestException>()
                .Or<IOException>()
                .WaitAndRetryAsync(
                    _config.MaxRetries,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.Warn($"Retry {retryCount} for card {card.Id} after {timeSpan.TotalSeconds}s");
                        progress.UpdateProgress(card.Id, $"Retry {retryCount}", (retryCount * 100) / _config.MaxRetries);
                    }
                );

            await policy.ExecuteAsync(async () =>
            {
                await DownloadCardImageAsync(card, progress, cancellationToken);
            });
        }

        private async Task DownloadCardImageAsync(Card card, DownloadProgress progress, CancellationToken cancellationToken)
        {
            // Implement the logic to download the card image
            // This is a placeholder method and should be replaced with actual implementation
            await Task.Delay(1000, cancellationToken); // Simulate download delay
            progress.UpdateProgress(card.Id, "Downloaded", 100);
        }
    }
}
```

### 10. Create the `DownloadProgress` class

Create a new file named `DownloadProgress.cs` in the `GwentCardDownloader` directory and add the following code:

```csharp
using System;
using System.Collections.Generic;
using ShellProgressBar;

namespace GwentCardDownloader
{
    public class DownloadProgress
    {
        private readonly ShellProgressBar.ProgressBar _mainProgressBar;
        private readonly Dictionary<string, ChildProgressBar> _childBars;
        private readonly object _lockObject = new object();

        public DownloadProgress(int totalCards)
        {
            var options = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                BackgroundColor = ConsoleColor.DarkGray,
                ProgressCharacter = '─',
                EnableTaskBarProgress = true,
                ShowEstimatedDuration = true
            };

            _mainProgressBar = new ShellProgressBar.ProgressBar(totalCards, "Downloading cards", options);
            _childBars = new Dictionary<string, ChildProgressBar>();
        }

        public void UpdateProgress(string cardId, string message, int progress)
        {
            lock (_lockObject)
            {
                if (!_childBars.ContainsKey(cardId))
                {
                    _childBars[cardId] = _mainProgressBar.Spawn(100, $"Card: {cardId}");
                }
                _childBars[cardId].Tick(progress, message);
            }
        }
    }
}
```

### 11. Update the `Downloader` class to use the `DownloadManager` and `DownloadProgress` classes

Update the `Downloader` class to use the `DownloadManager` and `DownloadProgress` classes for managing and tracking download progress. Replace the existing progress tracking code with the following:

```csharp
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
        private readonly DownloadManager downloadManager;

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
            this.downloadManager = new DownloadManager(new DownloaderConfig(), logger);

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

            var downloadedCards = new HashSet<string>(File.Exists(resumeFilePath) ? File.ReadAllLines(resumeFilePath) : Array.Empty<string>());

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

                    // Update progress
                    downloadManager.DownloadCardsAsync(new List<Card> { new Card(cardId, cardName, "", imageUrl, true, DateTime.Now, 0, filePath) }, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error processing card");
                }
            }).ToList();

            await Task.WhenAll(tasks);
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
    }
}
```

### 12. Implement the `DownloadCardImageAsync` method using the `Downloader` class

Update the `DownloadCardImageAsync` method in the `DownloadManager` class to use the `Downloader` class for downloading card images. Replace the existing placeholder code with the following:

```csharp
private async Task DownloadCardImageAsync(Card card, DownloadProgress progress, CancellationToken cancellationToken)
{
    var downloader = new Downloader(_config.BaseUrl, _config.ImageFolder, _config.Delay, _logger, "resume.txt");
    await downloader.DownloadCardImageAsync(card, progress, cancellationToken);
}
```

### 13. Use the `ImageProcessor` class for optimizing images based on quality

Create a new file named `ImageProcessor.cs` in the `GwentCardDownloader` directory and add the following code:

```csharp
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace GwentCardDownloader
{
    public class ImageProcessor
    {
        public async Task ProcessImageAsync(string imagePath, ImageQuality quality)
        {
            switch (quality)
            {
                case ImageQuality.Low:
                    await OptimizeForLowQuality(imagePath);
                    break;
                case ImageQuality.Medium:
                    await OptimizeForMediumQuality(imagePath);
                    break;
                case ImageQuality.High:
                    await OptimizeForHighQuality(imagePath);
                    break;
            }
        }

        private async Task OptimizeForLowQuality(string imagePath)
        {
            using (var image = Image.FromFile(imagePath))
            {
                var resizedImage = ResizeImage(image, 100, 100);
                var grayscaleImage = ConvertToGrayscale(resizedImage);
                grayscaleImage.Save(imagePath, ImageFormat.Jpeg);
            }
            await Task.CompletedTask;
        }

        private async Task OptimizeForMediumQuality(string imagePath)
        {
            using (var image = Image.FromFile(imagePath))
            {
                var resizedImage = ResizeImage(image, 200, 200);
                var brightenedImage = AdjustBrightness(resizedImage, 1.2f);
                brightenedImage.Save(imagePath, ImageFormat.Jpeg);
            }
            await Task.CompletedTask;
        }

        private async Task OptimizeForHighQuality(string imagePath)
        {
            using (var image = Image.FromFile(imagePath))
            {
                var resizedImage = ResizeImage(image, 300, 300);
                var enhancedImage = EnhanceColors(resizedImage);
                enhancedImage.Save(imagePath, ImageFormat.Jpeg);
            }
            await Task.CompletedTask;
        }

        private Image ResizeImage(Image image, int width, int height)
        {
            var resized = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(resized))
            {
                graphics.DrawImage(image, 0, 0, width, height);
            }
            return resized;
        }

        private Image ConvertToGrayscale(Image image)
        {
            var grayscale = new Bitmap(image.Width, image.Height);
            using (var graphics = Graphics.FromImage(grayscale))
            {
                var colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
                    new float[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                    new float[] { 0.11f, 0.11f, 0.11f, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 0, 0, 0, 0, 1 }
                });
                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
            return grayscale;
        }

        private Image AdjustBrightness(Image image, float brightness)
        {
            var adjusted = new Bitmap(image.Width, image.Height);
            using (var graphics = Graphics.FromImage(adjusted))
            {
                var colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] { brightness, 0, 0, 0, 0 },
                    new float[] { 0, brightness, 0, 0, 0 },
                    new float[] { 0, 0, brightness, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 0, 0, 0, 0, 1 }
                });
                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
            return adjusted;
        }

        private Image EnhanceColors(Image image)
        {
            var enhanced = new Bitmap(image.Width, image.Height);
            using (var graphics = Graphics.FromImage(enhanced))
            {
                var colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] { 1.2f, 0, 0, 0, 0 },
                    new float[] { 0, 1.2f, 0, 0, 0 },
                    new float[] { 0, 0, 1.2f, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 0, 0, 0, 0, 1 }
                });
                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
            return enhanced;
        }
    }
}
```

### 14. Specify the desired image quality when calling the `DownloadImage` method

Update the `DownloadImage` method in the `Downloader` class to include a parameter for specifying the desired image quality. Replace the existing method signature with the following:

```csharp
private async Task DownloadImage(string imageUrl, string filePath, ImageQuality quality)
{
    int maxRetries = 3;
    int retryDelay = 2000;

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            var imageBytes = await client.GetByteArrayAsync(imageUrl);
            await File.WriteAllBytesAsync(filePath, imageBytes);

            // Process the image based on the specified quality
            await imageProcessor.ProcessImageAsync(filePath, quality);

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
```

### 15. Call the `ProcessImageAsync` method from the `ImageProcessor` class based on the specified quality

Update the `DownloadImage` method in the `Downloader` class to call the `ProcessImageAsync` method from the `ImageProcessor` class based on the specified quality. Replace the existing method body with the following:

```csharp
private async Task DownloadImage(string imageUrl, string filePath, ImageQuality quality)
{
    int maxRetries = 3;
    int retryDelay = 2000;

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            var imageBytes = await client.GetByteArrayAsync(imageUrl);
            await File.WriteAllBytesAsync(filePath, imageBytes);

            // Process the image based on the specified quality
            await imageProcessor.ProcessImageAsync(filePath, quality);

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
```

### 16. Run the application

Run the application using the following command:

```sh
dotnet run --project GwentCardDownloader
```

The application will start downloading Gwent cards and save them in the `gwent_cards` folder.
