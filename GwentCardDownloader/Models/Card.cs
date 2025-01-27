using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace GwentCardDownloader.Models
{
    public class Card
    {
        // Enums for type safety
        public enum CardType { Unit, Special, Artifact, Stratagem }
        public enum CardRarity { Common, Rare, Epic, Legendary }
        public enum CardColor { Bronze, Gold }
        public enum CardStatus { Active, Retired, Modified }

        // Core Identifiers
        private string _id;
        [Required]
        public string Id
        {
            get => _id;
            init => _id = !string.IsNullOrWhiteSpace(value)
                ? value
                : throw new ArgumentException("Id cannot be empty");
        }

        private string _name;
        [Required]
        [StringLength(100)]
        public string Name
        {
            get => _name;
            set => _name = !string.IsNullOrWhiteSpace(value)
                ? value
                : throw new ArgumentException("Name cannot be empty");
        }

        public string LocalizedName { get; set; }

        // Card Properties
        public string Faction { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CardType Type { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CardRarity Rarity { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CardColor Color { get; set; }
        public int Power { get; set; }
        public int Provisions { get; set; }

        private string _keywords;
        public string Keywords
        {
            get => _keywords;
            set => _keywords = value?.Trim();
        }

        public IEnumerable<string> KeywordsList =>
            Keywords?.Split(',').Select(k => k.Trim()) ?? Enumerable.Empty<string>();

        public string Ability { get; set; }

        private string _categories;
        public string Categories
        {
            get => _categories;
            set => _categories = value?.Trim();
        }

        public IEnumerable<string> CategoriesList =>
            Categories?.Split(',').Select(c => c.Trim()) ?? Enumerable.Empty<string>();

        // Art & Media
        public string ImageUrl { get; set; }
        public string PremiumImageUrl { get; set; }
        public string ArtistName { get; set; }
        public string FlavorText { get; set; }

        // Technical Properties
        public string LocalPath { get; set; }
        public string PremiumLocalPath { get; set; }
        public bool IsDownloaded { get; set; }
        public bool IsPremiumDownloaded { get; set; }
        public DateTime? DownloadDate { get; set; }
        public int RetryCount { get; set; }
        public long FileSize { get; set; }
        public string Checksum { get; set; }

        // Card Set Information
        public string Set { get; set; }
        public DateTime? ReleaseDate { get; set; }

        // Game Status
        public bool IsAvailable { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CardStatus Status { get; set; }

        // Version Control
        public Version Version { get; set; }
        public DateTime? LastModified { get; set; }
        public string PatchNumber { get; set; }

        // Computed Property
        public double StrengthRating => (Power + Provisions) / 2.0;

        public Card()
        {
            RetryCount = 0;
            IsDownloaded = false;
            IsPremiumDownloaded = false;
            IsAvailable = true;
            Status = CardStatus.Active;
        }

        public override string ToString() =>
            $"{Name} ({Id}) - {Faction} {Type}";

        public string GetSafeFileName(bool includeFaction = true)
        {
            var nameBase = includeFaction ? $"{Faction}_{Name}" : Name;
            return string.Join("_", nameBase.Split(Path.GetInvalidFileNameChars()))
                        .Replace(" ", "_")
                        .ToLowerInvariant();
        }

        public bool IsPremium() =>
            !string.IsNullOrEmpty(PremiumImageUrl);

        public string GetImageUrl(bool premium = false) =>
            premium ? PremiumImageUrl : ImageUrl;

        public string GetLocalPath(bool premium = false) =>
            premium ? PremiumLocalPath : LocalPath;

        public bool HasKeyword(string keyword) =>
            KeywordsList.Any(k => k.Equals(keyword, StringComparison.OrdinalIgnoreCase));

        public bool HasCategory(string category) =>
            CategoriesList.Any(c => c.Equals(category, StringComparison.OrdinalIgnoreCase));

        public bool IsValid() =>
            !string.IsNullOrEmpty(Id) &&
            !string.IsNullOrEmpty(Name) &&
            !string.IsNullOrEmpty(ImageUrl);

        public override bool Equals(object obj)
        {
            if (obj is Card other)
            {
                return Id == other.Id;
            }
            return false;
        }

        public override int GetHashCode() =>
            Id?.GetHashCode() ?? 0;

        public Card Clone() =>
            (Card)MemberwiseClone();
    }
}
