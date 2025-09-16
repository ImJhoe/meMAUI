// Views/CitasListaPage.xaml.cs
using ClinicaApp.ViewModels;

namespace ClinicaApp.Views;
public partial class CitasListaPage : ContentPage
{
    public CitasListaPage(CitasListaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Constructor sin par�metros como fallback
    public CitasListaPage()
    {
        InitializeComponent();
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://192.168.1.14:8081/webservice-slim/");
        var apiService = new ApiService(httpClient);
        BindingContext = new CitasListaViewModel(apiService);
    }
}