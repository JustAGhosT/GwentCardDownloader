using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using GwentCardDownloader.Models.Enums;  // Assuming enums are in this namespace

namespace GwentCardDownloader.Models
{
    public class Card
    {
        #region Properties
        // Core Identifiers
        [Required]
        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "Card ID must be a 6-digit number")]
        public string Id { get; init; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string LocalizedName { get; set; }

        // Card Properties
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Faction Faction { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CardType Type { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Rarity Rarity { get; set; }
        
        [Range(0, 30, ErrorMessage = "Power must be between 0 and 30")]
        public int Power { get; set; }
        
        [Range(4, 15, ErrorMessage = "Provisions must be between 4 and 15")]
        public int Provisions { get; set; }

        // Collections
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HashSet<Category> Categories { get; set; } = new();

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HashSet<Keyword> Keywords { get; set; } = new();

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HashSet<Effect> Effects { get; set; } = new();

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HashSet<Archetype> SupportedArchetypes { get; set; } = new();

        public string Ability { get; set; }

        public string ArtistName { get; set; }
        public string FlavorText { get; set; }

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

        // Static card database
        public static List<Card> Database { get; private set; } = new();
        #endregion

        #region Constructors
        public Card()
        {
            IsAvailable = true;
            Status = CardStatus.Active;
        }
        #endregion

        #region Methods
        public override string ToString() =>
            $"{Name} ({Id}) - {Faction} {Type}";

        public string GetSafeFileName(bool includeFaction = true)
        {
            var nameBase = includeFaction ? $"{Faction}_{Name}" : Name;
            return string.Join("_", nameBase.Split(Path.GetInvalidFileNameChars()))
                        .Replace(" ", "_")
                        .ToLowerInvariant();
        }

        // Category checking methods
        public bool HasCategory(Category category) => Categories.Contains(category);
        public bool HasKeyword(Keyword keyword) => Keywords.Contains(keyword);
        public bool HasEffect(Effect effect) => Effects.Contains(effect);

        public bool BelongsToArchetype(Category category)
        {
            // Your existing BelongsToArchetype implementation
        }

        public bool SupportsArchetype(Archetype archetype)
        {
            // Your existing SupportsArchetype implementation
        }

        public bool IsValid() =>
            !string.IsNullOrEmpty(Id) &&
            !string.IsNullOrEmpty(Name) &&
            ValidateProvisions() &&
            ValidatePower();

        private bool ValidateProvisions()
        {
            return Rarity switch
            {
                Rarity.Bronze => Provisions >= 4 && Provisions <= 8,
                Rarity.Gold => Provisions >= 7 && Provisions <= 15,
                _ => false
            };
        }

        private bool ValidatePower()
        {
            return Type switch
            {
                CardType.Unit => Power >= 1 && Power <= 30,
                CardType.Special or CardType.Artifact => Power == 0,
                CardType.Stratagem => Power == 0,
                _ => false
            };
        }

        // Static factory method
        public static Card Create(
            string id,
            string name,
            Faction faction,
            int power,
            int provisions,
            Rarity rarity,
            CardType type,
            IEnumerable<Category> categories = null,
            IEnumerable<Keyword> keywords = null,
            IEnumerable<Effect> effects = null,
            IEnumerable<Archetype> archetypes = null)
        {
            var card = new Card
            {
                Id = id,
                Name = name,
                Faction = faction,
                Power = power,
                Provisions = provisions,
                Rarity = rarity,
                Type = type,
                Categories = new HashSet<Category>(categories ?? Enumerable.Empty<Category>()),
                Keywords = new HashSet<Keyword>(keywords ?? Enumerable.Empty<Keyword>()),
                Effects = new HashSet<Effect>(effects ?? Enumerable.Empty<Effect>()),
                SupportedArchetypes = new HashSet<Archetype>(archetypes ?? Enumerable.Empty<Archetype>())
            };

            Database.Add(card);
            return card;
        }

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

        public Card Clone()
        {
            return new Card
            {
                Id = Id,
                Name = Name,
                Faction = Faction,
                Power = Power,
                Provisions = Provisions,
                Rarity = Rarity,
                Type = Type,
                Ability = Ability,
                Categories = new HashSet<Category>(Categories),
                Keywords = new HashSet<Keyword>(Keywords),
                Effects = new HashSet<Effect>(Effects),
                SupportedArchetypes = new HashSet<Archetype>(SupportedArchetypes),
                ArtistName = ArtistName,
                FlavorText = FlavorText,
                Set = Set,
                ReleaseDate = ReleaseDate,
                IsAvailable = IsAvailable,
                Status = Status,
                Version = Version,
                LastModified = LastModified,
                PatchNumber = PatchNumber
            };
        }
        #endregion
    }
}
