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
    System.Diagnostics.Debug.WriteLine($"[LOGIN DEBUG] Método LoginAsync ejecutado");
    System.Diagnostics.Debug.WriteLine($"[LOGIN DEBUG] Email: '{Email}'");
    System.Diagnostics.Debug.WriteLine($"[LOGIN DEBUG] Password: '{Password}'");
    
    if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
    {
        System.Diagnostics.Debug.WriteLine("[LOGIN DEBUG] Campos vacíos");
        ShowError("Por favor completa todos los campos");
        return;
    }

            try
            {
                ShowLoading(true);
                ClearError();

                // Debug de red
                var connectivity = Connectivity.Current.NetworkAccess;
                System.Diagnostics.Debug.WriteLine($"[NETWORK] Estado de red: {connectivity}");

                if (connectivity != NetworkAccess.Internet)
                {
                    ShowError("No hay conexión a internet");
                    return;
                }

                // Test de conexión
                var (connectionOk, connectionMsg) = await _apiService.TestConnectionAsync();
                System.Diagnostics.Debug.WriteLine($"[CONNECTION] Resultado: {connectionOk} - {connectionMsg}");

                if (!connectionOk)
                {
                    ShowError($"Error de conexión: {connectionMsg}");
                    return;
                }

                // Hacer login
                var response = await _apiService.LoginAsync(Email.Trim(), Password.Trim());

                if (response.Success && response.Data != null)
                {
                    var usuario = response.Data;

                    // Debug detallado del usuario recibido
                    System.Diagnostics.Debug.WriteLine($"[LOGIN VM] Usuario recibido:");
                    System.Diagnostics.Debug.WriteLine($"  - ID: {usuario.IdUsuario}");
                    System.Diagnostics.Debug.WriteLine($"  - Username: {usuario.Username}");
                    System.Diagnostics.Debug.WriteLine($"  - Nombres: '{usuario.Nombres}'");
                    System.Diagnostics.Debug.WriteLine($"  - Apellidos: '{usuario.Apellidos}'");
                    System.Diagnostics.Debug.WriteLine($"  - NombreCompleto: '{usuario.NombreCompleto}'");
                    System.Diagnostics.Debug.WriteLine($"  - Rol: '{usuario.NombreRol}'");

                    // Verificar si el rol está vacío
                    if (string.IsNullOrEmpty(usuario.NombreRol))
                    {
                        ShowError("Error: Rol de usuario vacío");
                        return;
                    }

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
                    ShowError(response.Message ?? "Error de autenticación");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN ERROR] {ex}");
                ShowError($"Error inesperado: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        // En LoginViewModel.cs, reemplazar NavigateBasedOnRole:
        private async Task NavigateBasedOnRole(string role)
        {
            System.Diagnostics.Debug.WriteLine($"[NAVIGATION] Navegando para rol: '{role}'");

            string targetPage = role?.ToLower() switch
            {
                "administrador" => "//AdminMenuPage",
                "recepcionista" => "//RecepcionistaMenuPage",
                "medico" => "//MedicoMenuPage",
                "médico" => "//MedicoMenuPage",
                "paciente" => "//PacienteMenuPage",
                "enfermero" => "//EnfermeroMenuPage",
                "enfermera" => "//EnfermeroMenuPage",
                _ => null
            };

            System.Diagnostics.Debug.WriteLine($"[NAVIGATION] Página destino: {targetPage ?? "NINGUNA"}");

            if (!string.IsNullOrEmpty(targetPage))
            {
                try
                {
                    await Shell.Current.GoToAsync(targetPage);
                    System.Diagnostics.Debug.WriteLine($"[NAVIGATION] Navegación exitosa a {targetPage}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[NAVIGATION ERROR] {ex.Message}");
                    ShowError($"Error navegando a la pantalla principal. {ex.Message}");
                }
            }
            else
            {
                ShowError($"Rol de usuario no reconocido: '{role}'. Roles válidos: Administrador, Recepcionista, Medico, Paciente, Enfermero");
            }
        }
    }
}