// ViewModels/RecepcionistaMenuViewModel.cs
using ClinicaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClinicaApp.ViewModels
{
    public partial class RecepcionistaMenuViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string userName = string.Empty;

        public RecepcionistaMenuViewModel()
        {
            Title = "Panel Recepcionista";
            LoadUserInfo();

            System.Diagnostics.Debug.WriteLine("[RECEPCIONISTA MENU] ViewModel inicializado");
        }

        private async void LoadUserInfo()
        {
            try
            {
                var storedName = await SecureStorage.GetAsync("UserName");
                var storedRole = await SecureStorage.GetAsync("UserRole");
                UserName = $"{storedName} ({storedRole})";

                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA MENU] Usuario cargado: {UserName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA MENU] Error cargando info: {ex.Message}");
                UserName = "Usuario Recepcionista";
            }
        }

        // ✅ PUNTO 3: Comando principal para crear citas
        [RelayCommand]
        private async Task IrACrearCita()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[RECEPCIONISTA] 🎯 PUNTO 3: Navegando a crear cita");
                await Shell.Current.GoToAsync("//CitaCreacionPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] ❌ Error navegando a crear cita: {ex.Message}");
                ShowError("Error al navegar a la creación de citas");
            }
        }

        // Comando para ir al registro de pacientes (Punto 5)
        [RelayCommand]
        private async Task IrARegistroPaciente()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[RECEPCIONISTA] Navegando a registro de paciente");
                // TODO: Implementar cuando creemos PacienteRegistroPage
                await Shell.Current.DisplayAlert("Info", "Función de registro de paciente en desarrollo", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] Error navegando a registro paciente: {ex.Message}");
            }
        }

        // Comando para ver citas
        [RelayCommand]
        private async Task VerCitas()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[RECEPCIONISTA] Ver citas solicitado");
                await Shell.Current.DisplayAlert("Info", "Función de ver citas en desarrollo", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] Error: {ex.Message}");
            }
        }

        // Comando para ver horarios médicos
        [RelayCommand]
        private async Task VerHorariosMedicos()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[RECEPCIONISTA] Ver horarios médicos solicitado");
                await Shell.Current.DisplayAlert("Info", "Función de horarios médicos en desarrollo", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] Error: {ex.Message}");
            }
        }

        // Comando para cerrar sesión
        [RelayCommand]
        private async Task Logout()
        {
            try
            {
                var confirmar = await Shell.Current.DisplayAlert("Confirmar", "¿Desea cerrar sesión?", "Sí", "No");

                if (confirmar)
                {
                    System.Diagnostics.Debug.WriteLine("[RECEPCIONISTA] Cerrando sesión...");

                    // Limpiar datos almacenados
                    SecureStorage.RemoveAll();

                    // Navegar al login
                    await Shell.Current.GoToAsync("//LoginPage");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] Error en logout: {ex.Message}");
                ShowError("Error al cerrar sesión");
            }
        }
    }
}