using System;
using System.Collections.Generic;
using ShellProgressBar;

namespace GwentCardDownloader.Utils
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
