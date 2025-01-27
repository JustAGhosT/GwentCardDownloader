namespace GwentCardDownloader.Models
{
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
}
