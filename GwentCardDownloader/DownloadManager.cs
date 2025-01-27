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
