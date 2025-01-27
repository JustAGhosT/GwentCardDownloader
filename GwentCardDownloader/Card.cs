using System;

namespace GwentCardDownloader
{
    public class Card
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Faction { get; set; }
        public string ImageUrl { get; set; }
        public bool IsDownloaded { get; set; }
        public DateTime? DownloadDate { get; set; }
        public int RetryCount { get; set; }
        public string LocalPath { get; set; }

        public Card(string id, string name, string faction, string imageUrl, bool isDownloaded, DateTime? downloadDate, int retryCount, string localPath)
        {
            Id = id;
            Name = name;
            Faction = faction;
            ImageUrl = imageUrl;
            IsDownloaded = isDownloaded;
            DownloadDate = downloadDate;
            RetryCount = retryCount;
            LocalPath = localPath;
        }
    }
}
