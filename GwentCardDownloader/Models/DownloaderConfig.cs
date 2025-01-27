using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace GwentCardDownloader.Models
{
    public class DownloaderConfig
    {
        private string _baseUrl;

        public string BaseUrl
        {
            get => _baseUrl;
            set => _baseUrl = Uri.IsWellFormedUriString(value, UriKind.Absolute)
                ? value
                : throw new ArgumentException("Invalid URL format");
        }

        public string ImageFolder { get; set; }
        public int Delay { get; set; }
        public int MaxRetries { get; set; }
        public int MaxConcurrentDownloads { get; set; }
        public bool SkipExisting { get; set; }
        public ImageQuality Quality { get; set; }
        public string UserAgent { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public RateLimitConfig RateLimit { get; set; } = new();

        public DownloaderConfig()
        {
            BaseUrl = "https://gwent.one/en/cards/";
            ImageFolder = "gwent_cards";
            Delay = 100;
            MaxRetries = 3;
            MaxConcurrentDownloads = 5;
            SkipExisting = true;
            Quality = ImageQuality.Low;
            UserAgent = "GwentCardDownloader/1.0";
            Headers = new Dictionary<string, string>();
        }

        public void Save(string filePath)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static DownloaderConfig Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new DownloaderConfig();
            }

            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<DownloaderConfig>(json);
        }

        public bool Validate()
        {
            return !string.IsNullOrEmpty(BaseUrl)
                && Delay >= 0
                && MaxRetries > 0
                && MaxConcurrentDownloads > 0;
        }

        public class RateLimitConfig
        {
            public int RequestsPerMinute { get; set; } = 60;
            public int BurstSize { get; set; } = 10;
        }
    }

    public enum ImageQuality
    {
        Low,
        Medium,
        High
    }
}
