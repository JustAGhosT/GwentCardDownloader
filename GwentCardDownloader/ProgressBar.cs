using ShellProgressBar;
using System;

namespace GwentCardDownloader
{
    public class ProgressBar
    {
        private readonly ShellProgressBar.ProgressBar progressBar;

        public ProgressBar(int totalTicks, string message)
        {
            var options = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                BackgroundColor = ConsoleColor.DarkGray,
                ProgressCharacter = '─'
            };

            progressBar = new ShellProgressBar.ProgressBar(totalTicks, message, options);
        }

        public void Tick(string message = null)
        {
            progressBar.Tick(message);
        }

        public void Dispose()
        {
            progressBar.Dispose();
        }
    }
}
