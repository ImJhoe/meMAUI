// Views/MedicoHistorialPage.xaml.cs - PUNTO 2: Historial de Triaje
using ClinicaApp.ViewModels;

namespace ClinicaApp.Views;

public partial class MedicoHistorialPage : ContentPage
{
    // ✅ CONSTRUCTOR SIN PARÁMETROS para MAUI Shell
    public MedicoHistorialPage()
    {
        InitializeComponent();

        // Crear ViewModel manualmente
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://192.168.93.154:8081/webservice-slim/");
        var apiService = new ClinicaApp.Services.ApiService(httpClient);

        BindingContext = new MedicoHistorialViewModel(apiService);
    }

    // ✅ CONSTRUCTOR CON DI (opcional, para compatibilidad)
    public MedicoHistorialPage(MedicoHistorialViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}