using ScientistCardGame.Services;

namespace ScientistCardGame.Views
{
    public partial class StatsPage : ContentPage
    {
        private StatsService _statsService;

        public StatsPage()
        {
            InitializeComponent();
            _statsService = new StatsService();
            LoadStatistics();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadStatistics();
        }

        private async void LoadStatistics()
        {
            var stats = _statsService.GetStats();

            // Display stats
            WinsLabel.Text = stats.TotalWins.ToString();
            LossesLabel.Text = stats.TotalLosses.ToString();
            TotalGamesLabel.Text = (stats.TotalWins + stats.TotalLosses).ToString();

            // Win rate
            double winRate = _statsService.GetWinRate();
            WinRateLabel.Text = $"{winRate:F1}%";
            WinRateProgress.Progress = winRate / 100.0;

            // Last played
            if (stats.LastPlayedDate != DateTime.MinValue)
            {
                var lastPlayed = stats.LastPlayedDate;
                var timeAgo = DateTime.Now - lastPlayed;

                if (timeAgo.TotalMinutes < 1)
                    LastPlayedLabel.Text = "Just now";
                else if (timeAgo.TotalHours < 1)
                    LastPlayedLabel.Text = $"{(int)timeAgo.TotalMinutes} minutes ago";
                else if (timeAgo.TotalDays < 1)
                    LastPlayedLabel.Text = $"{(int)timeAgo.TotalHours} hours ago";
                else if (timeAgo.TotalDays < 7)
                    LastPlayedLabel.Text = $"{(int)timeAgo.TotalDays} days ago";
                else
                    LastPlayedLabel.Text = lastPlayed.ToString("MMM dd, yyyy");
            }
            else
            {
                LastPlayedLabel.Text = "Never";
            }

            // Performance message
            ShowPerformanceMessage(stats, winRate);
        }

        private void ShowPerformanceMessage(GameStats stats, double winRate)
        {
            int totalGames = stats.TotalWins + stats.TotalLosses;

            if (totalGames == 0)
            {
                PerformanceFrame.IsVisible = true;
                PerformanceTitle.Text = "🎮 READY TO PLAY!";
                PerformanceMessage.Text = "Start your first duel and make history!";
                PerformanceFrame.BorderColor = Color.FromArgb("#3b82f6");
                PerformanceTitle.TextColor = Color.FromArgb("#3b82f6");
            }
            else if (winRate >= 80)
            {
                PerformanceFrame.IsVisible = true;
                PerformanceTitle.Text = "⭐ LEGENDARY MASTER!";
                PerformanceMessage.Text = "You're dominating the game! Incredible skill!";
                PerformanceFrame.BorderColor = Colors.Gold;
                PerformanceTitle.TextColor = Colors.Gold;
            }
            else if (winRate >= 60)
            {
                PerformanceFrame.IsVisible = true;
                PerformanceTitle.Text = "🔥 STRONG PLAYER!";
                PerformanceMessage.Text = "Great performance! Keep pushing forward!";
                PerformanceFrame.BorderColor = Color.FromArgb("#22c55e");
                PerformanceTitle.TextColor = Color.FromArgb("#22c55e");
            }
            else if (winRate >= 40)
            {
                PerformanceFrame.IsVisible = true;
                PerformanceTitle.Text = "💪 BALANCED FIGHTER!";
                PerformanceMessage.Text = "You're learning fast! Victory awaits!";
                PerformanceFrame.BorderColor = Color.FromArgb("#f59e0b");
                PerformanceTitle.TextColor = Color.FromArgb("#f59e0b");
            }
            else
            {
                PerformanceFrame.IsVisible = true;
                PerformanceTitle.Text = "📚 KEEP LEARNING!";
                PerformanceMessage.Text = "Every defeat is a lesson. Study your strategies!";
                PerformanceFrame.BorderColor = Color.FromArgb("#3b82f6");
                PerformanceTitle.TextColor = Color.FromArgb("#3b82f6");
            }
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadStatistics();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}