using System;
using System.IO;

namespace GwentCardDownloader
{
    public class Card
    {
        // Core Identifiers
        public string Id { get; set; }
        public string Name { get; set; }
        public string LocalizedName { get; set; }

        // Card Properties
        public string Faction { get; set; }
        public string Type { get; set; }  // Unit, Special, Artifact, Stratagem
        public string Rarity { get; set; }  // Common, Rare, Epic, Legendary
        public string Color { get; set; }  // Bronze, Gold
        public int Power { get; set; }
        public int Provisions { get; set; }
        public string Keywords { get; set; }  // Comma-separated keywords like "Deploy", "Order", etc.
        public string Ability { get; set; }  // Card text/description
        public string Categories { get; set; }  // Soldier, Warrior, etc.

        // Art & Media
        public string ImageUrl { get; set; }
        public string PremiumImageUrl { get; set; }  // Animated/Premium version
        public string ArtistName { get; set; }
        public string FlavorText { get; set; }  // Lore text

        // Technical Properties
        public string LocalPath { get; set; }
        public string PremiumLocalPath { get; set; }
        public bool IsDownloaded { get; set; }
        public bool IsPremiumDownloaded { get; set; }
        public DateTime? DownloadDate { get; set; }
        public int RetryCount { get; set; }

        // Card Set Information
        public string Set { get; set; }  // Base Set, Way of the Witcher, etc.
        public string ReleaseDate { get; set; }

        // Game Status
        public bool IsAvailable { get; set; }  // Whether card is currently in the game
        public string Status { get; set; }  // Active, Retired, Modified, etc.

        // Version Control
        public string Version { get; set; }
        public DateTime? LastModified { get; set; }
        public string PatchNumber { get; set; }

        public Card()
        {
            // Initialize collections and default values
            RetryCount = 0;
            IsDownloaded = false;
            IsPremiumDownloaded = false;
        }

        public override string ToString()
        {
            return $"{Name} ({Id}) - {Faction} {Type}";
        }

        // Helper method to generate safe filename
        public string GetSafeFileName()
        {
            return string.Join("_", Name.Split(Path.GetInvalidFileNameChars()));
        }

        // Helper method to check if the card is premium
        public bool IsPremium()
        {
            return !string.IsNullOrEmpty(PremiumImageUrl);
        }

        // Helper method to get appropriate image URL based on quality
        public string GetImageUrl(bool premium = false)
        {
            return premium ? PremiumImageUrl : ImageUrl;
        }

        // Helper method to get appropriate local path based on quality
        public string GetLocalPath(bool premium = false)
        {
            return premium ? PremiumLocalPath : LocalPath;
        }
    }
}
