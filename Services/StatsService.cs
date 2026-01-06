using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScientistCardGame.Services
{
    public class StatsService
    {
        private string _statsFilePath;
        private GameStats _stats;

        public StatsService()
        {
            _statsFilePath = Path.Combine(FileSystem.AppDataDirectory, "game_stats.json");
            _stats = new GameStats();
            LoadStatsAsync();
        }

        // Load stats from file
        private async Task LoadStatsAsync()
        {
            try
            {
                if (File.Exists(_statsFilePath))
                {
                    string json = await File.ReadAllTextAsync(_statsFilePath);
                    _stats = JsonSerializer.Deserialize<GameStats>(json) ?? new GameStats();
                }
            }
            catch
            {
                _stats = new GameStats();
            }
        }

        // Save stats to file
        private async Task SaveStatsAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(_stats);
                await File.WriteAllTextAsync(_statsFilePath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }

        // Record a win
        public async Task RecordWin()
        {
            _stats.TotalWins++;
            _stats.TotalGames++;
            _stats.LastPlayedDate = DateTime.Now;
            await SaveStatsAsync();
        }

        // Record a loss
        public async Task RecordLoss()
        {
            _stats.TotalLosses++;
            _stats.TotalGames++;
            _stats.LastPlayedDate = DateTime.Now;
            await SaveStatsAsync();
        }

        // Get current stats
        public GameStats GetStats()
        {
            return _stats;
        }

        // Get win rate percentage
        public double GetWinRate()
        {
            if (_stats.TotalGames == 0)
                return 0;
            return Math.Round((_stats.TotalWins * 100.0) / _stats.TotalGames, 1);
        }

        // Get formatted stats string
        public string GetStatsDisplay()
        {
            return $"🏆 STATS:\n" +
                   $"Wins: {_stats.TotalWins}\n" +
                   $"Losses: {_stats.TotalLosses}\n" +
                   $"Total Games: {_stats.TotalGames}\n" +
                   $"Win Rate: {GetWinRate()}%\n" +
                   $"Last Played: {_stats.LastPlayedDate:MM/dd/yyyy}";
        }

        // Reset stats
        public async Task ResetStats()
        {
            _stats = new GameStats();
            await SaveStatsAsync();
        }
    }

    // Stats data model
    public class GameStats
    {
        public int TotalWins { get; set; }
        public int TotalLosses { get; set; }
        public int TotalGames { get; set; }
        public DateTime LastPlayedDate { get; set; }

        public GameStats()
        {
            TotalWins = 0;
            TotalLosses = 0;
            TotalGames = 0;
            LastPlayedDate = DateTime.Now;
        }
    }
}