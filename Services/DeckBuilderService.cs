using ScientistCardGame.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace ScientistCardGame.Services
{
    public class DeckBuilderService
    {
        private readonly DatabaseService _databaseService;
        private List<Card> _allAvailableCards;

        public DeckBuilderService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _allAvailableCards = new List<Card>();
        }

        // DNA: Load all available cards from database (FIXED - returns List<Card>)
        public async Task<List<Card>> LoadAvailableCardsAsync()
        {
            _allAvailableCards.Clear();

            // Get all cards from database
            var characterCards = await _databaseService.GetAllCharacterCardsAsync();
            var discoveryCards = await _databaseService.GetAllDiscoveryCardsAsync();
            var paradoxCards = await _databaseService.GetAllParadoxCardsAsync();

            _allAvailableCards.AddRange(characterCards);
            _allAvailableCards.AddRange(discoveryCards);
            _allAvailableCards.AddRange(paradoxCards);

            return _allAvailableCards;
        }

        // Get all available cards (for browsing)
        public List<Card> GetAllAvailableCards()
        {
            return _allAvailableCards;
        }

        // DNA: Get cards by FIELD
        public List<Card> GetCardsByField(string field)
        {
            return _allAvailableCards
                .Where(c => c.Field == field && c.CardType == "CHARACTER")
                .OrderByDescending(c => c.ATK)
                .ToList();
        }

        // DNA: Get cards by SCHOOL
        public List<Card> GetCardsBySchool(string school)
        {
            return _allAvailableCards
                .Where(c => c.School == school && c.CardType == "CHARACTER")
                .OrderByDescending(c => c.ATK)
                .ToList();
        }

        // DNA: Get cards by TIER
        public List<Card> GetCardsByTier(string tier)
        {
            return _allAvailableCards
                .Where(c => c.Tier == tier && c.CardType == "CHARACTER")
                .OrderByDescending(c => c.ATK)
                .ToList();
        }

        // Get DISCOVERY cards
        public List<Card> GetDiscoveryCards()
        {
            return _allAvailableCards
                .Where(c => c.CardType == "DISCOVERY")
                .ToList();
        }

        // Get PARADOX cards
        public List<Card> GetParadoxCards()
        {
            return _allAvailableCards
                .Where(c => c.CardType == "PARADOX")
                .ToList();
        }

        // Search cards by name
        public List<Card> SearchCardsByName(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return _allAvailableCards;

            return _allAvailableCards
                .Where(c => c.Name.ToLower().Contains(searchText.ToLower()))
                .ToList();
        }

        // Create a new empty deck
        public Deck CreateNewDeck(string deckName)
        {
            return new Deck
            {
                DeckName = deckName,
                PlayerId = 1
            };
        }

        // DNA: Validate deck follows all rules
        public (bool isValid, string errorMessage) ValidateDeck(Deck deck)
        {
            if (deck.Cards.Count != 30)
            {
                return (false, $"Deck must have exactly 30 cards. Current: {deck.Cards.Count}");
            }

            foreach (var kvp in deck.CardCounts)
            {
                if (kvp.Value > 4)
                {
                    return (false, $"Card '{kvp.Key}' has {kvp.Value} copies. Maximum is 4.");
                }
            }

            return (true, "Deck is valid!");
        }

        // DNA: Create a sample/starter deck
        public async Task<Deck> CreateStarterDeckAsync(string deckName)
        {
            Deck deck = new Deck { DeckName = deckName };
            await LoadAvailableCardsAsync();

            var legendaries = GetCardsByTier("LEGENDARY").Take(3).ToList();
            foreach (var card in legendaries)
            {
                for (int i = 0; i < 2; i++)
                    deck.AddCard(card);
            }

            var masters = GetCardsByTier("MASTER").Take(5).ToList();
            foreach (var card in masters)
            {
                for (int i = 0; i < 2; i++)
                    deck.AddCard(card);
            }

            var scholars = GetCardsByTier("SCHOLAR").Take(4).ToList();
            foreach (var card in scholars)
            {
                deck.AddCard(card);
            }

            var discoveries = GetDiscoveryCards().Take(5).ToList();
            foreach (var card in discoveries)
            {
                deck.AddCard(card);
            }

            var paradoxes = GetParadoxCards().Take(5).ToList();
            foreach (var card in paradoxes)
            {
                deck.AddCard(card);
            }

            return deck;
        }

        // DNA: Create a SCIENCE-focused deck
        public async Task<Deck> CreateScienceDeckAsync()
        {
            Deck deck = new Deck { DeckName = "Science Masters" };
            await LoadAvailableCardsAsync();

            var scienceCards = GetCardsByField("SCIENCE");

            int added = 0;
            foreach (var card in scienceCards)
            {
                if (added >= 20) break;

                int copiesToAdd = card.Tier == "LEGENDARY" ? 2 : 1;
                for (int i = 0; i < copiesToAdd && added < 20; i++)
                {
                    deck.AddCard(card);
                    added++;
                }
            }

            var discoveries = GetDiscoveryCards().Take(5).ToList();
            foreach (var card in discoveries)
                deck.AddCard(card);

            var paradoxes = GetParadoxCards().Take(5).ToList();
            foreach (var card in paradoxes)
                deck.AddCard(card);

            return deck;
        }

        // Get deck statistics
        public DeckStatistics GetDeckStatistics(Deck deck)
        {
            return new DeckStatistics
            {
                TotalCards = deck.Cards.Count,
                CharacterCards = deck.Cards.Count(c => c.CardType == "CHARACTER"),
                DiscoveryCards = deck.Cards.Count(c => c.CardType == "DISCOVERY"),
                ParadoxCards = deck.Cards.Count(c => c.CardType == "PARADOX"),
                LegendaryCount = deck.Cards.Count(c => c.Tier == "LEGENDARY"),
                MasterCount = deck.Cards.Count(c => c.Tier == "MASTER"),
                ScholarCount = deck.Cards.Count(c => c.Tier == "SCHOLAR"),
                AverageATK = deck.Cards.Where(c => c.CardType == "CHARACTER").Average(c => (double?)c.ATK) ?? 0,
                AverageDEF = deck.Cards.Where(c => c.CardType == "CHARACTER").Average(c => (double?)c.DEF) ?? 0,
                ScienceCount = deck.Cards.Count(c => c.Field == "SCIENCE"),
                PhilosophyCount = deck.Cards.Count(c => c.Field == "PHILOSOPHY"),
                SpiritualityCount = deck.Cards.Count(c => c.Field == "SPIRITUALITY"),
                HumanitiesCount = deck.Cards.Count(c => c.Field == "HUMANITIES")
            };
        }

        // DNA: Save deck to file (MOVED HERE - was in wrong class!)
        public async Task<bool> SaveDeckToFile(Deck deck, string fileName)
        {
            try
            {
                string folderPath = Path.Combine(FileSystem.AppDataDirectory, "SavedDecks");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, $"{fileName}.deck");

                var saveData = new
                {
                    DeckName = deck.DeckName,
                    CardNames = deck.Cards.Select(c => c.Name).ToList()
                };

                string json = System.Text.Json.JsonSerializer.Serialize(saveData);
                await File.WriteAllTextAsync(filePath, json);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // DNA: Load deck from file (MOVED HERE - was in wrong class!)
        public async Task<Deck> LoadDeckFromFile(string fileName)
        {
            try
            {
                string folderPath = Path.Combine(FileSystem.AppDataDirectory, "SavedDecks");
                string filePath = Path.Combine(folderPath, $"{fileName}.deck");

                if (!File.Exists(filePath))
                    return null;

                string json = await File.ReadAllTextAsync(filePath);
                var saveData = System.Text.Json.JsonSerializer.Deserialize<DeckSaveData>(json);

                var deck = new Deck
                {
                    DeckName = saveData.DeckName
                };

                var allCards = await LoadAvailableCardsAsync();

                foreach (var cardName in saveData.CardNames)
                {
                    var card = allCards.FirstOrDefault(c => c.Name == cardName);
                    if (card != null)
                    {
                        var cardCopy = new Card
                        {
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
                            EffectTrigger = card.EffectTrigger
                        };
                        deck.AddCard(cardCopy);
                    }
                }

                return deck;
            }
            catch
            {
                return null;
            }
        }

        // DNA: Get list of saved deck names (MOVED HERE - was in wrong class!)
        public List<string> GetSavedDeckNames()
        {
            string folderPath = Path.Combine(FileSystem.AppDataDirectory, "SavedDecks");

            if (!Directory.Exists(folderPath))
                return new List<string>();

            var files = Directory.GetFiles(folderPath, "*.deck");
            return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
        }

        // Helper class for serialization
        private class DeckSaveData
        {
            public string DeckName { get; set; }
            public List<string> CardNames { get; set; }
        }
    }

    // Helper class for deck statistics (CLEANED UP - removed methods that don't belong here!)
    public class DeckStatistics
    {
        public int TotalCards { get; set; }
        public int CharacterCards { get; set; }
        public int DiscoveryCards { get; set; }
        public int ParadoxCards { get; set; }
        public int LegendaryCount { get; set; }
        public int MasterCount { get; set; }
        public int ScholarCount { get; set; }
        public double AverageATK { get; set; }
        public double AverageDEF { get; set; }
        public int ScienceCount { get; set; }
        public int PhilosophyCount { get; set; }
        public int SpiritualityCount { get; set; }
        public int HumanitiesCount { get; set; }

        public override string ToString()
        {
            return $"Total: {TotalCards}/30\n" +
                   $"Characters: {CharacterCards} (L:{LegendaryCount} M:{MasterCount} S:{ScholarCount})\n" +
                   $"Discovery: {DiscoveryCards} | Paradox: {ParadoxCards}\n" +
                   $"Avg ATK: {AverageATK:F0} | Avg DEF: {AverageDEF:F0}\n" +
                   $"Fields: Sci:{ScienceCount} Phil:{PhilosophyCount} Spir:{SpiritualityCount} Hum:{HumanitiesCount}";
        }
    }
}