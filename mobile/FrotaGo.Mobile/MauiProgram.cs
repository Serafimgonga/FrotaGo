using Microsoft.Extensions.Logging;
using FrotaGo.Mobile.Services;
using FrotaGo.Mobile.Views;

namespace FrotaGo.Mobile
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

            // Registar serviços no DI container
            builder.Services.AddSingleton<AuthService>(_ => new AuthService(MobileConfig.BaseUrl));
            builder.Services.AddSingleton<ApiService>(_ => new ApiService(MobileConfig.BaseUrl));
            builder.Services.AddTransient<TrackingService>();

            // Registar Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<LessonListPage>();
            builder.Services.AddTransient<LessonDetailPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
