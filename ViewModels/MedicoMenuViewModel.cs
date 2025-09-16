// ViewModels/MedicoMenuViewModel.cs - MENÚ DEL MÉDICO
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClinicaApp.ViewModels
{
    public partial class MedicoMenuViewModel : BaseViewModel
    {
        public MedicoMenuViewModel()
        {
            Title = "Menú Médico";
            System.Diagnostics.Debug.WriteLine("[MEDICO MENU] ✅ ViewModel inicializado");
        }

        // ==================== COMANDOS DE NAVEGACIÓN ====================

        [RelayCommand]
        private async Task IrAHistorialTriaje()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MEDICO MENU] ✅ PUNTO 2: Navegando a Historial de Triaje");
                await Shell.Current.GoToAsync("MedicoHistorialPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO MENU] ❌ Error navegación historial: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    "Error al navegar al historial de triaje", "OK");
            }
        }

        [RelayCommand]
        private async Task IrAConsultaMedica()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MEDICO MENU] ✅ PUNTOS 3,4,5: Navegando a Consulta Médica");
                await Shell.Current.GoToAsync("ConsultaMedicaPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO MENU] ❌ Error navegación consulta: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    "Error al navegar a consulta médica", "OK");
            }
        }

        [RelayCommand]
        private async Task Actualizar()
        {
            try
            {
                IsBusy = true;

                // Simular actualización de datos
                await Task.Delay(1000);

                await Application.Current?.MainPage?.DisplayAlert("✅ Actualizado",
                    "Datos actualizados correctamente", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO MENU] ❌ Error actualizar: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    "Error al actualizar datos", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CerrarSesion()
        {
            try
            {
                var confirmar = await Application.Current?.MainPage?.DisplayAlert("🚪 Cerrar Sesión",
                    "¿Está seguro que desea cerrar la sesión?", "Sí", "No");

                if (confirmar == true)
                {
                    // Limpiar datos de usuario
                    App.CurrentUser = null;

                    // Navegar al login
                    await Shell.Current.GoToAsync("//LoginPage");

                    System.Diagnostics.Debug.WriteLine("[MEDICO MENU] ✅ Sesión cerrada");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO MENU] ❌ Error cerrar sesión: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    "Error al cerrar sesión", "OK");
            }
        }
    }
}