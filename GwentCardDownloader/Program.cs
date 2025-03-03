using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using GwentCardDownloader.Models;
using GwentCardDownloader.Services;
using GwentCardDownloader.Utils;

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
            var commandLineParser = new CommandLineParser();
            var config = commandLineParser.ParseArguments(args);

            if (config == null)
            {
                return;
            }

            // Create directory if it doesn't exist
            Directory.CreateDirectory(config.ImageFolder);

            // Configure logging
            var nlogConfig = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "logfile.txt" };
            nlogConfig.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            LogManager.Configuration = nlogConfig;

            try
            {
                logger.Info("Starting Gwent card download...");

                var stateManager = new StateManager(resumeFilePath);
                var errorHandler = new ErrorHandler(new Logger());

                // Check if the --includeimages flag is present
                bool includeImages = args.Contains("--includeimages");

                // Download card metadata
                var jsonDataDownloader = new JsonDataDownloader(config.BaseUrl);
                var cards = await jsonDataDownloader.DownloadCardDataAsync();

                if (includeImages)
                {
                    var downloadManager = new DownloadManager(config, new Logger());
                    await downloadManager.DownloadCardsAsync(cards, default);
                }

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
