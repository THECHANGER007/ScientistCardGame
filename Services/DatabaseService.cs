using SQLite;
using ScientistCardGame.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ScientistCardGame.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        // Constructor - connects to database file
        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);

            // Create tables if they don't exist
            _database.CreateTableAsync<Card>().Wait();
        }

        // DNA: Get all CHARACTER cards (100 scientists)
        public Task<List<Card>> GetAllCharacterCardsAsync()
        {
            return _database.Table<Card>()
                           .Where(c => c.CardType == "CHARACTER")
                           .ToListAsync();
        }

        // DNA: Get all DISCOVERY cards (10 support cards)
        public Task<List<Card>> GetAllDiscoveryCardsAsync()
        {
            return _database.Table<Card>()
                           .Where(c => c.CardType == "DISCOVERY")
                           .ToListAsync();
        }

        // DNA: Get all PARADOX cards (10 trap cards)
        public Task<List<Card>> GetAllParadoxCardsAsync()
        {
            return _database.Table<Card>()
                           .Where(c => c.CardType == "PARADOX")
                           .ToListAsync();
        }

        // Get cards by FIELD (from DNA: SCIENCE, PHILOSOPHY, SPIRITUALITY, HUMANITIES)
        public Task<List<Card>> GetCardsByFieldAsync(string field)
        {
            return _database.Table<Card>()
                           .Where(c => c.Field == field && c.CardType == "CHARACTER")
                           .ToListAsync();
        }

        // Get cards by SCHOOL (from DNA: RATIONALISM, EMPIRICISM, etc.)
        public Task<List<Card>> GetCardsBySchoolAsync(string school)
        {
            return _database.Table<Card>()
                           .Where(c => c.School == school && c.CardType == "CHARACTER")
                           .ToListAsync();
        }

        // Get cards by TIER (from DNA: LEGENDARY, MASTER, SCHOLAR)
        public Task<List<Card>> GetCardsByTierAsync(string tier)
        {
            return _database.Table<Card>()
                           .Where(c => c.Tier == tier && c.CardType == "CHARACTER")
                           .ToListAsync();
        }

        // Get specific card by name
        public Task<Card> GetCardByNameAsync(string name)
        {
            return _database.Table<Card>()
                           .Where(c => c.Name == name)
                           .FirstOrDefaultAsync();
        }

        // Add a card to database
        public Task<int> SaveCardAsync(Card card)
        {
            return _database.InsertAsync(card);
        }

        // Add multiple cards at once
        public Task<int> SaveCardsAsync(List<Card> cards)
        {
            return _database.InsertAllAsync(cards);
        }

        // Delete all cards (for fresh import)
        public Task<int> DeleteAllCardsAsync()
        {
            return _database.DeleteAllAsync<Card>();
        }

        // Count total cards in database
        public Task<int> GetCardCountAsync()
        {
            return _database.Table<Card>().CountAsync();
        }

        // Check if database is empty
        public async Task<bool> IsDatabaseEmptyAsync()
        {
            int count = await GetCardCountAsync();
            return count == 0;
        }
    }
}