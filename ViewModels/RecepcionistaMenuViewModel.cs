// ViewModels/RecepcionistaMenuViewModel.cs - VERSIÓN CORREGIDA
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

                if (!string.IsNullOrEmpty(storedName))
                {
                    UserName = $"{storedName} ({storedRole ?? "Recepcionista"})";
                }
                else
                {
                    UserName = "Usuario Recepcionista";
                }

                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA MENU] Usuario cargado: {UserName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA MENU] Error cargando info: {ex.Message}");
                UserName = "Usuario Recepcionista";
            }
        }

        // ==================== PUNTO 1: TRIAJE ====================

        [RelayCommand]
        private async Task IrATriaje()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[RECEPCIONISTA] ✅ PUNTO 1: Navegando a Triaje");
                // ✅ CAMBIO: De "TriajeRegistroPage" a "///TriajeRegistroPage"
                await Shell.Current.GoToAsync("///TriajeRegistroPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] ❌ Error navegación triaje: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    "Error al navegar al registro de triaje", "OK");
            }
        }

        // ==================== COMANDOS PRINCIPALES ====================

        // ✅ BOTÓN 1: Buscar Paciente - Va a creación de citas (PUNTO 3 y 4)
        [RelayCommand]
        private async Task IrACrearCita()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[RECEPCIONISTA] 🔍 Navegando a buscar paciente/crear cita (Puntos 3-4)");
                // ✅ CAMBIO: De "CitaCreacionPage" a "///CitaCreacionPage"
                await Shell.Current.GoToAsync("///CitaCreacionPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] ❌ Error navegando a crear cita: {ex.Message}");
                ShowError("Error al navegar a la búsqueda de pacientes");
            }
        }

        // ✅ BOTÓN 2: Ver Citas - Nueva página para listar citas
        [RelayCommand]
        private async Task VerCitas()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[RECEPCIONISTA] 📋 Navegando a ver citas");
                // ✅ CAMBIO: De "CitasListaPage" a "///CitasListaPage"
                await Shell.Current.GoToAsync("///CitasListaPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] ❌ Error navegando a ver citas: {ex.Message}");

                // Si la página no existe, mostrar mensaje informativo
                await Application.Current?.MainPage?.DisplayAlert("Info",
                    "Página de lista de citas en desarrollo", "OK");
            }
        }

        // ✅ BOTÓN 3: Registrar Paciente - Va directo a registro (PUNTO 5)
        [RelayCommand]
        private async Task IrARegistroPaciente()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[RECEPCIONISTA] 👤 Navegando a registro de paciente (Punto 5)");
                // ✅ CAMBIO: De "PacienteRegistroPage" a "///PacienteRegistroPage"
                await Shell.Current.GoToAsync("///PacienteRegistroPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] ❌ Error navegando a registro paciente: {ex.Message}");
                ShowError($"Error al navegar al registro de pacientes: {ex.Message}");
            }
        }

        // ✅ BOTÓN 4: Horarios Médicos - Nueva página para calendario (PUNTO 7)
        [RelayCommand]
        private async Task VerHorariosMedicos()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[RECEPCIONISTA] 🗓️ Navegando a horarios médicos (Punto 7)");
                // ✅ CAMBIO: De "HorariosMedicosPage" a "///HorariosMedicosPage"
                await Shell.Current.GoToAsync("///HorariosMedicosPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] ❌ Error navegando a horarios: {ex.Message}");

                // Si la página no existe, mostrar mensaje informativo
                await Application.Current?.MainPage?.DisplayAlert("Info",
                    "Página de horarios médicos en desarrollo", "OK");
            }
        }

        // ==================== MÉTODO TEMPORAL PARA MOSTRAR CITAS ====================

        private async Task MostrarCitasTemporalAsync()
        {
            try
            {
                await Application.Current?.MainPage?.DisplayAlert("📋 Lista de Citas",
                    "Funcionalidad en desarrollo.\n\n" +
                    "Próximamente podrá:\n" +
                    "• Ver todas las citas del día\n" +
                    "• Filtrar por médico\n" +
                    "• Editar citas existentes\n" +
                    "• Cancelar citas",
                    "Entendido");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] Error mostrar citas temporal: {ex.Message}");
            }
        }

        // ==================== COMANDO DE LOGOUT ====================

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