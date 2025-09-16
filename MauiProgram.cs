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
   // Cambiar líneas 15-16:
private static readonly string CABLE_IP = "192.168.93.154";
private static readonly string WIFI_IP = "192.168.93.154";

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

        // ✅ REGISTRAR ApiService PRIMERO
        builder.Services.AddSingleton<ApiService>(provider =>
        {
            var httpClient = new HttpClient();
            var baseUrl = $"http://{selectedIP}:8081/webservice-slim/";
            httpClient.BaseAddress = new Uri(baseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(60);
            httpClient.DefaultRequestHeaders.ConnectionClose = false;

            Console.WriteLine($"🌐 Usando {connectionType}: {baseUrl}");
            return new ApiService(httpClient);
        });

        // ✅ REGISTRAR ViewModels CON DEPENDENCIA DE ApiService
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<AdminMenuViewModel>();
        builder.Services.AddSingleton<MedicoRegistroViewModel>();
        builder.Services.AddSingleton<MedicoConsultaViewModel>();
        builder.Services.AddSingleton<RecepcionistaMenuViewModel>();
        builder.Services.AddSingleton<CitaViewModel>();
        builder.Services.AddSingleton<HorariosMedicosViewModel>();
        builder.Services.AddSingleton<CitasListaViewModel>();
        builder.Services.AddSingleton<PacienteRegistroViewModel>();

        // ✅ REGISTRAR Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<AdminMenuPage>();
        builder.Services.AddTransient<MedicoRegistroPage>();
        builder.Services.AddTransient<MedicoConsultaPage>();
        builder.Services.AddTransient<RecepcionistaMenuPage>();
        builder.Services.AddTransient<CitaCreacionPage>();
        builder.Services.AddTransient<PacienteRegistroPage>();
        builder.Services.AddTransient<CitasListaPage>();
        builder.Services.AddTransient<HorariosMedicosPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}