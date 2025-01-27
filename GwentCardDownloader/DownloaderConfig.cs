using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace GwentCardDownloader
{
    public class DownloaderConfig
    {
        public string BaseUrl { get; set; }
        public string ImageFolder { get; set; }
        public int Delay { get; set; }
        public int MaxRetries { get; set; }
        public int MaxConcurrentDownloads { get; set; }
        public bool SkipExisting { get; set; }
        public ImageQuality Quality { get; set; }
        public string UserAgent { get; set; }
        public Dictionary<string, string> Headers { get; set; }

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
    }

    public enum ImageQuality
    {
        Low,
        Medium,
        High
    }
}
