using ScientistCardGame.Services;
using Plugin.Maui.Audio;

namespace ScientistCardGame.Views
{
    public partial class MainMenuPage : ContentPage
    {
        private AudioService _audioService;

        public MainMenuPage()
        {
            InitializeComponent();

            // Initialize audio
            _audioService = new AudioService(AudioManager.Current);
            _ = _audioService.PlayBackgroundMusicAsync("background_music.mp3");
        }

        private async void OnDuelClicked(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffectAsync("click");
            await Navigation.PushAsync(new GamePage());
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffectAsync("click");
            await Navigation.PushAsync(new SettingsPage());
        }

        private async void OnStatisticsClicked(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffectAsync("click");
            await Navigation.PushAsync(new StatsPage());
        }

        private async void OnCharactersClicked(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffectAsync("click");
            await Navigation.PushAsync(new CharactersLibraryPage());
        }

        private async void OnHowToPlayClicked(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffectAsync("click");
            await Navigation.PushAsync(new HowToPlayPage());
        }

        private async void OnExitClicked(object sender, EventArgs e)
        {
            await _audioService.PlaySoundEffectAsync("click");

            bool confirm = await DisplayAlert("Exit Game", "Are you sure you want to exit?", "Yes", "No");
            if (confirm)
            {
                Application.Current.Quit();
            }
        }
    }
}