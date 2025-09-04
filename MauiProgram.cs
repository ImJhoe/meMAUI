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

        // Configurar HttpClient con tu URL
        builder.Services.AddHttpClient<ApiService>(client =>
        {
            client.BaseAddress = new Uri("http://192.168.1.8:8081/webservice-slim/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Registrar ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<MedicoRegistroViewModel>();
        builder.Services.AddTransient<CitaViewModel>();

        // Registrar Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MedicoRegistroPage>();
        builder.Services.AddTransient<CitaCreacionPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}