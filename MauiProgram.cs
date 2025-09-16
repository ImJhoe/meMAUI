using ClinicaApp.Services;
using ClinicaApp.ViewModels;
using ClinicaApp.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace ClinicaApp;

public static class MauiProgram
{
    // 🎛️ CAMBIA AQUÍ SEGÚN TU CONEXIÓN
    private static readonly bool USE_CABLE = false; // true = cable, false = wifi

    // IPs conocidas
    private static readonly string CABLE_IP = "192.168.1.8";
    private static readonly string WIFI_IP = "192.168.1.14";

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

        // Seleccionar IP según configuración
        var selectedIP = USE_CABLE ? CABLE_IP : WIFI_IP;
        var connectionType = USE_CABLE ? "CABLE" : "WIFI";

        // HttpClient con IP seleccionada
        builder.Services.AddHttpClient<ApiService>(client =>
        {
            var baseUrl = $"http://{selectedIP}:8081/webservice-slim/";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.ConnectionClose = false;

            Console.WriteLine($"🌐 Usando {connectionType}: {baseUrl}");
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

        // Resto de configuraciones...
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<AdminMenuViewModel>();
        builder.Services.AddSingleton<MedicoRegistroViewModel>();
        builder.Services.AddSingleton<MedicoConsultaViewModel>();
        builder.Services.AddSingleton<RecepcionistaMenuViewModel>();
        builder.Services.AddSingleton<CitaViewModel>();
        builder.Services.AddSingleton<HorariosMedicosViewModel>();
        builder.Services.AddSingleton<CitasListaViewModel>();




        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<AdminMenuPage>();
        builder.Services.AddTransient<MedicoRegistroPage>();
        builder.Services.AddTransient<MedicoConsultaPage>();
        builder.Services.AddTransient<RecepcionistaMenuPage>();
        builder.Services.AddTransient<CitaCreacionPage>();
        builder.Services.AddTransient<PacienteRegistroPage>();
        builder.Services.AddTransient<CitasListaPage>();
        builder.Services.AddTransient<HorariosMedicosPage>();
        builder.Services.AddSingleton<ApiService>();


#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}