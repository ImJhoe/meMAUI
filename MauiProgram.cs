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

        // ✅ REGISTRAR VIEWMODELS COMO SINGLETON (para XAML binding)
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<AdminMenuViewModel>();
        builder.Services.AddSingleton<MedicoRegistroViewModel>();


        // ✅ REGISTRAR PAGES COMO TRANSIENT
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<AdminMenuPage>();
        builder.Services.AddTransient<MedicoRegistroPage>();
        builder.Services.AddTransient<CitaCreacionPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}