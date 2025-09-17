// ViewModels/MedicoHistorialViewModel.cs - PUNTO 2: Vista de Historial/Triaje del Médico - CORREGIDO
using ClinicaApp.Models;
using ClinicaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ClinicaApp.ViewModels
{
    public partial class MedicoHistorialViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public MedicoHistorialViewModel()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://192.168.93.154:8081/webservice-slim/");
            _apiService = new ApiService(httpClient);
            InitializeViewModel();
        }

        public MedicoHistorialViewModel(ApiService apiService)
        {
            _apiService = apiService;
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            Title = "PUNTO 2: Historial de Triaje";

            // Inicializar colecciones
            CitasConTriaje = new ObservableCollection<CitaConTriaje>();

            // Cargar citas con triaje registrado
            _ = CargarCitasConTriajeAsync();

            System.Diagnostics.Debug.WriteLine("[HISTORIAL] ✅ PUNTO 2: ViewModel inicializado");
        }

        // ==================== PROPIEDADES OBSERVABLES ====================

        // SELECCIÓN DE CITA
        public ObservableCollection<CitaConTriaje> CitasConTriaje { get; private set; }

        [ObservableProperty]
        private CitaConTriaje? citaSeleccionada;

        [ObservableProperty]
        private bool mostrarInfoPaciente;

        [ObservableProperty]
        private string infoPaciente = string.Empty;

        // DATOS DEL TRIAJE
        [ObservableProperty]
        private bool mostrarTriaje;

        [ObservableProperty]
        private TriajeCompleto? triajeSeleccionado; // ✅ Usar el modelo de ClinicaApp.Models

        [ObservableProperty]
        private DateTime fechaTriaje;

        [ObservableProperty]
        private string nivelUrgenciaTexto = string.Empty;

        [ObservableProperty]
        private bool tieneObservaciones;

        // ==================== CAMBIO DE PROPIEDADES ====================

        partial void OnCitaSeleccionadaChanged(CitaConTriaje? value)
        {
            if (value != null)
            {
                MostrarInfoPaciente = true;
                InfoPaciente = $"👤 {value.NombrePaciente} | 📅 {value.FechaHora:dd/MM/yyyy HH:mm} | 🏥 {value.NombreSucursal}";

                // Cargar datos del triaje
                _ = CargarDatosTriajeAsync(value.IdCita);
            }
            else
            {
                MostrarInfoPaciente = false;
                MostrarTriaje = false;
                InfoPaciente = string.Empty;
                TriajeSeleccionado = null;
            }
        }

        partial void OnTriajeSeleccionadoChanged(TriajeCompleto? value) // ✅ Usar el modelo correcto
        {
            if (value != null)
            {
                MostrarTriaje = true;
                FechaTriaje = value.FechaRegistro;
                NivelUrgenciaTexto = ObtenerTextoUrgencia(value.NivelUrgencia);
                TieneObservaciones = !string.IsNullOrEmpty(value.Observaciones);

                System.Diagnostics.Debug.WriteLine($"[HISTORIAL] ✅ Triaje mostrado - Urgencia: {value.NivelUrgencia}");
            }
            else
            {
                MostrarTriaje = false;
            }
        }

        // ==================== MÉTODOS PRIVADOS ====================

        private async Task CargarCitasConTriajeAsync()
        {
            try
            {
                IsBusy = true;

                // ✅ USAR HttpClient directo para debug
                using var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("http://192.168.93.154:8081/webservice-slim/");

                var response = await httpClient.GetStringAsync("api/citas/con-triaje"); // ✅ CORRECTO: endpoint para lista de citas
                System.Diagnostics.Debug.WriteLine($"[HISTORIAL DEBUG] Respuesta completa: {response}");

                var jsonDoc = System.Text.Json.JsonDocument.Parse(response);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("success", out var successProp) && successProp.GetBoolean())
                {
                    if (root.TryGetProperty("data", out var dataArray) && dataArray.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        CitasConTriaje.Clear();

                        foreach (var item in dataArray.EnumerateArray())
                        {
                            System.Diagnostics.Debug.WriteLine($"[HISTORIAL DEBUG] Item JSON: {item}");

                            var fechaString = item.GetProperty("fechaHora").GetString();
                            System.Diagnostics.Debug.WriteLine($"[HISTORIAL DEBUG] Fecha raw: {fechaString}");

                            var cita = new CitaConTriaje
                            {
                                IdCita = item.GetProperty("idCita").GetInt32(),
                                NombrePaciente = item.GetProperty("nombrePaciente").GetString() ?? "Sin nombre",
                                CedulaPaciente = item.GetProperty("cedulaPaciente").GetString() ?? "",
                                NombreMedico = item.GetProperty("nombreMedico").GetString() ?? "Sin médico",
                                FechaHora = DateTime.TryParse(fechaString, out var fecha) ? fecha : DateTime.MinValue,
                                MotivoConsulta = item.GetProperty("motivoConsulta").GetString() ?? "Sin motivo",
                                NombreSucursal = item.GetProperty("nombreSucursal").GetString() ?? "Sin sucursal",
                                NombreEspecialidad = item.GetProperty("nombreEspecialidad").GetString() ?? "",
                                NivelUrgencia = item.GetProperty("nivelUrgencia").GetInt32()
                            };

                            System.Diagnostics.Debug.WriteLine($"[HISTORIAL DEBUG] Cita creada: {cita.DisplayText}");
                            CitasConTriaje.Add(cita);
                        }

                        System.Diagnostics.Debug.WriteLine($"[HISTORIAL] ✅ {CitasConTriaje.Count} citas cargadas");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HISTORIAL ERROR] {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CargarDatosTriajeAsync(int idCita)
        {
            try
            {
                IsBusy = true;

                // ✅ CORRECCIÓN: Usar el endpoint correcto
                var response = await _apiService.GetAsync<TriajeCompleto>($"api/triaje/cita/{idCita}");

                if (response.Success && response.Data != null)
                {
                    TriajeSeleccionado = response.Data;
                    System.Diagnostics.Debug.WriteLine($"[HISTORIAL] ✅ Triaje cargado para cita {idCita}");
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("Info",
                        $"No se encontró triaje para esta cita: {response.Message}", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HISTORIAL] ❌ Error: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    "Error de conexión al cargar triaje", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string ObtenerTextoUrgencia(int nivel)
        {
            return nivel switch
            {
                1 => "🔴 CRÍTICO",
                2 => "🟠 ALTO",
                3 => "🟡 MEDIO",
                4 => "🟢 BAJO",
                5 => "⚪ NO URGENTE",
                _ => "❓ DESCONOCIDO"
            };
        }

        // ==================== COMANDOS ====================

        [RelayCommand]
        private async Task IrAConsultaMedica()
        {
            if (CitaSeleccionada == null)
            {
                await Application.Current?.MainPage?.DisplayAlert("⚠️ Validación",
                    "Debe seleccionar una cita", "OK");
                return;
            }

            try
            {
                // ✅ CORREGIR: Agregar /// al inicio para rutas absolutas
                var parametros = new Dictionary<string, object>
                {
                    ["citaId"] = CitaSeleccionada.IdCita,
                    ["pacienteNombre"] = CitaSeleccionada.NombrePaciente
                };

                await Shell.Current.GoToAsync("///ConsultaMedicaPage", parametros);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HISTORIAL] ❌ Error navegación: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    "Error al navegar a consulta médica", "OK");
            }
        }
    }
}

// ==================== MODELOS AUXILIARES ====================

public class CitaConTriaje
{
    public int IdCita { get; set; }
    public string NombrePaciente { get; set; } = string.Empty;
    public string CedulaPaciente { get; set; } = string.Empty;
    public string NombreMedico { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string MotivoConsulta { get; set; } = string.Empty;
    public string NombreSucursal { get; set; } = string.Empty;
    public string NombreEspecialidad { get; set; } = string.Empty;
    public DateTime FechaTriaje { get; set; }
    public int NivelUrgencia { get; set; }

    public string DisplayText => $"📅 {FechaHora:dd/MM HH:mm} - {NombrePaciente} - {MotivoConsulta}";
}