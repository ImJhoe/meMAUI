using ClinicaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClinicaApp.ViewModels
{
    public partial class AdminMenuViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string userName = string.Empty;

        public AdminMenuViewModel()
        {
            Title = "Panel Administrador";
            LoadUserInfo();
        }

        private async void LoadUserInfo()
        {
            try
            {
                var storedName = await SecureStorage.GetAsync("UserName");
                var storedRole = await SecureStorage.GetAsync("UserRole");
                UserName = $"{storedName} ({storedRole})";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ADMIN MENU] Error cargando info: {ex.Message}");
                UserName = "Usuario Administrador";
            }
        }

        [RelayCommand]
        private async Task IrARegistroMedico()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[ADMIN] Navegando a registro de médico");
                await Shell.Current.GoToAsync("//MedicoRegistroPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ADMIN] Error navegando: {ex.Message}");
                ShowError("Error al navegar al registro de médicos");
            }
        }

        [RelayCommand]
        private async Task IrAConsultaMedico()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[ADMIN] Navegando a consulta de médicos");
                await Shell.Current.GoToAsync("//MedicoConsultaPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ADMIN] Error navegando: {ex.Message}");
                ShowError("Error al navegar a la consulta de médicos");
            }
        }

        [RelayCommand]
        private async Task IrACreacionCita()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[ADMIN] Navegando a creación de citas");
                await Shell.Current.GoToAsync("//CitaCreacionPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ADMIN] Error navegando: {ex.Message}");
                ShowError("Error al navegar a la creación de citas");
            }
        }

        [RelayCommand]
        private async Task VerTodasFunciones()
        {
            ShowError("Función en desarrollo: Panel completo de administración");
        }

        [RelayCommand]
        private async Task Logout()
        {
            try
            {
                // Limpiar datos de sesión
                SecureStorage.RemoveAll();

                // Volver al login
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOGOUT] Error: {ex.Message}");
                ShowError("Error al cerrar sesión");
            }
        }
    }
}