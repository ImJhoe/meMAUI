// Views/HorariosMedicosPage.xaml.cs
using ClinicaApp.ViewModels;

namespace ClinicaApp.Views;

public partial class HorariosMedicosPage : ContentPage
{
    public HorariosMedicosPage()
    {
        InitializeComponent();
        BindingContext = new HorariosMedicosViewModel();
    }
}