using ClinicaApp.ViewModels;

namespace ClinicaApp.Views;

public partial class PacienteRegistroPage : ContentPage
{
    // ✅ CONSTRUCTOR SIN PARÁMETROS para MAUI Shell
    public PacienteRegistroPage()
    {
        InitializeComponent();

        // Crear ViewModel manualmente
        var httpClient = new HttpClient();
        // Cambiar línea 14:
        httpClient.BaseAddress = new Uri("http://192.168.1.14:8081/webservice-slim/");
        var apiService = new ClinicaApp.Services.ApiService(httpClient);

        BindingContext = new PacienteRegistroViewModel(apiService);
    }

    // ✅ CONSTRUCTOR CON DI (opcional, para compatibilidad)
    public PacienteRegistroPage(PacienteRegistroViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}