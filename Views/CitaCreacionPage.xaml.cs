using ClinicaApp.ViewModels;

namespace ClinicaApp.Views;

public partial class CitaCreacionPage : ContentPage
{
    public CitaCreacionPage(CitaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}