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

### 3. Replace the content of `Program.cs` with the provided code

Replace the content of the `Program.cs` file in your project with the following code:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace GwentCardDownloader
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private const string BaseUrl = "https://gwent.one/en/cards/";
        private const string ImageFolder = "gwent_cards";

        static async Task Main(string[] args)
        {
            // Create directory if it doesn't exist
            Directory.CreateDirectory(ImageFolder);

            try
            {
                Console.WriteLine("Starting Gwent card download...");
                await DownloadAllCards();
                Console.WriteLine("Download completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task DownloadAllCards()
        {
            // Get the main page content
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(BaseUrl);

            // Find all card containers
            var cardNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'gwent-card')]");

            if (cardNodes == null)
            {
                throw new Exception("No cards found on the page.");
            }

            int totalCards = cardNodes.Count;
            int currentCard = 0;

            foreach (var cardNode in cardNodes)
            {
                currentCard++;

                try
                {
                    // Extract card details
                    var cardId = cardNode.GetAttributeValue("data-card-id", "");
                    var cardName = cardNode.SelectSingleNode(".//div[contains(@class, 'card-name')]")?.InnerText.Trim();

                    if (string.IsNullOrEmpty(cardId) || string.IsNullOrEmpty(cardName))
                    {
                        Console.WriteLine($"Skipping card - missing information");
                        continue;
                    }

                    // Clean filename
                    string safeFileName = string.Join("_", cardName.Split(Path.GetInvalidFileNameChars()));
                    string filePath = Path.Combine(ImageFolder, $"{safeFileName}.png");

                    // Skip if file already exists
                    if (File.Exists(filePath))
                    {
                        Console.WriteLine($"Skipping existing card: {cardName}");
                        continue;
                    }

                    // Construct image URL (you might need to adjust this based on the website structure)
                    string imageUrl = $"https://gwent.one/image/card/low/{cardId}.jpg";

                    // Download the image
                    await DownloadImage(imageUrl, filePath);

                    Console.WriteLine($"Downloaded ({currentCard}/{totalCards}): {cardName}");

                    // Add a small delay to be nice to the server
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing card: {ex.Message}");
                }
            }
        }

        static async Task DownloadImage(string imageUrl, string filePath)
        {
            try
            {
                var imageBytes = await client.GetByteArrayAsync(imageUrl);
                await File.WriteAllBytesAsync(filePath, imageBytes);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download image from {imageUrl}: {ex.Message}");
            }
        }
    }
}
```

### 4. Run the application

Run the application using the following command:

```sh
dotnet run --project GwentCardDownloader
```

The application will start downloading Gwent cards and save them in the `gwent_cards` folder.
