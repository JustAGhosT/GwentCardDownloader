namespace GwentCardDownloader.Models
{
    public record CardDownloadState(
        string CardId,
        bool IsDownloaded,
        int RetryCount
    );
}
