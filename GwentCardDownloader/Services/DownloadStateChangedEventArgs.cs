using System;

namespace GwentCardDownloader.Services
{
    public class DownloadStateChangedEventArgs : EventArgs
    {
        public string CardId { get; }
        public bool IsDownloaded { get; }
        public DateTime Timestamp { get; }

        public DownloadStateChangedEventArgs(string cardId, bool isDownloaded, DateTime timestamp)
        {
            CardId = cardId;
            IsDownloaded = isDownloaded;
            Timestamp = timestamp;
        }
    }
}
