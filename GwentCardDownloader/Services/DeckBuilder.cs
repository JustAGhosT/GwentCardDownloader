public class DeckBuilder
    {
        private const int MaxCards = 25;
        private const int MinCards = 25;
        private List<Card> cards = new();
        public Archetype PrimaryArchetype { get; set; }
        public Archetype? SecondaryArchetype { get; set; }
        
        public bool ValidateDeck()
        {
            if (cards.Count < MinCards || cards.Count > MaxCards)
                return false;
                
            // Check if enough cards support primary archetype
            var primaryArchetypeCards = cards.Count(c => c.SupportsArchetype(PrimaryArchetype));
            if (primaryArchetypeCards < 10) // Arbitrary minimum
                return false;
                
            // If secondary archetype exists, check its support
            if (SecondaryArchetype.HasValue)
            {
                var secondaryArchetypeCards = cards.Count(c => c.SupportsArchetype(SecondaryArchetype.Value));
                if (secondaryArchetypeCards < 5) // Arbitrary minimum
                    return false;
            }
            
            // Additional deck building rules...
            return true;
        }
        
        public List<Card> SuggestCards(int count)
        {
            // Suggest cards that synergize with the chosen archetype(s)
            return Card.Database
                .Where(c => c.SupportsArchetype(PrimaryArchetype) || 
                           (SecondaryArchetype.HasValue && c.SupportsArchetype(SecondaryArchetype.Value)))
                .OrderByDescending(c => GetSynergyScore(c))
                .Take(count)
                .ToList();
        }
        
        private double GetSynergyScore(Card card)
        {
            double score = 0;
            
            // Calculate synergy based on archetype support
            if (card.SupportsArchetype(PrimaryArchetype))
                score += 2.0;
            if (SecondaryArchetype.HasValue && card.SupportsArchetype(SecondaryArchetype.Value))
                score += 1.0;
                
            // Add additional synergy calculations...
            
            return score;
        }
    }