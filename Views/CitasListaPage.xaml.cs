// Views/CitasListaPage.xaml.cs
using ClinicaApp.ViewModels;
using ClinicaApp.Services;
using ClinicaApp.Models;
namespace ClinicaApp.Views;
public partial class CitasListaPage : ContentPage
{
    public CitasListaPage(CitasListaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        BindingContext = viewModel;
    }

    // Constructor sin parámetros como fallback
    public CitasListaPage()
    {
        InitializeComponent();
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://192.168.1.14:8081/webservice-slim/");
        var apiService = new ApiService(httpClient);
        BindingContext = new CitasListaViewModel(apiService);
    }
}