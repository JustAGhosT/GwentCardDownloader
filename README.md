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

### 7. Replace the content of `Program.cs` with the provided code

Replace the content of the `Program.cs` file in your project with the following code:

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
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
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

            // Set User-Agent header
            client.DefaultRequestHeaders.UserAgent.ParseAdd("GwentCardDownloader/1.0");

            try
            {
                logger.Info("Starting Gwent card download...");
                await DownloadAllCards(baseUrl, imageFolder, delay);
                logger.Info("Download completed!");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task DownloadAllCards(string baseUrl, string imageFolder, int delay)
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

            int totalCards = cardNodes.Count;
            int currentCard = 0;

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

        static async Task DownloadImage(string imageUrl, string filePath)
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

        static bool VerifyImage(string filePath)
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

### 8. Run the application

Run the application using the following command:

```sh
dotnet run --project GwentCardDownloader
```

The application will start downloading Gwent cards and save them in the `gwent_cards` folder.
