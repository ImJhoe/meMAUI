using ClinicaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClinicaApp.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public LoginViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Iniciar Sesión";
        }

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ShowError("Por favor completa todos los campos");
                return;
            }

            try
            {
                ShowLoading(true);
                ClearError();

                System.Diagnostics.Debug.WriteLine($"[LOGIN VM] Iniciando login con: {Email}");

                // Test de conexión primero
                var (connectionOk, connectionMsg) = await _apiService.TestConnectionAsync();
                if (!connectionOk)
                {
                    ShowError($"Error de conexión: {connectionMsg}");
                    return;
                }

                // Solo una llamada al login
                var response = await _apiService.LoginAsync(Email.Trim(), Password.Trim());

                if (response.Success && response.Data != null)
                {
                    var usuario = response.Data;

                    System.Diagnostics.Debug.WriteLine($"[LOGIN VM] Login exitoso: {usuario.NombreCompleto}");

                    // Guardar sesión
                    await SecureStorage.SetAsync("UserId", usuario.IdUsuario.ToString());
                    await SecureStorage.SetAsync("UserName", usuario.NombreCompleto);
                    await SecureStorage.SetAsync("UserRole", usuario.NombreRol);

                    // Navegar según rol
                    await NavigateBasedOnRole(usuario.NombreRol);
                }
                else
                {
                    ShowError(response.Message ?? "Credenciales incorrectas");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN VM ERROR] {ex}");
                ShowError($"Error: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async Task NavigateBasedOnRole(string role)
        {
            string targetPage = role?.ToLower() switch
            {
                "administrador" => "//AdminMenuPage",
                "recepcionista" => "//RecepcionistaMenuPage",
                "medico" => "//MedicoMenuPage",
                "médico" => "//MedicoMenuPage", // Por si tiene acento
                "paciente" => "//PacienteMenuPage",
                "enfermero" => "//EnfermeroMenuPage", // ⭐ NUEVO
                "enfermera" => "//EnfermeroMenuPage", // ⭐ NUEVO
                _ => "//LoginPage"
            };

            if (targetPage != "//LoginPage")
            {
                await Shell.Current.GoToAsync(targetPage);
            }
            else
            {
                ShowError($"Rol de usuario no reconocido: {role}");
            }
        }
    }
}