using System;

namespace GwentCardDownloader.Models
{
    public class CardImage
    {
        public string ArtworkUrl { get; set; }
        public string PremiumArtworkUrl { get; set; }
        public string LocalPath { get; set; }
        public string PremiumLocalPath { get; set; }
        public bool IsDownloaded { get; set; }
        public bool IsPremiumDownloaded { get; set; }
        public DateTime? DownloadDate { get; set; }
        public int RetryCount { get; set; }
        public long FileSize { get; set; }
        public string Checksum { get; set; }

        // Link to Card
        public string CardId { get; set; }
    }
}
