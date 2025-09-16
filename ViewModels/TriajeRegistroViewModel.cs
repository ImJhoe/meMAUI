// ViewModels/TriajeRegistroViewModel.cs - PUNTO 1: Registro de Triaje
using ClinicaApp.Models;
using ClinicaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ClinicaApp.ViewModels
{
    public partial class TriajeRegistroViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public TriajeRegistroViewModel()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://192.168.93.154:8081/webservice-slim/");
            _apiService = new ApiService(httpClient);
            InitializeViewModel();
        }

        public TriajeRegistroViewModel(ApiService apiService)
        {
            _apiService = apiService;
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            Title = "PUNTO 1: Registro de Triaje";

            // Inicializar colecciones
            CitasDisponibles = new ObservableCollection<CitaTriaje>();
            NivelesUrgencia = new ObservableCollection<NivelUrgencia>();

            // Cargar niveles de urgencia
            CargarNivelesUrgencia();

            // Cargar citas disponibles
            _ = CargarCitasDisponiblesAsync();

            System.Diagnostics.Debug.WriteLine("[TRIAJE] ✅ PUNTO 1: ViewModel inicializado");
        }

        // ==================== PROPIEDADES OBSERVABLES ====================

        // SELECCIÓN DE CITA
        public ObservableCollection<CitaTriaje> CitasDisponibles { get; private set; }

        [ObservableProperty]
        private CitaTriaje? citaSeleccionada;

        [ObservableProperty]
        private bool mostrarInfoPaciente;

        [ObservableProperty]
        private string infoPaciente = string.Empty;

        // SIGNOS VITALES
        [ObservableProperty]
        private string temperatura = string.Empty;

        [ObservableProperty]
        private string presionArterial = string.Empty;

        [ObservableProperty]
        private string frecuenciaCardiaca = string.Empty;

        [ObservableProperty]
        private string frecuenciaRespiratoria = string.Empty;

        [ObservableProperty]
        private string saturacionOxigeno = string.Empty;

        // MEDIDAS ANTROPOMÉTRICAS
        [ObservableProperty]
        private string peso = string.Empty;

        [ObservableProperty]
        private string talla = string.Empty;

        [ObservableProperty]
        private bool mostrarIMC;

        [ObservableProperty]
        private double imcCalculado;

        [ObservableProperty]
        private string clasificacionIMC = string.Empty;

        // NIVEL DE URGENCIA
        public ObservableCollection<NivelUrgencia> NivelesUrgencia { get; private set; }

        [ObservableProperty]
        private NivelUrgencia? nivelUrgenciaSeleccionado;

        // OBSERVACIONES
        [ObservableProperty]
        private string observaciones = string.Empty;

        // ==================== CAMBIO DE PROPIEDADES ====================

        partial void OnCitaSeleccionadaChanged(CitaTriaje? value)
        {
            if (value != null)
            {
                MostrarInfoPaciente = true;
                InfoPaciente = $"👤 {value.NombrePaciente} | 📅 {value.FechaHora:dd/MM/yyyy HH:mm} | 👨‍⚕️ Dr. {value.NombreMedico}";
            }
            else
            {
                MostrarInfoPaciente = false;
                InfoPaciente = string.Empty;
            }
        }

        partial void OnPesoChanged(string value)
        {
            CalcularIMC();
        }

        partial void OnTallaChanged(string value)
        {
            CalcularIMC();
        }

        // ==================== MÉTODOS AUXILIARES ====================

        private void CalcularIMC()
        {
            if (double.TryParse(Peso, out double pesoNum) &&
                double.TryParse(Talla, out double tallaNum) &&
                pesoNum > 0 && tallaNum > 0)
            {
                double tallaMetros = tallaNum / 100;
                ImcCalculado = pesoNum / (tallaMetros * tallaMetros);
                MostrarIMC = true;

                // Clasificación del IMC
                ClasificacionIMC = ImcCalculado switch
                {
                    < 18.5 => "⬇️ Bajo peso",
                    >= 18.5 and < 25 => "✅ Peso normal",
                    >= 25 and < 30 => "⬆️ Sobrepeso",
                    >= 30 and < 35 => "🔴 Obesidad grado I",
                    >= 35 and < 40 => "🔴 Obesidad grado II",
                    >= 40 => "🔴 Obesidad grado III",
                    _ => "📊 Calculando..."
                };
            }
            else
            {
                MostrarIMC = false;
                ImcCalculado = 0;
                ClasificacionIMC = string.Empty;
            }
        }

        private void CargarNivelesUrgencia()
        {
            NivelesUrgencia.Clear();
            NivelesUrgencia.Add(new NivelUrgencia { Nivel = 1, Descripcion = "🟢 Nivel 1 - No urgente" });
            NivelesUrgencia.Add(new NivelUrgencia { Nivel = 2, Descripcion = "🟡 Nivel 2 - Poco urgente" });
            NivelesUrgencia.Add(new NivelUrgencia { Nivel = 3, Descripcion = "🟠 Nivel 3 - Urgente" });
            NivelesUrgencia.Add(new NivelUrgencia { Nivel = 4, Descripcion = "🔴 Nivel 4 - Muy urgente" });
            NivelesUrgencia.Add(new NivelUrgencia { Nivel = 5, Descripcion = "🔴 Nivel 5 - Emergencia" });
        }

        // ==================== MÉTODOS API ====================

        private async Task CargarCitasDisponiblesAsync()
        {
            try
            {
                IsBusy = true;
                var response = await _apiService.GetAsync<List<CitaTriaje>>("api/citas/pendientes-triaje");

                if (response.Success && response.Data != null)
                {
                    CitasDisponibles.Clear();
                    foreach (var cita in response.Data)
                    {
                        CitasDisponibles.Add(cita);
                    }
                    System.Diagnostics.Debug.WriteLine($"[TRIAJE] ✅ Cargadas {CitasDisponibles.Count} citas");
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("Error",
                        response.Message ?? "Error al cargar citas", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TRIAJE] ❌ Error: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("Error",
                    "Error de conexión al cargar citas", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ==================== COMANDOS ====================

        [RelayCommand]
        private async Task RegistrarTriaje()
        {
            if (!ValidarFormulario())
                return;

            try
            {
                IsBusy = true;

                // Crear objeto triaje
                var triaje = new TriajeRegistro
                {
                    IdCita = CitaSeleccionada?.IdCita ?? 0,
                    IdEnfermero = App.CurrentUser?.IdUsuario ?? 0, // Usuario logueado
                    Temperatura = double.TryParse(Temperatura, out double temp) ? temp : null,
                    PresionArterial = PresionArterial,
                    FrecuenciaCardiaca = int.TryParse(FrecuenciaCardiaca, out int fc) ? fc : null,
                    FrecuenciaRespiratoria = int.TryParse(FrecuenciaRespiratoria, out int fr) ? fr : null,
                    SaturacionOxigeno = int.TryParse(SaturacionOxigeno, out int so) ? so : null,
                    Peso = double.TryParse(Peso, out double pesoNum) ? pesoNum : null,
                    Talla = double.TryParse(Talla, out double tallaNum) ? tallaNum : null,
                    NivelUrgencia = NivelUrgenciaSeleccionado?.Nivel ?? 1,
                    Observaciones = Observaciones
                };

                var response = await _apiService.PostAsync<TriajeRegistro, object>("api/triaje", triaje);

                if (response.Success)
                {
                    await Application.Current?.MainPage?.DisplayAlert("✅ Éxito",
                        "Triaje registrado correctamente", "OK");

                    // Limpiar formulario y recargar citas
                    LimpiarFormulario();
                    await CargarCitasDisponiblesAsync();
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("❌ Error",
                        response.Message ?? "Error al registrar triaje", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TRIAJE] ❌ Error: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("❌ Error",
                    "Error de conexión al registrar triaje", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void LimpiarFormulario()
        {
            CitaSeleccionada = null;
            Temperatura = string.Empty;
            PresionArterial = string.Empty;
            FrecuenciaCardiaca = string.Empty;
            FrecuenciaRespiratoria = string.Empty;
            SaturacionOxigeno = string.Empty;
            Peso = string.Empty;
            Talla = string.Empty;
            NivelUrgenciaSeleccionado = null;
            Observaciones = string.Empty;
            MostrarIMC = false;
            MostrarInfoPaciente = false;
        }

        private bool ValidarFormulario()
        {
            if (CitaSeleccionada == null)
            {
                Application.Current?.MainPage?.DisplayAlert("⚠️ Validación",
                    "Debe seleccionar una cita", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Temperatura))
            {
                Application.Current?.MainPage?.DisplayAlert("⚠️ Validación",
                    "La temperatura es requerida", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(PresionArterial))
            {
                Application.Current?.MainPage?.DisplayAlert("⚠️ Validación",
                    "La presión arterial es requerida", "OK");
                return false;
            }

            if (NivelUrgenciaSeleccionado == null)
            {
                Application.Current?.MainPage?.DisplayAlert("⚠️ Validación",
                    "Debe seleccionar un nivel de urgencia", "OK");
                return false;
            }

            return true;
        }
    }
}

// ==================== MODELOS AUXILIARES ====================

public class CitaTriaje
{
    public int IdCita { get; set; }
    public string NombrePaciente { get; set; } = string.Empty;
    public string NombreMedico { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string MotivoConsulta { get; set; } = string.Empty;

    public string DisplayText => $"📅 {FechaHora:dd/MM HH:mm} - {NombrePaciente} - {MotivoConsulta}";
}

public class NivelUrgencia
{
    public int Nivel { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}

public class TriajeRegistro
{
    public int IdCita { get; set; }
    public int IdEnfermero { get; set; }
    public double? Temperatura { get; set; }
    public string? PresionArterial { get; set; }
    public int? FrecuenciaCardiaca { get; set; }
    public int? FrecuenciaRespiratoria { get; set; }
    public int? SaturacionOxigeno { get; set; }
    public double? Peso { get; set; }
    public double? Talla { get; set; }
    public int NivelUrgencia { get; set; }
    public string? Observaciones { get; set; }
}