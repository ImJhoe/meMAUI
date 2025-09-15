using ClinicaApp.ViewModels;

namespace ClinicaApp.Views;

public partial class RecepcionistaMenuPage : ContentPage
{
    public RecepcionistaMenuPage(RecepcionistaMenuViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}