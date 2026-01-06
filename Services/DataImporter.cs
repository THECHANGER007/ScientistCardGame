using ExcelDataReader;
using ScientistCardGame.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace ScientistCardGame.Services
{
    public class DataImporter
    {
        private readonly DatabaseService _databaseService;

        public DataImporter(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            // Required for ExcelDataReader to work
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        // DNA: Import all 120 cards from both Excel files
        public async Task<bool> ImportAllCardsAsync(string scientistsFilePath, string specialCardsFilePath)
        {
            try
            {
                // Clear existing data
                await _databaseService.DeleteAllCardsAsync();

                // Import 100 character cards
                List<Card> characterCards = ImportCharacterCards(scientistsFilePath);
                await _databaseService.SaveCardsAsync(characterCards);

                // Import 20 special cards (DISCOVERY + PARADOX)
                List<Card> specialCards = ImportSpecialCards(specialCardsFilePath);
                await _databaseService.SaveCardsAsync(specialCards);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Import failed: {ex.Message}");
                return false;
            }
        }

        // DNA: Import 100 scientists from Scientists_Cards_100_With_Effects.xlsx
        private List<Card> ImportCharacterCards(string filePath)
        {
            List<Card> cards = new List<Card>();
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    var table = result.Tables[0]; // First sheet
                                                  // Start from row 1 (row 0 is header)
                    for (int i = 1; i < table.Rows.Count; i++)
                    {
                        DataRow row = table.Rows[i];
                        Card card = new Card
                        {
                            CardType = "CHARACTER",
                            // Column 0: NAME
                            Name = row[0]?.ToString() ?? "",
                            // Column 1: COUNTRY
                            Country = row[1]?.ToString() ?? "",
                            // Column 2: FIELD
                            Field = row[2]?.ToString() ?? "",
                            // Column 3: SCHOOL
                            School = row[3]?.ToString() ?? "",
                            // Column 4: TIER
                            Tier = row[4]?.ToString() ?? "",
                            // Column 5: ATK
                            ATK = int.Parse(row[5]?.ToString() ?? "0"),
                            // Column 6: DEF
                            DEF = int.Parse(row[6]?.ToString() ?? "0"),
                            // Column 7: SPECIAL_EFFECT
                            SpecialEffect = row[7]?.ToString() ?? "",
                            Location = "DECK",
                            IsFaceDown = false
                        };
                        card.CurrentATK = card.ATK;
                        card.CurrentDEF = card.DEF;

                        // DNA: Determine when effect should trigger
                        card.EffectTrigger = DetermineEffectTrigger(card.Name, card.SpecialEffect);

                        cards.Add(card);
                    }
                }
            }
            return cards;
        }

        // DNA: Import 20 special cards from Special_Cards_20.xlsx
        private List<Card> ImportSpecialCards(string filePath)
        {
            List<Card> cards = new List<Card>();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    var table = result.Tables[0]; // First sheet

                    // Start from row 1 (row 0 is header)
                    for (int i = 1; i < table.Rows.Count; i++)
                    {
                        DataRow row = table.Rows[i];

                        Card card = new Card
                        {
                            // Column 0: CARD_NAME
                            Name = row[0]?.ToString() ?? "",

                            // Column 1: TYPE (DISCOVERY or PARADOX)
                            CardType = row[1]?.ToString() ?? "",

                            // Column 2: EFFECT_DESCRIPTION
                            SpecialEffect = row[2]?.ToString() ?? "",

                            // Column 3: EFFECT_TYPE
                            EffectType = row[3]?.ToString() ?? "",

                            Country = "",
                            Field = "",
                            School = "",
                            Tier = "",
                            ATK = 0,
                            DEF = 0,
                            CurrentATK = 0,
                            CurrentDEF = 0,
                            Location = "DECK",
                            IsFaceDown = false
                        };

                        cards.Add(card);
                    }
                }
            }

            return cards;
        }

        // Verify import was successful
        public async Task<string> GetImportSummaryAsync()
        {
            int totalCards = await _databaseService.GetCardCountAsync();
            var characterCards = await _databaseService.GetAllCharacterCardsAsync();
            var discoveryCards = await _databaseService.GetAllDiscoveryCardsAsync();
            var paradoxCards = await _databaseService.GetAllParadoxCardsAsync();

            return $"Import Summary:\n" +
                   $"Total Cards: {totalCards}\n" +
                   $"Character Cards: {characterCards.Count}\n" +
                   $"Discovery Cards: {discoveryCards.Count}\n" +
                   $"Paradox Cards: {paradoxCards.Count}";
        }

        // DNA: Determine when effect should trigger
        private string DetermineEffectTrigger(string cardName, string effect)
        {
            // Cards with graveyard effects
            if (cardName == "Buddha" || cardName == "Nelson Mandela" || cardName == "Dante Alighieri")
                return "GRAVEYARD";

            // Cards that activate on summon
            if (cardName == "Albert Einstein" || cardName == "Isaac Newton" || cardName == "Nikola Tesla" ||
                cardName == "Marie Curie" || cardName == "Muhammad" || cardName == "Jesus Christ" ||
                cardName == "Archimedes" || cardName == "Max Planck")
                return "ON_SUMMON";

            // Cards with continuous effects (while on field)
            if (cardName == "Charles Darwin" || cardName == "Mahatma Gandhi" ||
                cardName == "Friedrich Nietzsche" || cardName == "Søren Kierkegaard")
                return "CONTINUOUS";

            // Everything else is manual
            return "MANUAL";
        }
    }
}