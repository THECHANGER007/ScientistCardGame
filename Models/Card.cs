namespace ScientistCardGame.Models
{
    public class Card
    {
        // Primary Key for database
        public int Id { get; set; }

        // Card Identity
        public string Name { get; set; }
        public string CardType { get; set; }  // "CHARACTER", "DISCOVERY", or "PARADOX"

        // CHARACTER card properties (from our 100 cards Excel)
        public string Country { get; set; }
        public string Field { get; set; }      // SCIENCE, PHILOSOPHY, SPIRITUALITY, HUMANITIES
        public string School { get; set; }     // RATIONALISM, EMPIRICISM, MYSTICISM, IDEALISM, MATERIALISM, HUMANISM
        public string Tier { get; set; }       // LEGENDARY, MASTER, SCHOLAR
        public int ATK { get; set; }
        public int DEF { get; set; }

        // Card Effect (for ALL card types)
        public string SpecialEffect { get; set; }
        public string EffectType { get; set; }  // For DISCOVERY/PARADOX: DRAW, HEAL, NEGATE_ATTACK, etc.

        // Game State Properties (changes during gameplay)
        public int CurrentATK { get; set; }    // Can be modified by effects
        public int CurrentDEF { get; set; }    // Can be modified by effects
        public string Location { get; set; }   // "DECK", "HAND", "FIELD", "GRAVEYARD"
        public bool IsFaceDown { get; set; }   // For PARADOX cards
        public int OwnerId { get; set; }       // Which player owns this card (1 or 2)

        // Effect tracking
        public bool EffectUsedThisTurn { get; set; }
        public bool EffectUsedThisDuel { get; set; }
        public int TimesEffectUsed { get; set; }
        public bool HasAttackedThisTurn { get; set; }
        public string BattlePosition { get; set; } // "ATTACK" or "DEFENSE"

        // DNA: Effect restrictions
        public string EffectFrequency { get; set; } // "ONCE_PER_TURN", "ONCE_PER_DUEL", "UNLIMITED"
        public string EffectTrigger { get; set; } // "ON_SUMMON", "WHEN_DESTROYED", "CONTINUOUS", "GRAVEYARD", "MANUAL"

        // Constructor - creates a new card
        public Card()
        {
            // Default values
            Location = "DECK";
            IsFaceDown = false;
            BattlePosition = "ATTACK"; // ← ADD THIS (default to attack)
            EffectUsedThisTurn = false;
            EffectUsedThisDuel = false;
            TimesEffectUsed = 0;
            EffectFrequency = "ONCE_PER_TURN"; // Default: once per turn
            EffectTrigger = "MANUAL";
        }

        // Method to reset ATK/DEF to original values
        public void ResetStats()
        {
            CurrentATK = ATK;
            CurrentDEF = DEF;
        }
        // DNA: Reset effect usage at end of turn
        public void ResetTurnEffects()
        {
            EffectUsedThisTurn = false;
        }

        // Method to check if card is a character
        public bool IsCharacter()
        {
            return CardType == "CHARACTER";
        }

        // Method to check if card is DISCOVERY
        public bool IsDiscovery()
        {
            return CardType == "DISCOVERY";
        }

        // Method to check if card is PARADOX
        public bool IsParadox()
        {
            return CardType == "PARADOX";
        }
    }
}