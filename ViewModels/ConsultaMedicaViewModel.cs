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
            httpClient.BaseAddress = new Uri("http://192.168.93.154:8081/webservice-slim/");
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
        private bool mostrarResumenTriaje;

        [ObservableProperty]
        private string resumenTriaje = string.Empty;

        // ==================== PUNTO 3: CHECKBOXES PRINCIPALES ====================

        [ObservableProperty]
        private bool habilitarReceta;

        [ObservableProperty]
        private bool habilitarDiagnostico;

        // ==================== PUNTO 4: VENTANA FLOTANTE RECETA ====================

        [ObservableProperty]
        private bool mostrarVentanaReceta;

        [ObservableProperty]
        private string medicamentos = string.Empty;

        [ObservableProperty]
        private string instruccionesReceta = string.Empty;

        [ObservableProperty]
        private DateTime fechaEmision;

        [ObservableProperty]
        private DateTime fechaVencimiento;

        [ObservableProperty]
        private string observacionesReceta = string.Empty;

        // ==================== PUNTO 5: VENTANA FLOTANTE DIAGNÓSTICO ====================

        [ObservableProperty]
        private bool mostrarVentanaDiagnostico;

        [ObservableProperty]
        private string motivoConsulta = string.Empty;

        [ObservableProperty]
        private string sintomatologia = string.Empty;

        [ObservableProperty]
        private string diagnostico = string.Empty;

        [ObservableProperty]
        private string tratamiento = string.Empty;

        [ObservableProperty]
        private string observacionesDiagnostico = string.Empty;

        [ObservableProperty]
        private DateTime fechaSeguimiento;

        // ==================== HISTORIAL ====================

        public ObservableCollection<ConsultaPrevia> ConsultasPrevias { get; private set; }

        [ObservableProperty]
        private bool tieneConsultasPrevias;

        // ==================== CAMBIO DE PROPIEDADES ====================

        partial void OnCitaIdChanged(int value)
        {
            if (value > 0)
            {
                _ = CargarDatosCitaAsync(value);
            }
        }

        // ==================== MÉTODOS API ====================

        private async Task CargarDatosCitaAsync(int idCita)
        {
            try
            {
                IsBusy = true;

                // Cargar datos del triaje para mostrar resumen
                var responseTriaje = await _apiService.GetAsync<TriajeCompleto>($"api/triaje/cita/{idCita}");
                if (responseTriaje.Success && responseTriaje.Data != null)
                {
                    var triaje = responseTriaje.Data;
                    MostrarResumenTriaje = true;
                    ResumenTriaje = $"👤 {PacienteNombre} | 🌡️ {triaje.Temperatura:F1}°C | 💓 {triaje.PresionArterial} | ⚠️ Nivel {triaje.NivelUrgencia}";
                }

                // Cargar consultas previas del paciente
                await CargarConsultasPreviasAsync(idCita);

                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ✅ Datos cargados para cita {idCita}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ❌ Error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CargarConsultasPreviasAsync(int idCita)
        {
            try
            {
                // Obtener historial de consultas del paciente
                var response = await _apiService.GetAsync<List<ConsultaPrevia>>($"api/consultas/historial/cita/{idCita}");

                if (response.Success && response.Data != null && response.Data.Count > 0)
                {
                    ConsultasPrevias.Clear();
                    foreach (var consulta in response.Data)
                    {
                        ConsultasPrevias.Add(consulta);
                    }
                    TieneConsultasPrevias = true;
                    System.Diagnostics.Debug.WriteLine($"[CONSULTA] ✅ Cargadas {ConsultasPrevias.Count} consultas previas");
                }
                else
                {
                    TieneConsultasPrevias = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ❌ Error consultas previas: {ex.Message}");
                TieneConsultasPrevias = false;
            }
        }

        // ==================== COMANDOS PUNTO 3: VENTANAS FLOTANTES ====================

        [RelayCommand]
        private void AbrirVentanaReceta()
        {
            if (HabilitarReceta)
            {
                MostrarVentanaReceta = true;
                System.Diagnostics.Debug.WriteLine("[CONSULTA] ✅ PUNTO 4: Ventana flotante receta abierta");
            }
        }

        [RelayCommand]
        private void CerrarVentanaReceta()
        {
            MostrarVentanaReceta = false;
        }

        [RelayCommand]
        private void AbrirVentanaDiagnostico()
        {
            if (HabilitarDiagnostico)
            {
                MostrarVentanaDiagnostico = true;
                System.Diagnostics.Debug.WriteLine("[CONSULTA] ✅ PUNTO 5: Ventana flotante diagnóstico abierta");
            }
        }

        [RelayCommand]
        private void CerrarVentanaDiagnostico()
        {
            MostrarVentanaDiagnostico = false;
        }

        // ==================== COMANDOS PUNTO 4: RECETA MÉDICA ====================

        [RelayCommand]
        private async Task GuardarReceta()
        {
            if (!ValidarReceta())
                return;

            try
            {
                IsBusy = true;

                // Primero necesitamos crear/obtener la consulta médica
                var idConsulta = await ObtenerOCrearConsultaAsync();
                if (idConsulta == 0)
                {
                    await Application.Current?.MainPage?.DisplayAlert("❌ Error",
                        "No se pudo crear la consulta médica", "OK");
                    return;
                }

                // Crear objeto receta
                var receta = new RecetaMedica
                {
                    IdConsulta = idConsulta,
                    Medicamentos = Medicamentos,
                    Instrucciones = InstruccionesReceta,
                    FechaEmision = FechaEmision.ToString("yyyy-MM-dd"),
                    FechaVencimiento = FechaVencimiento.ToString("yyyy-MM-dd"),
                    Observaciones = ObservacionesReceta
                };

                // ✅ CORREGIDO: Especificar tipo de respuesta
                var response = await _apiService.PostAsync<object>("api/recetas", receta);

                if (response.Success)
                {
                    await Application.Current?.MainPage?.DisplayAlert("✅ PUNTO 4 Completado",
                        "Receta médica guardada exitosamente", "OK");

                    LimpiarReceta();
                    CerrarVentanaReceta();
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("❌ Error",
                        response.Message ?? "Error al guardar receta", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ❌ Error receta: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("❌ Error",
                    "Error de conexión al guardar receta", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void LimpiarReceta()
        {
            Medicamentos = string.Empty;
            InstruccionesReceta = string.Empty;
            ObservacionesReceta = string.Empty;
            FechaEmision = DateTime.Today;
            FechaVencimiento = DateTime.Today.AddDays(30);
        }

        // ==================== COMANDOS PUNTO 5: DIAGNÓSTICO ====================

        [RelayCommand]
        private async Task GuardarDiagnostico()
        {
            if (!ValidarDiagnostico())
                return;

            try
            {
                IsBusy = true;

                // Obtener el historial del paciente
                var idHistorial = await ObtenerHistorialPacienteAsync();
                if (idHistorial == 0)
                {
                    await Application.Current?.MainPage?.DisplayAlert("❌ Error",
                        "No se pudo obtener el historial del paciente", "OK");
                    return;
                }

                // Crear objeto consulta médica
                var consulta = new ConsultaMedica
                {
                    IdCita = CitaId,
                    IdHistorial = idHistorial,
                    MotivoConsulta = MotivoConsulta,
                    Sintomatologia = Sintomatologia,
                    Diagnostico = Diagnostico,
                    Tratamiento = Tratamiento,
                    Observaciones = ObservacionesDiagnostico,
                    FechaSeguimiento = FechaSeguimiento.ToString("yyyy-MM-dd")
                };

                // ✅ CORREGIDO: Especificar tipo de respuesta
                var response = await _apiService.PostAsync<ConsultaCreada>("api/consultas-medicas", consulta);

                if (response.Success)
                {
                    await Application.Current?.MainPage?.DisplayAlert("✅ PUNTO 5 Completado",
                        "Diagnóstico guardado exitosamente", "OK");

                    LimpiarDiagnostico();
                    CerrarVentanaDiagnostico();

                    // Recargar consultas previas
                    await CargarConsultasPreviasAsync(CitaId);
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("❌ Error",
                        response.Message ?? "Error al guardar diagnóstico", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ❌ Error diagnóstico: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("❌ Error",
                    "Error de conexión al guardar diagnóstico", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void LimpiarDiagnostico()
        {
            MotivoConsulta = string.Empty;
            Sintomatologia = string.Empty;
            Diagnostico = string.Empty;
            Tratamiento = string.Empty;
            ObservacionesDiagnostico = string.Empty;
            FechaSeguimiento = DateTime.Today.AddDays(7);
        }

        // ==================== MÉTODOS DE VALIDACIÓN ====================

        private bool ValidarReceta()
        {
            if (string.IsNullOrWhiteSpace(Medicamentos))
            {
                Application.Current?.MainPage?.DisplayAlert("⚠️ Validación",
                    "Los medicamentos son requeridos", "OK");
                return false;
            }

            if (FechaVencimiento <= FechaEmision)
            {
                Application.Current?.MainPage?.DisplayAlert("⚠️ Validación",
                    "La fecha de vencimiento debe ser posterior a la emisión", "OK");
                return false;
            }

            return true;
        }

        private bool ValidarDiagnostico()
        {
            if (string.IsNullOrWhiteSpace(MotivoConsulta))
            {
                Application.Current?.MainPage?.DisplayAlert("⚠️ Validación",
                    "El motivo de consulta es requerido", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Diagnostico))
            {
                Application.Current?.MainPage?.DisplayAlert("⚠️ Validación",
                    "El diagnóstico es requerido", "OK");
                return false;
            }

            return true;
        }

        // ==================== MÉTODOS AUXILIARES ====================

        private async Task<int> ObtenerOCrearConsultaAsync()
        {
            try
            {
                // Verificar si ya existe una consulta para esta cita
                var response = await _apiService.GetAsync<ConsultaExistente>($"api/consultas-medicas/cita/{CitaId}");

                if (response.Success && response.Data != null)
                {
                    return response.Data.IdConsulta;
                }

                // Si no existe, crear una nueva consulta básica
                var nuevaConsulta = new ConsultaMedica
                {
                    IdCita = CitaId,
                    IdHistorial = await ObtenerHistorialPacienteAsync(),
                    MotivoConsulta = "Consulta médica",
                    Diagnostico = "En proceso",
                    Sintomatologia = "",
                    Tratamiento = "",
                    Observaciones = ""
                };

                // ✅ CORREGIDO: Especificar tipo de respuesta
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

// ✅ MODELO TRIAJE COMPLETO (NECESARIO PARA EL RESUMEN)
public class TriajeCompleto
{
    public int IdTriaje { get; set; }
    public int IdCita { get; set; }
    public int IdEnfermero { get; set; }
    public string NombreEnfermero { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public int NivelUrgencia { get; set; }
    public string EstadoTriaje { get; set; } = string.Empty;

    // Signos vitales
    public double? Temperatura { get; set; }
    public string? PresionArterial { get; set; }
    public int? FrecuenciaCardiaca { get; set; }
    public int? FrecuenciaRespiratoria { get; set; }
    public int? SaturacionOxigeno { get; set; }

    // Medidas antropométricas
    public double? Peso { get; set; }
    public double? Talla { get; set; }
    public double? IMC { get; set; }

    // Observaciones
    public string? Observaciones { get; set; }
}