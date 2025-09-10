using ClinicaApp.Models;
using ClinicaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ClinicaApp.ViewModels
{
    public partial class MedicoConsultaViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        // Constructor sin parámetros para XAML
        public MedicoConsultaViewModel()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://192.168.1.8:8081/webservice-slim/");
            _apiService = new ApiService(httpClient);
            InitializeViewModel();
        }

        // Constructor con DI
        public MedicoConsultaViewModel(ApiService apiService)
        {
            _apiService = apiService;
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            Title = "Consultar Médicos - Punto 2";
            System.Diagnostics.Debug.WriteLine("[MEDICO CONSULTA] ViewModel inicializado");

            // Inicializar colecciones
            Medicos = new ObservableCollection<MedicoConHorarios>();

            // Cargar datos iniciales
            CargarDatosIniciales();
        }

        // Propiedades observables
        public ObservableCollection<MedicoConHorarios> Medicos { get; private set; }

        [ObservableProperty]
        private bool noHayMedicos;

        // Método para cargar datos iniciales
        private async void CargarDatosIniciales()
        {
            await CargarMedicosAsync();
        }

        [RelayCommand]
        private async Task CargarMedicos()
        {
            await CargarMedicosAsync();
        }

        private async Task CargarMedicosAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MEDICO CONSULTA] Cargando médicos...");
                ShowLoading(true);

                var response = await _apiService.ObtenerMedicosAsync();

                if (response.Success && response.Data != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] {response.Data.Count} médicos obtenidos");

                    Medicos.Clear();
                    foreach (var medico in response.Data)
                    {
                        var medicoConHorarios = new MedicoConHorarios
                        {
                            IdMedico = medico.IdMedico,
                            IdDoctor = medico.IdDoctor,
                            Nombres = medico.Nombres,
                            Apellidos = medico.Apellidos,
                            Cedula = medico.Cedula,
                            Correo = medico.Correo,
                            NombreEspecialidad = medico.NombreEspecialidad,
                            TituloProfesional = medico.TituloProfesional,
                            MostrarHorarios = false,
                            HorariosAsignados = new ObservableCollection<Horario>()
                        };

                        Medicos.Add(medicoConHorarios);
                        System.Diagnostics.Debug.WriteLine($"  - {medicoConHorarios.NombreCompleto}");
                    }

                    NoHayMedicos = !Medicos.Any();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Error: {response.Message}");
                    ShowError($"Error cargando médicos: {response.Message}");
                    NoHayMedicos = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Exception: {ex.Message}");
                ShowError($"Error inesperado: {ex.Message}");
                NoHayMedicos = true;
            }
            finally
            {
                ShowLoading(false);
            }
        }

        [RelayCommand]
        private async Task VerHorarios(MedicoConHorarios medico)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Cargando horarios para {medico.NombreCompleto}");
                ShowLoading(true);

                // Usar IdDoctor para la consulta (según tu API)
                int idMedicoParaAPI = medico.IdDoctor > 0 ? medico.IdDoctor : medico.IdMedico;

                var response = await _apiService.ObtenerHorariosPorMedicoAsync(idMedicoParaAPI);

                if (response.Success && response.Data != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] {response.Data.Count} horarios obtenidos");

                    medico.HorariosAsignados.Clear();
                    foreach (var horario in response.Data)
                    {
                        medico.HorariosAsignados.Add(horario);
                        System.Diagnostics.Debug.WriteLine($"  - {horario.HorarioTexto}");
                    }

                    medico.MostrarHorarios = true;

                    if (!response.Data.Any())
                    {
                        ShowError($"{medico.NombreCompleto} no tiene horarios asignados");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Error horarios: {response.Message}");
                    ShowError($"Error cargando horarios: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Exception horarios: {ex.Message}");
                ShowError($"Error inesperado: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        [RelayCommand]
        private async Task EditarHorarios(MedicoConHorarios medico)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Editando horarios para {medico.NombreCompleto}");

                // Por ahora, mostrar los horarios si no están visibles
                if (!medico.MostrarHorarios)
                {
                    await VerHorarios(medico);
                }

                // TODO: Implementar modal o navegación para editar horarios
                ShowError($"Función de edición de horarios para {medico.NombreCompleto} - En desarrollo");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Exception editar: {ex.Message}");
                ShowError($"Error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task EditarHorarioIndividual(Horario horario)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Editando horario individual: {horario.HorarioTexto}");

                // TODO: Implementar modal para editar horario específico
                ShowError($"Editar horario: {horario.HorarioTexto} - En desarrollo");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Exception editar horario: {ex.Message}");
                ShowError($"Error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task VolverAlMenu()
        {
            await Shell.Current.GoToAsync("//AdminMenuPage");
        }
    }

    // Clase auxiliar para médicos con horarios
    public partial class MedicoConHorarios : ObservableObject
    {
        public int IdMedico { get; set; }
        public int IdDoctor { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string NombreEspecialidad { get; set; } = string.Empty;
        public string TituloProfesional { get; set; } = string.Empty;

        [ObservableProperty]
        private bool mostrarHorarios;

        public ObservableCollection<Horario> HorariosAsignados { get; set; } = new();

        // Propiedades calculadas
        public string NombreCompleto => $"{TituloProfesional} {Nombres} {Apellidos}".Trim();
        public string MedicoInfo => $"{NombreCompleto} - {NombreEspecialidad}";
    }
}