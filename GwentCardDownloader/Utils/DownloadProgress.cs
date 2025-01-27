using System;
using System.Collections.Concurrent;
using System.Threading;

namespace GwentCardDownloader.Utils
{
    public class DownloadProgress
    {
        private readonly ConcurrentDictionary<string, (string status, int progress)> _progress;
        private readonly int _totalItems;
        private int _completedItems;

        public DownloadProgress(int totalItems)
        {
            _totalItems = totalItems;
            _progress = new ConcurrentDictionary<string, (string status, int progress)>();
            _completedItems = 0;
        }

        public void UpdateProgress(string itemId, string status, int progress)
        {
            _progress[itemId] = (status, progress);
            if (progress == 100)
            {
                Interlocked.Increment(ref _completedItems);
            }
            DisplayProgress();
        }

        private void DisplayProgress()
        {
            Console.Clear();
            Console.WriteLine($"Progress: {_completedItems}/{_totalItems} items completed");

            foreach (var item in _progress)
            {
                Console.WriteLine($"{item.Key}: {item.Value.status} ({item.Value.progress}%)");
            }
        }
    }
}
