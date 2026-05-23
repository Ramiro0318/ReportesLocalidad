using Microsoft.Extensions.Logging;
using ReportesLocalidadApp.Services;
using ReportesLocalidadApp.ViewModels;

namespace ReportesLocalidadApp
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

            builder.Services.AddSingleton(new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5027/")
            });
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddTransient<ReportesService>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegistroViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
