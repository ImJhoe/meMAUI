// Views/TriajeRegistroPage.xaml.cs - PUNTO 1: Registro de Triaje
using ClinicaApp.ViewModels;

namespace ClinicaApp.Views;

public partial class TriajeRegistroPage : ContentPage
{
    // ✅ CONSTRUCTOR SIN PARÁMETROS para MAUI Shell
    public TriajeRegistroPage()
    {
        InitializeComponent();

        // Crear ViewModel manualmente
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://192.168.1.14:8081/webservice-slim/");
        var apiService = new ClinicaApp.Services.ApiService(httpClient);

        BindingContext = new TriajeRegistroViewModel(apiService);
    }

    // ✅ CONSTRUCTOR CON DI (opcional, para compatibilidad)
    public TriajeRegistroPage(TriajeRegistroViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}