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
            ConsultasPrevias = new ObservableCollection<ConsultaPrevia>();
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

        // ==================== PROPIEDADES PARA VENTANAS ====================
        [ObservableProperty]
        private bool mostrarVentanaDiagnostico;

        [ObservableProperty]
        private bool mostrarVentanaReceta;

        [ObservableProperty]
        private bool habilitarReceta = true;

        // ==================== PROPIEDADES PARA FORMULARIOS ====================
        [ObservableProperty]
        private string tratamiento = string.Empty;

        [ObservableProperty]
        private string observacionesDiagnostico = string.Empty;

        [ObservableProperty]
        private string medicamentos = string.Empty;

        // ==================== PROPIEDADES DE TRIAJE ====================
        [ObservableProperty]
        private TriajeCompleto? triajeData;

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
                await CargarTriajeAsync();
                await CargarConsultasPreviasAsync();
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ✅ Datos cargados para cita {CitaId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ❌ Error: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("Error", "Error al cargar datos de la consulta", "OK");
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
                var response = await _apiService.GetAsync<TriajeCompleto>($"api/triaje/por-cita/{CitaId}");
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

        private string GenerarResumenTriaje(TriajeCompleto triaje)
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
                await Application.Current?.MainPage?.DisplayAlert("⚠️ Validación", "El diagnóstico es obligatorio", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                var idConsulta = await ObtenerOCrearConsultaAsync();
                if (idConsulta == 0)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Error", "Error al crear consulta médica", "OK");
                    return;
                }

                if (MostrarTratamiento)
                {
                    await GuardarTratamientoAsync(idConsulta);
                }

                if (MostrarReceta)
                {
                    await GuardarRecetaAsync(idConsulta);
                }

                await Application.Current?.MainPage?.DisplayAlert("✅ Éxito", "Consulta médica guardada correctamente", "OK");
                await Shell.Current.GoToAsync($"CitaCompletaPage?citaId={CitaId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ❌ Error: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("Error", "Error al guardar consulta", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GuardarDiagnostico()
        {
            if (string.IsNullOrWhiteSpace(Diagnostico) || string.IsNullOrWhiteSpace(Tratamiento))
            {
                await Application.Current?.MainPage?.DisplayAlert("⚠️ Validación",
                    "Debe completar el diagnóstico y tratamiento", "OK");
                return;
            }

            try
            {
                IsBusy = true;

                // ✅ CORRECCIÓN: Obtener el historial ANTES de crear el objeto
                var idHistorial = await ObtenerHistorialPacienteAsync();

                if (idHistorial == 0)
                {
                    await Application.Current?.MainPage?.DisplayAlert("❌ Error",
                        "No se pudo obtener el historial del paciente", "OK");
                    return;
                }

                var consultaMedica = new
                {
                    idCita = CitaId,
                    idHistorial = idHistorial,  // ✅ Usar el ID real obtenido
                    motivoConsulta = MotivoConsulta,
                    sintomatologia = Sintomatologia,
                    diagnostico = Diagnostico,
                    tratamiento = Tratamiento,
                    observaciones = ObservacionesDiagnostico,
                    fechaSeguimiento = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd")
                };

                System.Diagnostics.Debug.WriteLine($"[CONSULTA] Enviando datos: CitaId={CitaId}, HistorialId={idHistorial}");

                var response = await _apiService.PostAsync<ConsultaCreada>("api/consultas-medicas", consultaMedica);

                if (response.Success && response.Data != null)
                {
                    _consultaMedicaId = response.Data.IdConsulta;

                    await Application.Current?.MainPage?.DisplayAlert("✅ Éxito",
                        "Consulta médica registrada exitosamente", "OK");

                    MostrarVentanaDiagnostico = false;
                    HabilitarReceta = true;
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("❌ Error",
                        response.Message ?? "Error al guardar consulta", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ❌ Error: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    "Error de conexión al guardar consulta", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        // ✅ AGREGAR: Variable para almacenar ID de consulta
        private int _consultaMedicaId = 0;

        [RelayCommand]
        private void AbrirVentanaDiagnostico() => MostrarVentanaDiagnostico = true;

        [RelayCommand]
        private void CerrarVentanaDiagnostico() => MostrarVentanaDiagnostico = false;

        [RelayCommand]
        private void LimpiarDiagnostico()
        {
            Diagnostico = string.Empty;
            Tratamiento = string.Empty;
            ObservacionesDiagnostico = string.Empty;
        }

        [RelayCommand]
        private void AbrirVentanaReceta() => MostrarVentanaReceta = true;

        [RelayCommand]
        private void CerrarVentanaReceta() => MostrarVentanaReceta = false;

        [RelayCommand]
        private async Task GuardarReceta()
        {
            if (string.IsNullOrWhiteSpace(Medicamentos))
            {
                await Application.Current?.MainPage?.DisplayAlert("⚠️ Validación",
                    "Debe especificar los medicamentos", "OK");
                return;
            }

            // ✅ CORRECCIÓN 4: Verificar que existe consulta médica
            if (_consultaMedicaId == 0)
            {
                await Application.Current?.MainPage?.DisplayAlert("⚠️ Validación",
                    "Debe guardar primero el diagnóstico", "OK");
                return;
            }

            try
            {
                IsBusy = true;

                var receta = new
                {
                    id_consulta = _consultaMedicaId,  // ✅ CORRECCIÓN: Usar ID de consulta médica, no de cita
                    medicamentos = Medicamentos,
                    fecha_prescripcion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var response = await _apiService.PostAsync<object>("api/recetas", receta);

                if (response.Success)
                {
                    await Application.Current?.MainPage?.DisplayAlert("✅ Éxito",
                        "Receta registrada exitosamente", "OK");

                    MostrarVentanaReceta = false;
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
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    "Error de conexión al guardar receta", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void LimpiarReceta() => Medicamentos = string.Empty;

        // ==================== MÉTODOS AUXILIARES ====================
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
                var responseExistente = await _apiService.GetAsync<ConsultaExistente>($"api/consultas-medicas/por-cita/{CitaId}");
                if (responseExistente.Success && responseExistente.Data != null)
                {
                    return responseExistente.Data.IdConsulta;
                }

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


        // ✅ CORRECCIÓN: Asegurarse de que retorne el ID correcto
        private async Task<int> ObtenerHistorialPacienteAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<HistorialPaciente>($"api/historial/cita/{CitaId}");

                System.Diagnostics.Debug.WriteLine($"[CONSULTA] Response Success: {response.Success}");
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] Response Data: {response.Data?.IdHistorial}");

                if (response.Success && response.Data != null && response.Data.IdHistorial > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[CONSULTA] Historial obtenido: ID={response.Data.IdHistorial}");
                    return response.Data.IdHistorial;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[CONSULTA] ⚠️ No se encontró historial válido para cita {CitaId}");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONSULTA] ❌ Error obtener historial: {ex.Message}");
                return 1;
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
    }
}