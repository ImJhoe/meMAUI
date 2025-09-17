// Views/ConsultaMedicaPage.xaml.cs - PUNTOS 3,4,5: Consulta Médica
using ClinicaApp.ViewModels;

namespace ClinicaApp.Views;

public partial class ConsultaMedicaPage : ContentPage
{
    // ✅ CONSTRUCTOR SIN PARÁMETROS para MAUI Shell
    public ConsultaMedicaPage()
    {
        InitializeComponent();

        // Crear ViewModel manualmente
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://192.168.93.154:8081/webservice-slim/");
        var apiService = new ClinicaApp.Services.ApiService(httpClient);

        BindingContext = new ConsultaMedicaViewModel(apiService);
    }

    // ✅ CONSTRUCTOR CON DI (opcional, para compatibilidad)
    public ConsultaMedicaPage(ConsultaMedicaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}