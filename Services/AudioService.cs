using Plugin.Maui.Audio;

namespace ScientistCardGame.Services
{
    public class AudioService
    {
        private readonly IAudioManager _audioManager;
        private IAudioPlayer _backgroundMusic;
        private Dictionary<string, IAudioPlayer> _soundEffects;

        public bool IsMusicEnabled { get; set; } = true;
        public bool IsSoundEnabled { get; set; } = true;
        public float MusicVolume { get; set; } = 0.3f;  // ← Lower default volume
        public float SoundVolume { get; set; } = 0.5f;

        public AudioService(IAudioManager audioManager)
        {
            _audioManager = audioManager;
            _soundEffects = new Dictionary<string, IAudioPlayer>();
        }

        // Play background music (looping)
        public async Task PlayBackgroundMusicAsync(string fileName)
        {
            try
            {
                if (!IsMusicEnabled) return;

                StopBackgroundMusic();

                // ← FIXED PATH: Just filename, not "Resources/Raw/"
                var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
                _backgroundMusic = _audioManager.CreatePlayer(stream);
                _backgroundMusic.Volume = MusicVolume;
                _backgroundMusic.Loop = true;
                _backgroundMusic.Play();
            }
            catch (Exception ex)
            {
                // Silently fail if audio not available
                System.Diagnostics.Debug.WriteLine($"Music not available: {ex.Message}");
            }
        }

        public void StopBackgroundMusic()
        {
            if (_backgroundMusic != null)
            {
                _backgroundMusic.Stop();
                _backgroundMusic.Dispose();
                _backgroundMusic = null;
            }
        }

        // Play sound effect (one-shot)
        public async Task PlaySoundEffectAsync(string effectName)
        {
            try
            {
                if (!IsSoundEnabled) return;

                string fileName = GetSoundFileName(effectName);

                var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
                var player = _audioManager.CreatePlayer(stream);
                player.Volume = SoundVolume;
                player.Play();

                // Clean up after 3 seconds (most sound effects are shorter)
                _ = Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    player?.Dispose();
                });
            }
            catch (Exception ex)
            {
                // Silently fail
            }
        }

        private string GetSoundFileName(string effectName)
        {
            return effectName switch
            {
                "summon" => "summon.mp3",
                "attack" => "attack.mp3",
                "destroy" => "destroy.mp3",
                "trap_set" => "trap_set.mp3",
                "discovery" => "discovery.mp3",
                "phase_change" => "phase_change.mp3",
                "victory" => "victory.mp3",
                "defeat" => "defeat.mp3",
                "draw" => "draw.mp3",
                "click" => "click.mp3",
                _ => "click.mp3"
            };
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = Math.Clamp(volume, 0f, 1f);
            if (_backgroundMusic != null)
            {
                _backgroundMusic.Volume = MusicVolume;
            }
        }

        public void SetSoundVolume(float volume)
        {
            SoundVolume = Math.Clamp(volume, 0f, 1f);
        }

        public void ToggleMusic()
        {
            IsMusicEnabled = !IsMusicEnabled;
            if (!IsMusicEnabled)
            {
                _backgroundMusic?.Pause();
            }
            else
            {
                _backgroundMusic?.Play();
            }
        }

        public void ToggleSound()
        {
            IsSoundEnabled = !IsSoundEnabled;
        }

        public void Dispose()
        {
            StopBackgroundMusic();
            foreach (var effect in _soundEffects.Values)
            {
                effect.Dispose();
            }
            _soundEffects.Clear();
        }
    }
}