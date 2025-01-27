using System;
using System.Collections.Generic;
using NDesk.Options;

namespace GwentCardDownloader
{
    public class CommandLineParser
    {
        public DownloaderConfig ParseArguments(string[] args)
        {
            var config = new DownloaderConfig();
            
            var options = new OptionSet {
                { "u|url=", "Base URL for card downloads", v => config.BaseUrl = v },
                { "f|folder=", "Output folder for images", v => config.ImageFolder = v },
                { "d|delay=", "Delay between downloads (ms)", (int v) => config.Delay = v },
                { "r|retries=", "Maximum retry attempts", (int v) => config.MaxRetries = v },
                { "c|concurrent=", "Max concurrent downloads", (int v) => config.MaxConcurrentDownloads = v },
                { "q|quality=", "Image quality (Low/Medium/High)", v => config.Quality = Enum.Parse<ImageQuality>(v) },
                { "s|skip", "Skip existing files", v => config.SkipExisting = v != null },
                { "h|help", "Show this message and exit", v => config.Help = v != null }
            };

            options.Parse(args);

            if (config.Help)
            {
                ShowHelp(options);
                return null;
            }

            return config;
        }

        private void ShowHelp(OptionSet options)
        {
            Console.WriteLine("Usage: GwentCardDownloader [OPTIONS]");
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
        }
    }
}
