using System.Collections.Generic;
using System.Linq;

namespace ScientistCardGame.Models
{
    public class Player
    {
        // Player Identity
        public int PlayerId { get; set; }  // 1 or 2
        public string PlayerName { get; set; }

        // Life Points (from DNA: each player starts with 20,000 HP)
        public int HP { get; set; }
        public int MaxHP { get; set; }

        // Card Collections
        public List<Card> Deck { get; set; }           // 30 cards (DNA rule)
        public List<Card> Hand { get; set; }           // Cards drawn from deck
        public List<Card> Field { get; set; }          // Cards currently in battle
        public List<Card> Graveyard { get; set; }      // Destroyed/discarded cards

        // Game State
        public bool IsMyTurn { get; set; }

        // DNA NEW: Track summons this turn
        public int SummonsThisTurn { get; set; }  // ← ADD THIS
        public int MaxSummonsPerTurn { get; set; } = 2;  // ← ADD THIS
        public string SpecialEffect { get; set; } = ""; // ← ADD THIS

        // Constructor - creates a new player
        public Player(int playerId, string playerName)
        {
            PlayerId = playerId;
            PlayerName = playerName;

            // DNA Rule: 20,000 HP starting
            HP = 20000;
            MaxHP = 20000;

            // Initialize card collections
            Deck = new List<Card>();
            Hand = new List<Card>();
            Field = new List<Card>();
            Graveyard = new List<Card>();

            IsMyTurn = false;
            SummonsThisTurn = 0;
            SpecialEffect = ""; // ← ADD THIS
        }

        // DNA: Draw 1 card per turn
        public Card DrawCard()
        {
            if (Deck.Count == 0)
            {
                return null; // Deck is empty
            }

            Card drawnCard = Deck[0];
            Deck.RemoveAt(0);
            drawnCard.Location = "HAND";
            Hand.Add(drawnCard);

            return drawnCard;
        }

        // Play a card from hand to field
        public bool PlayCardToField(Card card)
        {
            if (!Hand.Contains(card))
            {
                return false; // Card not in hand
            }

            Hand.Remove(card);
            card.Location = "FIELD";
            Field.Add(card);

            return true;
        }

        // Send a card to graveyard
        public void SendToGraveyard(Card card)
        {
            // Remove from wherever it is
            Hand.Remove(card);
            Field.Remove(card);
            Deck.Remove(card);

            // Add to graveyard
            card.Location = "GRAVEYARD";
            Graveyard.Add(card);
        }

        // DNA: Take damage (reduce HP)
        public void TakeDamage(int damage)
        {
            HP -= damage;
            if (HP < 0)
            {
                HP = 0; // Can't go negative
            }
        }

        // DNA: Heal HP
        public void Heal(int healAmount)
        {
            HP += healAmount;
            if (HP > MaxHP)
            {
                HP = MaxHP; // Can't exceed max HP
            }
        }

        // Check if player lost (DNA: first to 0 HP loses)
        public bool HasLost()
        {
            return HP <= 0;
        }

        // DNA: Get cards of specific FIELD on field
        public List<Card> GetFieldCards(string field)
        {
            return Field.Where(c => c.Field == field && c.IsCharacter()).ToList();
        }

        // DNA: Get cards of specific SCHOOL on field
        public List<Card> GetSchoolCards(string school)
        {
            return Field.Where(c => c.School == school && c.IsCharacter()).ToList();
        }

        // DNA: Count cards of specific FIELD
        public int CountFieldCards(string field)
        {
            return GetFieldCards(field).Count;
        }

        // DNA: Count cards of specific SCHOOL
        public int CountSchoolCards(string school)
        {
            return GetSchoolCards(school).Count;
        }
    }
}