namespace GwentCardDownloader.Models
{
    public class CardDownloadState
    {
        public string CardId { get; set; }
        public bool IsDownloaded { get; set; }
        public int RetryCount { get; set; }
    }
}
