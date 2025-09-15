// ViewModels/PacienteRegistroViewModel.cs - PUNTOS 5 y 6
using ClinicaApp.Models;
using ClinicaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ClinicaApp.ViewModels
{
    [QueryProperty(nameof(CedulaBuscada), "cedula")]
    public partial class PacienteRegistroViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public PacienteRegistroViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "PUNTO 5: Registro de Paciente";

            System.Diagnostics.Debug.WriteLine("[PACIENTE REG] 🎯 PUNTO 5: ViewModel inicializado");

            InicializarDatos();
        }

        // PROPIEDADES OBSERVABLES
        [ObservableProperty]
        private string cedulaBuscada = string.Empty;

        [ObservableProperty]
        private string cedula = string.Empty;

        [ObservableProperty]
        private string nombres = string.Empty;

        [ObservableProperty]
        private string apellidos = string.Empty;

        [ObservableProperty]
        private string correo = string.Empty;

        [ObservableProperty]
        private DateTime fechaNacimiento = DateTime.Today.AddYears(-25);

        [ObservableProperty]
        private string sexoSeleccionado = "M";

        [ObservableProperty]
        private string tipoSangreSeleccionado = string.Empty;

        [ObservableProperty]
        private string telefono = string.Empty;

        [ObservableProperty]
        private string alergias = string.Empty;

        [ObservableProperty]
        private string contactoEmergencia = string.Empty;

        [ObservableProperty]
        private string telefonoEmergencia = string.Empty;

        // PROPIEDADES CALCULADAS
        public DateTime FechaMaxima => DateTime.Today;
        public ObservableCollection<string> OpcionesSexo { get; private set; }
        public ObservableCollection<string> TiposSangre { get; private set; }

        private void InicializarDatos()
        {
            OpcionesSexo = new ObservableCollection<string> { "M", "F" };
            TiposSangre = new ObservableCollection<string>
            {
                "", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"
            };
        }

        // PUNTO 6: Este método se ejecuta cuando se navega a esta página con parámetros
        partial void OnCedulaBuscadaChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                System.Diagnostics.Debug.WriteLine($"[PACIENTE REG] 📄 PUNTO 5: Recibida cédula: {value}");
                Cedula = value;
            }
        }

        // COMANDO PRINCIPAL - PUNTO 5: Registrar paciente
        [RelayCommand]
        private async Task RegistrarPacienteAsync()
        {
            if (!ValidarFormulario())
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine("[PACIENTE REG] 💾 PUNTO 5: Registrando paciente...");

                ShowLoading(true);
                ClearError();

                var paciente = new Paciente
                {
                    Cedula = Cedula.Trim(),
                    Nombres = Nombres.Trim(),
                    Apellidos = Apellidos.Trim(),
                    Correo = Correo.Trim(),
                    FechaNacimiento = FechaNacimiento,
                    Sexo = SexoSeleccionado,
                    TipoSangre = TipoSangreSeleccionado,
                    Telefono = Telefono.Trim(),
                    Alergias = Alergias.Trim(),
                    ContactoEmergencia = ContactoEmergencia.Trim(),
                    TelefonoEmergencia = TelefonoEmergencia.Trim()
                };

                System.Diagnostics.Debug.WriteLine($"[PACIENTE REG] 📊 Datos del paciente:");
                System.Diagnostics.Debug.WriteLine($"  - Cédula: {paciente.Cedula}");
                System.Diagnostics.Debug.WriteLine($"  - Nombre: {paciente.NombreCompleto}");
                System.Diagnostics.Debug.WriteLine($"  - Correo: {paciente.Correo}");

                var response = await _apiService.CrearPacienteAsync(paciente);

                if (response.Success)
                {
                    System.Diagnostics.Debug.WriteLine("[PACIENTE REG] ✅ PUNTO 5 COMPLETADO: Paciente registrado exitosamente");

                    await Shell.Current.DisplayAlert("🎉 Éxito",
                        "Paciente registrado correctamente", "OK");

                    // ✅ PUNTO 6: Regresar automáticamente al formulario de cita
                    System.Diagnostics.Debug.WriteLine("[PACIENTE REG] 🔄 PUNTO 6: Regresando automáticamente al formulario de cita");

                    await RegresarAFormularioCitaAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[PACIENTE REG] ❌ Error API: {response.Message}");
                    ShowError(response.Message ?? "Error al registrar el paciente");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PACIENTE REG] ❌ Excepción: {ex.Message}");
                ShowError($"Error: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        // ✅ PUNTO 6: Método para regresar automáticamente al formulario de cita
        private async Task RegresarAFormularioCitaAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[PACIENTE REG] ➡️ PUNTO 6: Navegando de vuelta a CitaCreacionPage");

                // Navegar de vuelta a la página de citas con la cédula como parámetro
                var parametros = new Dictionary<string, object>
                {
                    ["cedulaRegistrada"] = Cedula
                };

                // ✅ CORREGIDO: Usar ruta absoluta con ///
                await Shell.Current.GoToAsync("///CitaCreacionPage", parametros);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PACIENTE REG] ❌ Error navegación vuelta: {ex.Message}");

                // Si falla la navegación automática, mostrar opción manual
                var irACitas = await Shell.Current.DisplayAlert("Navegación",
                    "¿Desea ir al formulario de citas ahora?", "Sí", "No");

                if (irACitas)
                {
                    await Shell.Current.GoToAsync("///CitaCreacionPage");
                }
            }
        }

        // También actualiza el método CancelarAsync:
        [RelayCommand]
        private async Task CancelarAsync()
        {
            try
            {
                var confirmar = await Shell.Current.DisplayAlert("Confirmar",
                    "¿Desea cancelar el registro del paciente?", "Sí", "No");

                if (confirmar)
                {
                    System.Diagnostics.Debug.WriteLine("[PACIENTE REG] ❌ Registro cancelado por usuario");
                    // ✅ CORREGIDO: Usar ruta absoluta
                    await Shell.Current.GoToAsync("///CitaCreacionPage");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PACIENTE REG] Error en cancelar: {ex.Message}");
            }
        }

        // Comando para limpiar formulario
        [RelayCommand]
        private void LimpiarFormulario()
        {
            System.Diagnostics.Debug.WriteLine("[PACIENTE REG] 🧹 Limpiando formulario...");

            // No limpiar la cédula porque viene del flujo
            Nombres = string.Empty;
            Apellidos = string.Empty;
            Correo = string.Empty;
            FechaNacimiento = DateTime.Today.AddYears(-25);
            SexoSeleccionado = "M";
            TipoSangreSeleccionado = string.Empty;
            Telefono = string.Empty;
            Alergias = string.Empty;
            ContactoEmergencia = string.Empty;
            TelefonoEmergencia = string.Empty;

            ClearError();
        }

        // Validación del formulario
        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(Cedula))
            {
                ShowError("La cédula es requerida");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Nombres))
            {
                ShowError("Los nombres son requeridos");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Apellidos))
            {
                ShowError("Los apellidos son requeridos");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Correo))
            {
                ShowError("El correo electrónico es requerido");
                return false;
            }

            // Validación básica de correo
            if (!Correo.Contains("@") || !Correo.Contains("."))
            {
                ShowError("Ingrese un correo electrónico válido");
                return false;
            }

            // Validación de fecha de nacimiento
            if (FechaNacimiento > DateTime.Today)
            {
                ShowError("La fecha de nacimiento no puede ser futura");
                return false;
            }

            if (FechaNacimiento < DateTime.Today.AddYears(-120))
            {
                ShowError("La fecha de nacimiento no es válida");
                return false;
            }

            return true;
        }
    }
}