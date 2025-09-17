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
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ShowError("Por favor ingrese usuario y contraseña");
                return;
            }

            try
            {
                ShowLoading(true);
                ClearError();

                System.Diagnostics.Debug.WriteLine($"[LOGIN] Intentando login con usuario: {Email}");

                // ✅ CORREGIDO: Tu ApiService.LoginAsync devuelve ApiResponse<Usuario> directamente
                var response = await _apiService.LoginAsync(Email.Trim(), Password.Trim());

                if (response.Success && response.Data != null)
                {
                    // ✅ CORREGIDO: response.Data ES el Usuario directamente, no hay .User
                    var usuario = response.Data;

                    System.Diagnostics.Debug.WriteLine($"[LOGIN] ✅ Login exitoso: {usuario.Username}");
                    System.Diagnostics.Debug.WriteLine($"[LOGIN] Usuario ID: {usuario.IdUsuario}");
                    System.Diagnostics.Debug.WriteLine($"[LOGIN] Rol: {usuario.NombreRol}");
                    System.Diagnostics.Debug.WriteLine($"[LOGIN] Rol ID: {usuario.IdRol}"); // Usar IdRol, no RolId

                    // Guardar datos del usuario
                    await SecureStorage.SetAsync("UserId", usuario.IdUsuario.ToString());
                    await SecureStorage.SetAsync("UserName", $"{usuario.Nombres} {usuario.Apellidos}");
                    await SecureStorage.SetAsync("UserRole", usuario.NombreRol);
                    await SecureStorage.SetAsync("UserRoleId", usuario.IdRol.ToString()); // ✅ CORREGIDO: IdRol

                    // ✅ NOTA: No hay token en tu respuesta actual, comentamos esta línea
                    // await SecureStorage.SetAsync("AuthToken", response.Data.Token);
                    App.CurrentUser = usuario;
                    // ✅ NAVEGACIÓN INTELIGENTE SEGÚN ROL
                    await NavigateBasedOnRole(usuario.NombreRol, usuario.IdRol);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[LOGIN] ❌ Login fallido: {response.Message}");
                    ShowError(response.Message ?? "Usuario o contraseña incorrectos");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN] ❌ Error de conexión: {ex.Message}");
                ShowError($"Error de conexión: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async Task NavigateBasedOnRole(string role, int roleId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN] 🎯 Navegando según rol: {role} (ID: {roleId})");

                switch (roleId)
                {
                    case 1: // Administrador
                        System.Diagnostics.Debug.WriteLine("[LOGIN] ✅ Rol Administrador - Navegando a AdminMenuPage");
                        await Shell.Current.GoToAsync("//AdminMenuPage");
                        break;

                    case 72: // Recepcionista - ¡NUEVO PARA PUNTO 3!
                        System.Diagnostics.Debug.WriteLine("[LOGIN] ✅ Rol Recepcionista - Navegando a RecepcionistaMenuPage");
                        await Shell.Current.GoToAsync("//RecepcionistaMenuPage");
                        break;

                    case 70: // Médico
                        System.Diagnostics.Debug.WriteLine("[LOGIN] ✅ Rol Médico - Navegando a MedicoMenuPage");
                        await Shell.Current.DisplayAlert("Info", "Menú de médico en desarrollo", "OK");
                        await Shell.Current.GoToAsync("//MedicoHistorialPage"); // Temporal
                        break;

                    case 71: // Paciente
                        System.Diagnostics.Debug.WriteLine("[LOGIN] ✅ Rol Paciente - Navegando a PacienteMenuPage");
                        await Shell.Current.DisplayAlert("Info", "Menú de paciente en desarrollo", "OK");
                        await Shell.Current.GoToAsync("//AdminMenuPage"); // Temporal
                        break;

                    default:
                        System.Diagnostics.Debug.WriteLine($"[LOGIN] ⚠️ Rol no reconocido: {role} (ID: {roleId})");
                        await Shell.Current.DisplayAlert("Aviso", $"Acceso como {role}. Redirigiendo a menú administrador.", "OK");
                        await Shell.Current.GoToAsync("//AdminMenuPage");
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN] ❌ Error en navegación: {ex.Message}");
                ShowError("Error al navegar después del login");
            }
        }
    }
}