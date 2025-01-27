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
        #region Enums
        public enum CardType { Unit, Special, Artifact, Stratagem }
        public enum CardRarity { Common, Rare, Epic, Legendary }
        public enum CardColor { Bronze, Gold }
        public enum CardStatus { Active, Retired, Modified }

        public enum Faction
        {
            Neutral,
            NorthernRealms,
            Nilfgaard,
            Monsters,
            Skellige,
            ScoiaTael,
            Syndicate
        }

        public enum Category
        {
            // Combat Units
            Soldier,
            Warrior,
            Knight,
            Agent,
            Pirate,
            
            // Races
            Elf,
            Dwarf,
            Human,
            Vampire,
            Dragon,
            Beast,
            
            // Special Types
            Construct,
            Machine,
            Ship,
            
            // Magic & Supernatural
            Mage,
            Witch,
            Cursed,
            Specter,
            WildHunt,
            Relict,
            Insectoid,
            
            // Social Classes
            Aristocrat,
            Criminal,
            Cultist,
            
            // Card Types
            Tactic,
            Alchemy,
            Crime,
            Organic,
            
            // Syndicate-specific
            Crownsplitter,
            Firesworn,
            
            // Location
            City,
            Fortress
        }

        public enum Keyword
        {
            // Core Keywords
            Deploy,
            Order,
            Zeal,
            Adrenaline,
            
            // Faction-specific
            Formation,     // NR
            Crew,         // NR
            Resupply,     // NR
            Shield,       // NR
            
            Assimilate,   // NG
            Soldier,      // NG
            Spying,       // NG
            
            Deathwish,    // MO
            Thrive,       // MO
            Consume,      // MO
            Dominance,    // MO
            
            Bloodthirst,  // SK
            Berserk,      // SK
            Veteran,      // SK
            
            Harmony,      // ST
            Movement,     // ST
            Symbiosis,    // ST
            
            Tribute,      // SY
            Fee,          // SY
            Hoard,        // SY
            Profit,       // SY
            Insanity,     // SY
            
            // Generic
            Devotion,
            Echo,
            Doomed,
            Counter
        }

        public enum Effect
        {
            // Offensive
            Damage,
            Destroy,
            Poison,
            Seize,
            Banish,
            
            // Defensive/Utility
            Lock,
            Boost,
            Shield,
            Heal,
            Reset,
            
            // Movement
            Move,
            Swap,
            
            // Card Manipulation
            Draw,
            Discard,
            Transform,
            Spawn,
            Create,
            
            // Status Effects
            Bleeding,
            Vitality,
            Resilience,
            Immunity
        }
        #endregion

        #region Properties
        // Core Identifiers
        private string _id;
        [Required]
        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "Card ID must be a 6-digit number")]
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
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Faction Faction { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CardType Type { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CardRarity Rarity { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CardColor Color { get; set; }
        
        [Range(0, 30, ErrorMessage = "Power must be between 0 and 30")]
        public int Power { get; set; }
        
        [Range(4, 15, ErrorMessage = "Provisions must be between 4 and 15")]
        public int Provisions { get; set; }

        private HashSet<Keyword> _keywords = new();
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HashSet<Keyword> Keywords
        {
            get => _keywords;
            set => _keywords = value ?? new HashSet<Keyword>();
        }

        public string Ability { get; set; }

        private HashSet<Category> _categories = new();
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HashSet<Category> Categories
        {
            get => _categories;
            set => _categories = value ?? new HashSet<Category>();
        }

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
        #endregion

        #region Constructors
        public Card()
        {
            RetryCount = 0;
            IsDownloaded = false;
            IsPremiumDownloaded = false;
            IsAvailable = true;
            Status = CardStatus.Active;
            Keywords = new HashSet<Keyword>();
            Categories = new HashSet<Category>();
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

        public bool IsPremium() =>
            !string.IsNullOrEmpty(PremiumImageUrl);

        public string GetImageUrl(bool premium = false) =>
            premium ? PremiumImageUrl : ImageUrl;

        public string GetLocalPath(bool premium = false) =>
            premium ? PremiumLocalPath : LocalPath;

        public bool HasKeyword(Keyword keyword) =>
            Keywords.Contains(keyword);

        public bool HasCategory(Category category) =>
            Categories.Contains(category);

        public bool BelongsToArchetype(Category category)
        {
            return (Faction, category) switch
            {
                // Nilfgaard Archetypes
                (Faction.Nilfgaard, Category.Soldier) => HasCategory(category),
                (Faction.Nilfgaard, Category.Agent) => HasCategory(category),
                (Faction.Nilfgaard, Category.Aristocrat) => HasCategory(category),
                
                // Monsters Archetypes
                (Faction.Monsters, Category.Vampire) => HasCategory(category),
                (Faction.Monsters, Category.WildHunt) => HasCategory(category),
                (Faction.Monsters, Category.Insectoid) => HasCategory(category),
                (Faction.Monsters, Category.Beast) => HasCategory(category),
                (Faction.Monsters, Category.Relict) => HasCategory(category),
                
                // Skellige Archetypes
                (Faction.Skellige, Category.Warrior) => HasCategory(category),
                (Faction.Skellige, Category.Pirate) => HasCategory(category),
                (Faction.Skellige, Category.Ship) => HasCategory(category),
                (Faction.Skellige, Category.Cultist) => HasCategory(category),
                (Faction.Skellige, Category.Beast) => HasCategory(category),
                
                // Scoia'tael Archetypes
                (Faction.ScoiaTael, Category.Elf) => HasCategory(category),
                (Faction.ScoiaTael, Category.Dwarf) => HasCategory(category),
                (Faction.ScoiaTael, Category.Human) => HasCategory(category),
                (Faction.ScoiaTael, Category.Dragon) => HasCategory(category),
                
                // Northern Realms Archetypes
                (Faction.NorthernRealms, Category.Soldier) => HasCategory(category),
                (Faction.NorthernRealms, Category.Knight) => HasCategory(category),
                (Faction.NorthernRealms, Category.Machine) => HasCategory(category),
                (Faction.NorthernRealms, Category.Mage) => HasCategory(category),
                (Faction.NorthernRealms, Category.Witch) => HasCategory(category),
                
                // Syndicate Archetypes
                (Faction.Syndicate, Category.Criminal) => HasCategory(category),
                (Faction.Syndicate, Category.Crownsplitter) => HasCategory(category),
                (Faction.Syndicate, Category.Firesworn) => HasCategory(category),
                
                // Generic Categories (can appear in any faction)
                (_, Category.Tactic or 
                   Category.Alchemy or 
                   Category.Organic or 
                   Category.City or 
                   Category.Fortress) => HasCategory(category),
                
                // Default case
                _ => false
            };
        }

        public bool IsValid() =>
            !string.IsNullOrEmpty(Id) &&
            !string.IsNullOrEmpty(Name) &&
            !string.IsNullOrEmpty(ImageUrl) &&
            ValidateProvisions() &&
            ValidatePower();

        private bool ValidateProvisions()
        {
            return Color switch
            {
                CardColor.Bronze => Provisions >= 4 && Provisions <= 8,
                CardColor.Gold => Provisions >= 7 && Provisions <= 15,
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
        #endregion
    }
}
