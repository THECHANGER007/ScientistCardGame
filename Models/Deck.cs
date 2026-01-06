using System.Collections.Generic;

namespace ScientistCardGame.Models
{
    public class Deck
    {
        public int DeckId { get; set; }
        public string DeckName { get; set; }
        public int PlayerId { get; set; }

        // DNA Rule: 30 cards per deck
        public List<Card> Cards { get; set; }

        // Track card counts (for 4-copy limit rule)
        public Dictionary<string, int> CardCounts { get; set; }

        public Deck()
        {
            Cards = new List<Card>();
            CardCounts = new Dictionary<string, int>();
            DeckName = "New Deck";
        }

        // DNA: Check if deck is valid (exactly 30 cards)
        public bool IsValid()
        {
            return Cards.Count == 30;
        }

        // DNA: Check if we can add a card (with all new limits!)
        public bool CanAddCard(Card card)
        {
            // Already at 30 cards?
            if (Cards.Count >= 30)
                return false;

            // DNA NEW RULE: Max 5 PARADOX cards per deck
            if (card.CardType == "PARADOX")
            {
                int paradoxCount = Cards.Count(c => c.CardType == "PARADOX");
                if (paradoxCount >= 5)
                    return false;
            }

            // DNA NEW RULE: Max 5 DISCOVERY cards per deck
            if (card.CardType == "DISCOVERY")
            {
                int discoveryCount = Cards.Count(c => c.CardType == "DISCOVERY");
                if (discoveryCount >= 5)
                    return false;
            }

            // DNA NEW RULE: Max 6 LEGENDARY cards total per deck
            if (card.Tier == "LEGENDARY")
            {
                int legendaryCount = Cards.Count(c => c.Tier == "LEGENDARY");
                if (legendaryCount >= 6)
                    return false;
            }

            // Check copy limit
            if (CardCounts.ContainsKey(card.Name))
            {
                // DNA NEW RULE: LEGENDARY, PARADOX, and DISCOVERY cards max 2 copies each
                if (card.Tier == "LEGENDARY" || card.CardType == "PARADOX" || card.CardType == "DISCOVERY")
                {
                    return CardCounts[card.Name] < 2; // Max 2 legendary copies
                }

                // Regular cards: max 4 copies
                return CardCounts[card.Name] < 4;
            }

            return true; // First copy, can add
        }

        // Add a card to deck
        public bool AddCard(Card card)
        {
            if (!CanAddCard(card))
                return false;

            // Create a copy of the card (so we don't modify the original)
            Card cardCopy = new Card
            {
                Id = card.Id,
                Name = card.Name,
                CardType = card.CardType,
                Country = card.Country,
                Field = card.Field,
                School = card.School,
                Tier = card.Tier,
                ATK = card.ATK,
                DEF = card.DEF,
                CurrentATK = card.ATK,
                CurrentDEF = card.DEF,
                SpecialEffect = card.SpecialEffect,
                EffectType = card.EffectType,
                Location = "DECK",
                IsFaceDown = false
            };

            Cards.Add(cardCopy);

            // Update count
            if (CardCounts.ContainsKey(card.Name))
                CardCounts[card.Name]++;
            else
                CardCounts[card.Name] = 1;

            return true;
        }

        // Remove a card from deck
        public bool RemoveCard(string cardName)
        {
            var card = Cards.Find(c => c.Name == cardName);
            if (card == null)
                return false;

            Cards.Remove(card);

            // Update count
            if (CardCounts.ContainsKey(cardName))
            {
                CardCounts[cardName]--;
                if (CardCounts[cardName] == 0)
                    CardCounts.Remove(cardName);
            }

            return true;
        }

        // Get count of specific card in deck
        public int GetCardCount(string cardName)
        {
            if (CardCounts.ContainsKey(cardName))
                return CardCounts[cardName];
            return 0;
        }

        // DNA: Get summary (how many of each type)
        public string GetSummary()
        {
            int characterCount = 0;
            int discoveryCount = 0;
            int paradoxCount = 0;

            foreach (var card in Cards)
            {
                if (card.CardType == "CHARACTER")
                    characterCount++;
                else if (card.CardType == "DISCOVERY")
                    discoveryCount++;
                else if (card.CardType == "PARADOX")
                    paradoxCount++;
            }

            return $"Total: {Cards.Count}/30\n" +
                   $"Characters: {characterCount}\n" +
                   $"Discovery: {discoveryCount}\n" +
                   $"Paradox: {paradoxCount}";
        }

        // Shuffle deck (for game start)
        public void Shuffle()
        {
            Random rng = new Random();
            int n = Cards.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card temp = Cards[k];
                Cards[k] = Cards[n];
                Cards[n] = temp;
            }
        }
    }
}