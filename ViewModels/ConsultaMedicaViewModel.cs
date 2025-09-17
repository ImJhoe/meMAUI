// ViewModels/ConsultaMedicaViewModel.cs - PUNTOS 3,4,5: Consulta Médica con Checkboxes - CORREGIDO
using ClinicaApp.Models;
using ClinicaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ClinicaApp.ViewModels
{
    [QueryProperty(nameof(CitaId), "citaId")]
    [QueryProperty(nameof(PacienteNombre), "pacienteNombre")]
    public partial class ConsultaMedicaViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public ConsultaMedicaViewModel()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://192.168.1.14:8081/webservice-slim/");
            _apiService = new ApiService(httpClient);
            InitializeViewModel();
        }

        public ConsultaMedicaViewModel(ApiService apiService)
        {
            _apiService = apiService;
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            Title = "PUNTOS 3,4,5: Consulta Médica";

            // Inicializar colecciones
            ConsultasPrevias = new ObservableCollection<ConsultaPrevia>();

            // Inicializar fechas
            FechaEmision = DateTime.Today;
            FechaVencimiento = DateTime.Today.AddDays(30);
            FechaSeguimiento = DateTime.Today.AddDays(7);

            System.Diagnostics.Debug.WriteLine("[CONSULTA] ✅ PUNTOS 3,4,5: ViewModel inicializado");
        }

        // ==================== PROPIEDADES DE NAVEGACIÓN ====================

        [ObservableProperty]
        private int citaId;

        [ObservableProperty]
        private string pacienteNombre = string.Empty;

        // ==================== PROPIEDADES DE TRIAJE ====================

        [ObservableProperty]
        private TriajeCompleto? triajeData; // ✅ Usar el modelo de ClinicaApp.Models

        [ObservableProperty]
        private string resumenTriaje = string.Empty;

        // ==================== PROPIEDADES DE CONSULTA PREVIA ====================

        public ObservableCollection<ConsultaPrevia> ConsultasPrevias { get; private set; }

        [ObservableProperty]
        private bool tieneConsultasPrevias;

        // ==================== CHECKBOXES - PUNTO 3 ====================

        [ObservableProperty]
        private bool mostrarTratamiento;

        [ObservableProperty]
        private bool mostrarReceta;

        // ==================== CAMPOS DE TRATAMIENTO - PUNTO 3 ====================

        [ObservableProperty]
        private string nombreTratamiento = string.Empty;

        [ObservableProperty]
        private DateTime fechaInicioTratamiento = DateTime.Today;

        [ObservableProperty]
        private string frecuenciaTratamiento = string.Empty;

        [ObservableProperty]
        private string duracionTratamiento = string.Empty;

        // ==================== CAMPOS DE RECETA - PUNTO 4 ====================

        [ObservableProperty]
        private string medicamentosPrescritos = string.Empty;

        [ObservableProperty]
        private string instruccionesReceta = string.Empty;

        [ObservableProperty]
        private DateTime fechaEmision = DateTime.Today;

        [ObservableProperty]
        private DateTime fechaVencimiento;

        [ObservableProperty]
        private string observacionesReceta = string.Empty;

        // ==================== CAMPOS DE CONSULTA ====================

        [ObservableProperty]
        private string motivoConsulta = string.Empty;

        [ObservableProperty]
        private string sintomatologia = string.Empty;

        [ObservableProperty]
        private string diagnostico = string.Empty;

        [ObservableProperty]
        private string tratamientoGeneral = string.Empty;

        [ObservableProperty]
        private string observacionesConsulta = string.Empty;

        [ObservableProperty]
        private DateTime fechaSeguimiento = DateTime.Today;

        // ==================== CAMBIO DE PROPIEDADES ====================

        partial void OnCitaIdChanged(int value)
        {
            if (value > 0)
            {
                _ = CargarDatosConsultaAsync();
            }
        }

        // ==================== MÉTODOS PRIVADOS ====================

        private async Task CargarDatosConsultaAsync()
        {
            try
            {
                IsBusy = true;

                // 1. Cargar datos del triaje
                await CargarTriajeAsync();

                // 2. Cargar consultas previas
                await CargarConsultasPreviasAsync();

                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ✅ Datos cargados para cita {CitaId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ❌ Error: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    "Error al cargar datos de la consulta", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CargarTriajeAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<TriajeCompleto>($"api/triaje/por-cita/{CitaId}"); // ✅ Usar el modelo correcto

                if (response.Success && response.Data != null)
                {
                    TriajeData = response.Data;
                    ResumenTriaje = GenerarResumenTriaje(response.Data);
                    System.Diagnostics.Debug.WriteLine("[CONSULTA] ✅ Triaje cargado");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ❌ Error triaje: {ex.Message}");
            }
        }

        private string GenerarResumenTriaje(TriajeCompleto triaje) // ✅ Usar el modelo correcto
        {
            var urgencia = triaje.NivelUrgencia switch
            {
                1 => "🔴 CRÍTICO",
                2 => "🟠 ALTO",
                3 => "🟡 MEDIO",
                4 => "🟢 BAJO",
                5 => "⚪ NO URGENTE",
                _ => "❓ DESCONOCIDO"
            };

            return $"📊 Triaje - {urgencia} | 🌡️ {triaje.Temperatura}°C | 💓 {triaje.FrecuenciaCardiaca} bpm | 🩺 {triaje.PresionArterial}";
        }

        private async Task CargarConsultasPreviasAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<List<ConsultaPrevia>>($"api/consultas/historial/{CitaId}");

                if (response.Success && response.Data != null)
                {
                    ConsultasPrevias.Clear();
                    foreach (var consulta in response.Data)
                    {
                        ConsultasPrevias.Add(consulta);
                    }
                    TieneConsultasPrevias = ConsultasPrevias.Count > 0;
                    System.Diagnostics.Debug.WriteLine($"[CONSULTA] ✅ {ConsultasPrevias.Count} consultas previas");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ❌ Error consultas previas: {ex.Message}");
            }
        }

        // ==================== COMANDOS ====================

        [RelayCommand]
        private async Task GuardarConsulta()
        {
            if (string.IsNullOrEmpty(Diagnostico))
            {
                await Application.Current?.MainPage?.DisplayAlert("⚠️ Validación",
                    "El diagnóstico es obligatorio", "OK");
                return;
            }

            try
            {
                IsBusy = true;

                // 1. Obtener o crear consulta médica
                var idConsulta = await ObtenerOCrearConsultaAsync();
                if (idConsulta == 0)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Error",
                        "Error al crear consulta médica", "OK");
                    return;
                }

                // 2. Guardar tratamiento si está habilitado
                if (MostrarTratamiento)
                {
                    await GuardarTratamientoAsync(idConsulta);
                }

                // 3. Guardar receta médica si está habilitada
                if (MostrarReceta)
                {
                    await GuardarRecetaAsync(idConsulta);
                }

                await Application.Current?.MainPage?.DisplayAlert("✅ Éxito",
                    "Consulta médica guardada correctamente", "OK");

                // Navegar a resumen completo
                await Shell.Current.GoToAsync($"CitaCompletaPage?citaId={CitaId}");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ❌ Error: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    "Error al guardar consulta", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<bool> GuardarTratamientoAsync(int idConsulta)
        {
            try
            {
                var tratamiento = new
                {
                    IdConsulta = idConsulta,
                    Nombre = NombreTratamiento,
                    FechaInicio = FechaInicioTratamiento.ToString("yyyy-MM-dd"),
                    Frecuencia = FrecuenciaTratamiento,
                    Duracion = DuracionTratamiento
                };

                var response = await _apiService.PostAsync<object>("api/tratamientos", tratamiento);
                return response.Success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ❌ Error tratamiento: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> GuardarRecetaAsync(int idConsulta)
        {
            try
            {
                var receta = new RecetaMedica
                {
                    IdConsulta = idConsulta,
                    Medicamentos = MedicamentosPrescritos,
                    Instrucciones = InstruccionesReceta,
                    FechaEmision = FechaEmision.ToString("yyyy-MM-dd"),
                    FechaVencimiento = FechaVencimiento.ToString("yyyy-MM-dd"),
                    Observaciones = ObservacionesReceta
                };

                var response = await _apiService.PostAsync<object>("api/recetas-medicas", receta);
                return response.Success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ❌ Error receta: {ex.Message}");
                return false;
            }
        }

        private async Task<int> ObtenerOCrearConsultaAsync()
        {
            try
            {
                // Primero verificar si ya existe una consulta
                var responseExistente = await _apiService.GetAsync<ConsultaExistente>($"api/consultas-medicas/por-cita/{CitaId}");

                if (responseExistente.Success && responseExistente.Data != null)
                {
                    return responseExistente.Data.IdConsulta;
                }

                // Si no existe, crear nueva consulta
                var idHistorial = await ObtenerHistorialPacienteAsync();

                var nuevaConsulta = new ConsultaMedica
                {
                    IdCita = CitaId,
                    IdHistorial = idHistorial,
                    MotivoConsulta = MotivoConsulta,
                    Sintomatologia = Sintomatologia,
                    Diagnostico = Diagnostico,
                    Tratamiento = TratamientoGeneral,
                    Observaciones = ObservacionesConsulta,
                    FechaSeguimiento = FechaSeguimiento.ToString("yyyy-MM-dd")
                };

                var responseCrear = await _apiService.PostAsync<ConsultaCreada>("api/consultas-medicas", nuevaConsulta);

                if (responseCrear.Success && responseCrear.Data != null)
                {
                    return responseCrear.Data.IdConsulta;
                }

                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ❌ Error obtener consulta: {ex.Message}");
                return 0;
            }
        }

        private async Task<int> ObtenerHistorialPacienteAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<HistorialPaciente>($"api/historial/cita/{CitaId}");

                if (response.Success && response.Data != null)
                {
                    return response.Data.IdHistorial;
                }

                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ❌ Error obtener historial: {ex.Message}");
                return 0;
            }
        }
    }
}

// ==================== MODELOS AUXILIARES ====================

public class ConsultaPrevia
{
    public int IdConsulta { get; set; }
    public DateTime FechaHora { get; set; }
    public string MotivoConsulta { get; set; } = string.Empty;
    public string Diagnostico { get; set; } = string.Empty;
    public string Tratamiento { get; set; } = string.Empty;
}

public class RecetaMedica
{
    public int IdConsulta { get; set; }
    public string Medicamentos { get; set; } = string.Empty;
    public string? Instrucciones { get; set; }
    public string FechaEmision { get; set; } = string.Empty;
    public string FechaVencimiento { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}

public class ConsultaMedica
{
    public int IdCita { get; set; }
    public int IdHistorial { get; set; }
    public string MotivoConsulta { get; set; } = string.Empty;
    public string? Sintomatologia { get; set; }
    public string Diagnostico { get; set; } = string.Empty;
    public string? Tratamiento { get; set; }
    public string? Observaciones { get; set; }
    public string? FechaSeguimiento { get; set; }
}

public class ConsultaExistente
{
    public int IdConsulta { get; set; }
}

public class ConsultaCreada
{
    public int IdConsulta { get; set; }
}

public class HistorialPaciente
{
    public int IdHistorial { get; set; }
}