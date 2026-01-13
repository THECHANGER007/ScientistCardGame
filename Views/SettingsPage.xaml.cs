using ScientistCardGame.Services;

namespace ScientistCardGame.Views
{
    public partial class SettingsPage : ContentPage
    {
        private AudioService _audioService;

        public SettingsPage()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Load saved preferences
            MusicToggle.IsToggled = Preferences.Get("MusicEnabled", true);
            SoundToggle.IsToggled = Preferences.Get("SoundEnabled", true);
            MusicVolumeSlider.Value = Preferences.Get("MusicVolume", 30.0);
            SoundVolumeSlider.Value = Preferences.Get("SoundVolume", 50.0);
        }

        private void OnMusicToggled(object sender, ToggledEventArgs e)
        {
            Preferences.Set("MusicEnabled", e.Value);

            // Apply to AudioService if available
            if (_audioService != null)
            {
                _audioService.IsMusicEnabled = e.Value;
                if (e.Value)
                    _audioService.ToggleMusic();
                else
                    _audioService.ToggleMusic();
            }
        }

        private void OnSoundToggled(object sender, ToggledEventArgs e)
        {
            Preferences.Set("SoundEnabled", e.Value);

            // Apply to AudioService if available
            if (_audioService != null)
            {
                _audioService.IsSoundEnabled = e.Value;
            }
        }

        private void OnMusicVolumeChanged(object sender, ValueChangedEventArgs e)
        {
            int volumePercent = (int)e.NewValue;
            MusicVolumeLabel.Text = $"{volumePercent}%";
            Preferences.Set("MusicVolume", e.NewValue);

            // Apply to AudioService if available
            if (_audioService != null)
            {
                _audioService.SetMusicVolume((float)(e.NewValue / 100.0));
            }
        }

        private void OnSoundVolumeChanged(object sender, ValueChangedEventArgs e)
        {
            int volumePercent = (int)e.NewValue;
            SoundVolumeLabel.Text = $"{volumePercent}%";
            Preferences.Set("SoundVolume", e.NewValue);

            // Apply to AudioService if available
            if (_audioService != null)
            {
                _audioService.SetSoundVolume((float)(e.NewValue / 100.0));
            }
        }

        private async void OnResetStatsClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "⚠️ RESET ALL STATISTICS",
                "This will delete:\n• All win/loss records\n• All game statistics\n\nThis action CANNOT be undone!\n\nAre you sure?",
                "YES - DELETE ALL",
                "NO - Cancel"
            );

            if (!confirm) return;

            // Double confirmation
            bool doubleConfirm = await DisplayAlert(
                "⚠️ FINAL WARNING",
                "Are you ABSOLUTELY SURE?\n\nAll your statistics will be permanently deleted!",
                "YES - I'M SURE",
                "NO - Go Back"
            );

            if (!doubleConfirm) return;

            // Reset statistics
            var statsService = new StatsService();
            await statsService.ResetStats();

            await DisplayAlert("✅ Reset Complete", "All statistics have been reset!", "OK");
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}