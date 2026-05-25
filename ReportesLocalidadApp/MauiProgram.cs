using Microsoft.Extensions.Logging;
using ReportesLocalidadApp.Services;
using ReportesLocalidadApp.ViewModels;
using ReportesLocalidadApp.Views;

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
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddTransient<AuthViewModel>();
            builder.Services.AddTransient<LoginView>();
            builder.Services.AddTransient<InicioView>();
            builder.Services.AddTransient<AgregarReporteView>();
            builder.Services.AddTransient<MisReportesView>();
            builder.Services.AddTransient<PerfilView>();
            builder.Services.AddTransient<ReporteView>();
            builder.Services.AddTransient<ReportesAdminView>();
            builder.Services.AddTransient<ReporteAdminView>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
