using System;
using System.Threading.Tasks;
using GwentCardDownloader.Models;
using GwentCardDownloader.Services;
using GwentCardDownloader.Utils;
using Microsoft.Extensions.Logging;

namespace GwentCardDownloader.UI.Services
{
    public class DownloadService
    {
        private readonly ILogger<DownloadService> _logger;
        private readonly DownloaderConfig _config;
        private readonly JsonDataDownloader _jsonDataDownloader;
        private readonly DownloadManager _downloadManager;

        public DownloadService(ILogger<DownloadService> logger, DownloaderConfig config)
        {
            _logger = logger;
            _config = config;
            _jsonDataDownloader = new JsonDataDownloader(config.BaseUrl);
            _downloadManager = new DownloadManager(config, new Logger());
        }

        public async Task DownloadCardMetadataAsync(bool includeImages)
        {
            try
            {
                _logger.LogInformation("Starting metadata download...");

                var cards = await _jsonDataDownloader.DownloadCardDataAsync();

                if (includeImages)
                {
                    await _downloadManager.DownloadCardsAsync(cards, default);
                }

                _logger.LogInformation("Download completed!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the download process.");
                throw;
            }
        }
    }
}
