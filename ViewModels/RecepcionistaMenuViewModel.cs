// ViewModels/RecepcionistaMenuViewModel.cs - VERSIÓN COMPLETA
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

        // ✅ BOTÓN 1: Buscar Paciente - Va a creación de citas (PUNTO 3 y 4)
        [RelayCommand]
        private async Task IrACrearCita()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[RECEPCIONISTA] 🔍 Navegando a buscar paciente/crear cita (Puntos 3-4)");
                await Shell.Current.GoToAsync("//CitaCreacionPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] ❌ Error navegando a crear cita: {ex.Message}");
                ShowError("Error al navegar a la búsqueda de pacientes");
            }
        }

        // ✅ BOTÓN 2: Registrar Paciente - Va directo a registro (PUNTO 5)
        [RelayCommand]
        private async Task IrARegistroPaciente()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[RECEPCIONISTA] 👤 Navegando a registro de paciente (Punto 5)");

                // Verificar si la página existe en el Shell
                await Shell.Current.GoToAsync("//PacienteRegistroPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] ❌ Error navegando a registro paciente: {ex.Message}");
                ShowError($"Error al navegar al registro de pacientes: {ex.Message}");
            }
        }

        // ✅ BOTÓN 3: Ver Citas - Nueva página para listar citas
        [RelayCommand]
        private async Task VerCitas()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[RECEPCIONISTA] 📋 Navegando a ver citas");

                // Por ahora navegamos a una página que crearemos
                await Shell.Current.GoToAsync("//CitasListaPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] ❌ Error navegando a ver citas: {ex.Message}");

                // Si la página no existe, mostrar lista inline
                await MostrarCitasTemporalAsync();
            }
        }

        // ✅ BOTÓN 4: Horarios Médicos - Nueva página para calendario (PUNTO 7)
        [RelayCommand]
        private async Task VerHorariosMedicos()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[RECEPCIONISTA] 🗓️ Navegando a horarios médicos (Punto 7)");

                // Navegamos a una página de calendario/horarios
                await Shell.Current.GoToAsync("//HorariosMedicosPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] ❌ Error navegando a horarios: {ex.Message}");

                // Si la página no existe, mostrar horarios inline
                await MostrarHorariosTemporalAsync();
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

        // MÉTODOS TEMPORALES MIENTRAS CREAMOS LAS PÁGINAS ESPECÍFICAS
        private async Task MostrarCitasTemporalAsync()
        {
            try
            {
                // Aquí podrías llamar al API para obtener citas
                var mensaje = "📋 CITAS PROGRAMADAS\n\n" +
                             "• Hoy: 5 citas pendientes\n" +
                             "• Mañana: 8 citas programadas\n" +
                             "• Esta semana: 35 citas totales\n\n" +
                             "💡 Función completa en desarrollo";

                await Shell.Current.DisplayAlert("Ver Citas", mensaje, "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] Error mostrando citas: {ex.Message}");
            }
        }

        private async Task MostrarHorariosTemporalAsync()
        {
            try
            {
                var mensaje = "🗓️ HORARIOS MÉDICOS\n\n" +
                             "• Lunes a Viernes: 08:00 - 18:00\n" +
                             "• Sábados: 09:00 - 14:00\n" +
                             "• Domingos: Cerrado\n\n" +
                             "📅 Calendario interactivo en desarrollo";

                await Shell.Current.DisplayAlert("Horarios Médicos", mensaje, "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RECEPCIONISTA] Error mostrando horarios: {ex.Message}");
            }
        }
    }
}