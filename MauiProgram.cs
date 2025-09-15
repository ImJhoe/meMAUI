using ClinicaApp.Services;
using ClinicaApp.ViewModels;
using ClinicaApp.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace ClinicaApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // HttpClient
        builder.Services.AddHttpClient<ApiService>(client =>
        {
            client.BaseAddress = new Uri("http://192.168.1.8:8081/webservice-slim/");
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.ConnectionClose = false;
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
#if DEBUG
            handler.ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) => true;
#endif
            return handler;
        });

        // VIEWMODELS - Registrar como SINGLETON
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<AdminMenuViewModel>();
        builder.Services.AddSingleton<MedicoRegistroViewModel>();
        builder.Services.AddSingleton<MedicoConsultaViewModel>();
        builder.Services.AddSingleton<RecepcionistaMenuViewModel>();
        builder.Services.AddSingleton<CitaViewModel>();

        // ✅ REMOVIDO: PacienteRegistroViewModel (se crea manualmente en la página)
        // builder.Services.AddSingleton<PacienteRegistroViewModel>();

        // PAGES - Registrar como TRANSIENT
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<AdminMenuPage>();
        builder.Services.AddTransient<MedicoRegistroPage>();
        builder.Services.AddTransient<MedicoConsultaPage>();
        builder.Services.AddTransient<RecepcionistaMenuPage>();
        builder.Services.AddTransient<CitaCreacionPage>();

        // ✅ PÁGINA sin DI (se instancia con constructor sin parámetros)
        builder.Services.AddTransient<PacienteRegistroPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}