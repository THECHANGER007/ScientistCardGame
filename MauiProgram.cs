using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using ScientistCardGame.Services;

namespace ScientistCardGame
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            // DNA: Register Audio Service
            builder.Services.AddSingleton(AudioManager.Current);
            builder.Services.AddSingleton<AudioService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
