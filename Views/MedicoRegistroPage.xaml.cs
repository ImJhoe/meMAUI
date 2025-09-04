using ClinicaApp.ViewModels;

namespace ClinicaApp.Views;

public partial class MedicoRegistroPage : ContentPage
{
    public MedicoRegistroPage(MedicoRegistroViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}