using ClinicaApp.Models;

namespace ClinicaApp
{
    public partial class App : Application
    {
        // Propiedad estática para mantener el usuario logueado
        public static Usuario? CurrentUser { get; set; }

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}