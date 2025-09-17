// ViewModels/HorariosMedicosViewModel.cs - VERSIÓN CORREGIDA
using ClinicaApp.Services;
using ClinicaApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ClinicaApp.ViewModels
{
    public partial class HorariosMedicosViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private ObservableCollection<MedicoHorarioViewModel> medicos = new();

        [ObservableProperty]
        private ObservableCollection<HorarioDisponibleViewModel> horariosDisponibles = new();

        [ObservableProperty]
        private DateTime fechaSeleccionada = DateTime.Today.AddDays(1); // Mañana por defecto

        [ObservableProperty]
        private MedicoHorarioViewModel? medicoSeleccionado;

        [ObservableProperty]
        private bool mostrandoHorarios = false;

        [ObservableProperty]
        private string mensajeInfo = string.Empty;

        public HorariosMedicosViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Horarios Médicos";
            MensajeInfo = "Seleccione una fecha para ver horarios disponibles";

            // Cargar médicos al inicializar
            Task.Run(async () => await CargarMedicosAsync());
        }

        // Constructor sin parámetros para cuando no hay DI
        public HorariosMedicosViewModel()
        {
            // ✅ CORREGIDO: Crear HttpClient con configuración correcta
            var httpClient = new HttpClient();
            // Cambiar línea aproximada 56:
            httpClient.BaseAddress = new Uri("http://192.168.1.14:8081/webservice-slim/");
            httpClient.Timeout = TimeSpan.FromSeconds(60);

            _apiService = new ApiService(httpClient);
            Title = "Horarios Médicos";
            MensajeInfo = "Seleccione una fecha para ver horarios disponibles";

            // Inicializar colecciones
            Medicos = new ObservableCollection<MedicoHorarioViewModel>();
            HorariosDisponibles = new ObservableCollection<HorarioDisponibleViewModel>();

            // Cargar médicos al inicializar
            Task.Run(async () => await CargarMedicosAsync());
        }

        [RelayCommand]
        private async Task CargarMedicosAsync()
        {
            try
            {
                ShowLoading(true);
                System.Diagnostics.Debug.WriteLine("[HORARIOS] Cargando médicos...");

                // ✅ CORREGIDO: Usar el método existente ObtenerMedicosAsync que devuelve List<Medico>
                var medicosFromApi = await _apiService.ObtenerMedicosAsync();

                if (medicosFromApi != null && medicosFromApi.Any())
                {
                    Medicos.Clear();

                    foreach (var medico in medicosFromApi)
                    {
                        var medicoViewModel = new MedicoHorarioViewModel
                        {
                            IdMedico = medico.IdMedico,
                            Nombre = medico.NombreCompleto,
                            // ✅ CORREGIDO: Usar NombreEspecialidad en lugar de Especialidad
                            Especialidad = medico.NombreEspecialidad ?? "Sin especialidad",
                            Horario = "Cargando horarios...",
                            Disponible = true,
                            CedulaMedico = medico.Cedula
                        };

                        Medicos.Add(medicoViewModel);
                    }

                    MensajeInfo = $"Se cargaron {Medicos.Count} médicos exitosamente";
                    System.Diagnostics.Debug.WriteLine($"[HORARIOS] ✅ {Medicos.Count} médicos cargados");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[HORARIOS] No se encontraron médicos, cargando datos de ejemplo");
                    await CargarMedicosEjemplo();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HORARIOS] ❌ Error cargando médicos: {ex.Message}");
                ShowError($"Error al cargar médicos: {ex.Message}");

                // En caso de error, cargar datos de ejemplo
                await CargarMedicosEjemplo();
            }
            finally
            {
                ShowLoading(false);
            }
        }

        [RelayCommand]
        private async Task BuscarHorariosDisponibles()
        {
            try
            {
                ShowLoading(true);
                HorariosDisponibles.Clear();

                System.Diagnostics.Debug.WriteLine($"[HORARIOS] 🔍 Buscando horarios para {FechaSeleccionada:dd/MM/yyyy}");

                if (!Medicos.Any())
                {
                    ShowError("No hay médicos disponibles");
                    return;
                }

                var fecha = FechaSeleccionada.ToString("yyyy-MM-dd");
                var horariosEncontrados = 0;

                foreach (var medico in Medicos)
                {
                    try
                    {
                        // ✅ CORREGIDO: Usar el método existente ObtenerHorariosDisponiblesAsync
                        var horariosResponse = await _apiService.ObtenerHorariosDisponiblesAsync(medico.IdMedico, DateTime.Parse(fecha));

                        if (horariosResponse.Success && horariosResponse.Data != null)
                        {
                            var horariosDisponiblesDelMedico = horariosResponse.Data.Where(h => h.Disponible).ToList();

                            foreach (var horario in horariosDisponiblesDelMedico)
                            {
                                var horarioViewModel = new HorarioDisponibleViewModel
                                {
                                    IdMedico = medico.IdMedico,
                                    NombreMedico = medico.Nombre,
                                    EspecialidadMedico = medico.Especialidad,
                                    Fecha = DateTime.Parse(horario.FechaHora),
                                    Hora = DateTime.Parse(horario.FechaHora).ToString("HH:mm"),
                                    FechaHora = horario.FechaHora,
                                    Disponible = horario.Disponible,
                                    IdSucursal = horario.IdSucursal
                                };

                                HorariosDisponibles.Add(horarioViewModel);
                                horariosEncontrados++;
                            }

                            // Actualizar info del médico con sus horarios
                            var totalHorarios = horariosDisponiblesDelMedico.Count;
                            medico.Horario = totalHorarios > 0 ?
                                $"{totalHorarios} horarios disponibles" :
                                "Sin horarios disponibles";
                            medico.Disponible = totalHorarios > 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[HORARIOS] Error obteniendo horarios de médico {medico.IdMedico}: {ex.Message}");
                        medico.Horario = "Error cargando horarios";
                        medico.Disponible = false;
                    }
                }

                MostrandoHorarios = true;

                if (horariosEncontrados > 0)
                {
                    MensajeInfo = $"Se encontraron {horariosEncontrados} horarios disponibles para el {FechaSeleccionada:dd/MM/yyyy}";
                    System.Diagnostics.Debug.WriteLine($"[HORARIOS] ✅ {horariosEncontrados} horarios encontrados");
                }
                else
                {
                    MensajeInfo = $"No se encontraron horarios disponibles para el {FechaSeleccionada:dd/MM/yyyy}";
                    System.Diagnostics.Debug.WriteLine("[HORARIOS] ℹ️ No hay horarios disponibles");
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HORARIOS] ❌ Error general: {ex.Message}");
                ShowError($"Error al buscar horarios: {ex.Message}");
                MensajeInfo = "Error al buscar horarios disponibles";
            }
            finally
            {
                ShowLoading(false);
            }
        }

        [RelayCommand]
        private async Task OcultarHorarios()
        {
            MostrandoHorarios = false;
            HorariosDisponibles.Clear();
            MensajeInfo = "Seleccione una fecha para ver horarios disponibles";
        }

        [RelayCommand]
        private async Task SeleccionarHorario(HorarioDisponibleViewModel horario)
        {
            try
            {
                var mensaje = $"¿Desea programar una cita con {horario.NombreMedico} ({horario.EspecialidadMedico}) el {horario.HorarioTexto}?";

                var confirmar = await Shell.Current.DisplayAlert("Confirmar Cita", mensaje, "Sí", "No");

                if (confirmar)
                {
                    System.Diagnostics.Debug.WriteLine($"[HORARIOS] Horario seleccionado: {horario.NombreMedico} - {horario.HorarioTexto}");

                    // Navegar a creación de cita con datos pre-cargados
                    var navigationParameter = new ShellNavigationQueryParameters
                    {
                        { "IdMedico", horario.IdMedico.ToString() },
                        { "FechaHora", horario.FechaHora },
                        { "NombreMedico", horario.NombreMedico }
                    };

                    await Shell.Current.GoToAsync("//CitaCreacionPage", navigationParameter);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HORARIOS] Error al seleccionar horario: {ex.Message}");
                ShowError("Error al seleccionar horario");
            }
        }

        // Método auxiliar para cargar datos de ejemplo cuando no hay conexión API
        private async Task CargarMedicosEjemplo()
        {
            var medicosEjemplo = new List<MedicoHorarioViewModel>
            {
                new MedicoHorarioViewModel
                {
                    IdMedico = 1,
                    Nombre = "Dr. García Pérez",
                    Especialidad = "Cardiología",
                    Horario = "Lun-Vie 8:00-16:00",
                    Disponible = true,
                    CedulaMedico = "1234567890"
                },
                new MedicoHorarioViewModel
                {
                    IdMedico = 2,
                    Nombre = "Dra. López Vega",
                    Especialidad = "Pediatría",
                    Horario = "Mar-Sáb 8:00-16:00",
                    Disponible = true,
                    CedulaMedico = "0987654321"
                },
                new MedicoHorarioViewModel
                {
                    IdMedico = 3,
                    Nombre = "Dr. Martínez Silva",
                    Especialidad = "Neurología",
                    Horario = "Lun-Mie-Vie 10:00-18:00",
                    Disponible = false,
                    CedulaMedico = "1122334455"
                },
                new MedicoHorarioViewModel
                {
                    IdMedico = 4,
                    Nombre = "Dra. Rodríguez Morales",
                    Especialidad = "Dermatología",
                    Horario = "Lun-Jue 14:00-20:00",
                    Disponible = true,
                    CedulaMedico = "5566778899"
                }
            };

            foreach (var medico in medicosEjemplo)
            {
                Medicos.Add(medico);
            }

            MensajeInfo = $"Datos de ejemplo - {Medicos.Count} médicos disponibles";
            await Task.Delay(100);
        }

        [RelayCommand]
        private async Task VolverAlMenu()
        {
            await Shell.Current.GoToAsync("//RecepcionistaMenuPage");
        }
    }

    // Clase auxiliar para los items de médico con horarios
    public class MedicoHorarioViewModel
    {
        public int IdMedico { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public string Horario { get; set; } = string.Empty;
        public bool Disponible { get; set; }
        public string CedulaMedico { get; set; } = string.Empty;

        // Propiedades para UI
        public string EstadoTexto => Disponible ? "Disponible" : "No disponible";
        public string EstadoColor => Disponible ? "#4CAF50" : "#F44336";
        public string IconoDisponibilidad => Disponible ? "✅" : "❌";
    }

    // Clase auxiliar para horarios disponibles
    public class HorarioDisponibleViewModel
    {
        public int IdMedico { get; set; }
        public string NombreMedico { get; set; } = string.Empty;
        public string EspecialidadMedico { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Hora { get; set; } = string.Empty;
        public string FechaHora { get; set; } = string.Empty;
        public bool Disponible { get; set; }
        public int IdSucursal { get; set; }

        // Propiedades para UI
        public string FechaTexto => Fecha.ToString("dd/MM/yyyy");
        public string HorarioTexto => $"{FechaTexto} - {Hora}";
        public string InfoCompleta => $"{NombreMedico} - {EspecialidadMedico}";
    }
}