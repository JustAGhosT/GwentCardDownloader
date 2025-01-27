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

### 9. Run the application

Run the application using the following command:

```sh
dotnet run --project GwentCardDownloader
```

The application will start downloading Gwent cards and save them in the `gwent_cards` folder.
