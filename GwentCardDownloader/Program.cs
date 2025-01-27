using System;
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

                var downloader = new Downloader(baseUrl, imageFolder, delay, new Logger(), resumeFilePath);
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
