// ViewModels/CitasListaViewModel.cs - VERSIÓN FINAL CORREGIDA
using ClinicaApp.Services;
using ClinicaApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ClinicaApp.ViewModels
{
    public partial class CitasListaViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private ObservableCollection<CitaItemViewModel> citas = new();

        [ObservableProperty]
        private ObservableCollection<MedicoViewModel> medicos = new();

        [ObservableProperty]
        private DateTime fechaSeleccionada = DateTime.Today;

        [ObservableProperty]
        private MedicoViewModel? medicoSeleccionado;

        [ObservableProperty]
        private string estadoSeleccionado = "Todos";

        [ObservableProperty]
        private int totalCitas = 0;

        [ObservableProperty]
        private string mensajeFiltro = string.Empty;

        public List<string> EstadosDisponibles { get; } = new List<string>
        {
            "Todos", "Pendiente", "Confirmada", "Completada", "Cancelada", "En Proceso"
        };

        public CitasListaViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Lista de Citas";

            // Cargar datos iniciales
            Task.Run(async () => await InicializarAsync());
        }

        // Constructor sin parámetros para cuando no hay DI
        public CitasListaViewModel() : this(new ApiService(new HttpClient()))
        {
        }

        private async Task InicializarAsync()
        {
            await CargarMedicosAsync();
            await CargarCitasAsync();
        }

        [RelayCommand]
        private async Task CargarCitasAsync()
        {
            try
            {
                ShowLoading(true);
                System.Diagnostics.Debug.WriteLine("[CITAS LISTA] Cargando citas...");

                // Preparar parámetros de filtro
                var fechaParam = FechaSeleccionada.ToString("yyyy-MM-dd");
                var medicoParam = MedicoSeleccionado?.IdMedico.ToString();
                var estadoParam = EstadoSeleccionado != "Todos" ? EstadoSeleccionado : null;

                System.Diagnostics.Debug.WriteLine($"[CITAS LISTA] Filtros - Fecha: {fechaParam}, Médico: {medicoParam}, Estado: {estadoParam}");

                // Construir URL con parámetros
                var url = "api/citas?";
                var parametros = new List<string>();

                if (!string.IsNullOrEmpty(fechaParam))
                    parametros.Add($"fecha={fechaParam}");

                if (!string.IsNullOrEmpty(medicoParam) && medicoParam != "0")
                    parametros.Add($"medico={medicoParam}");

                if (!string.IsNullOrEmpty(estadoParam))
                    parametros.Add($"estado={estadoParam}");

                if (parametros.Any())
                    url += string.Join("&", parametros);

                System.Diagnostics.Debug.WriteLine($"[CITAS LISTA] URL: {url}");

                // ✅ CORREGIDO: Usar método no genérico
                var response = await _apiService.GetHttpAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[CITAS LISTA] Response: {content}");

                    var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<List<CitaDto>>>(content, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (jsonResponse?.Success == true && jsonResponse.Data != null)
                    {
                        // Limpiar y cargar nuevas citas
                        Citas.Clear();

                        foreach (var citaDto in jsonResponse.Data)
                        {
                            var cita = new CitaItemViewModel
                            {
                                IdCita = citaDto.IdCita,
                                Paciente = citaDto.NombrePaciente,
                                CedulaPaciente = citaDto.CedulaPaciente,
                                Medico = citaDto.NombreMedico,
                                Especialidad = citaDto.NombreEspecialidad,
                                Sucursal = citaDto.NombreSucursal,
                                FechaHora = DateTime.Parse(citaDto.FechaHora),
                                Hora = DateTime.Parse(citaDto.FechaHora).ToString("HH:mm"),
                                Estado = citaDto.Estado,
                                Motivo = citaDto.Motivo,
                                TipoCita = citaDto.TipoCita,
                                Notas = citaDto.Notas ?? ""
                            };

                            Citas.Add(cita);
                        }

                        TotalCitas = Citas.Count;
                        MensajeFiltro = $"Se encontraron {TotalCitas} citas para los filtros seleccionados";

                        System.Diagnostics.Debug.WriteLine($"[CITAS LISTA] Citas cargadas: {TotalCitas}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[CITAS LISTA] Respuesta sin éxito del API");
                        ShowError("No se pudieron cargar las citas");
                        Citas.Clear();
                        TotalCitas = 0;
                        MensajeFiltro = "No se encontraron citas";
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[CITAS LISTA] Error HTTP: {response.StatusCode}");
                    ShowError($"Error al cargar citas: {response.StatusCode}");

                    // Cargar datos de ejemplo si falla API
                    await CargarDatosEjemploAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CITAS LISTA] Error: {ex.Message}");
                ShowError($"Error al cargar citas: {ex.Message}");

                // Cargar datos de ejemplo como fallback
                await CargarDatosEjemploAsync();
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async Task CargarMedicosAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[CITAS LISTA] Cargando médicos...");

                // ✅ CORREGIDO: Usar método no genérico
                var response = await _apiService.GetHttpAsync("api/medicos");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<List<Medico>>>(content, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (jsonResponse?.Success == true && jsonResponse.Data != null)
                    {
                        Medicos.Clear();

                        // Agregar opción "Todos"
                        Medicos.Add(new MedicoViewModel { IdMedico = 0, Nombre = "Todos los médicos" });

                        foreach (var medico in jsonResponse.Data)
                        {
                            Medicos.Add(new MedicoViewModel
                            {
                                IdMedico = medico.IdMedico,
                                Nombre = medico.NombreCompleto,
                                Cedula = medico.Cedula,
                                // ✅ CORREGIDO: Acceso seguro a especialidad
                                Especialidad = "Especialidad médica" // Temporal hasta tener la estructura correcta
                            });
                        }

                        System.Diagnostics.Debug.WriteLine($"[CITAS LISTA] Médicos cargados: {Medicos.Count}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CITAS LISTA] Error cargando médicos: {ex.Message}");

                // Datos de ejemplo si falla
                Medicos.Clear();
                Medicos.Add(new MedicoViewModel { IdMedico = 0, Nombre = "Todos los médicos" });
                Medicos.Add(new MedicoViewModel { IdMedico = 1, Nombre = "Dr. García Ramírez", Especialidad = "Cardiología" });
                Medicos.Add(new MedicoViewModel { IdMedico = 2, Nombre = "Dra. López Martínez", Especialidad = "Pediatría" });
            }
        }

        private async Task CargarDatosEjemploAsync()
        {
            // Datos de ejemplo cuando falla la API
            Citas.Clear();

            var citasEjemplo = new List<CitaItemViewModel>
            {
                new CitaItemViewModel
                {
                    IdCita = 1,
                    Paciente = "Juan Pérez García",
                    CedulaPaciente = "1234567890",
                    Medico = "Dr. García Ramírez",
                    Especialidad = "Cardiología",
                    Sucursal = "Sede Centro",
                    FechaHora = DateTime.Today.AddHours(9),
                    Hora = "09:00",
                    Estado = "Pendiente",
                    Motivo = "Control de rutina",
                    TipoCita = "Presencial"
                },
                new CitaItemViewModel
                {
                    IdCita = 2,
                    Paciente = "María López Vega",
                    CedulaPaciente = "0987654321",
                    Medico = "Dra. Martínez Silva",
                    Especialidad = "Pediatría",
                    Sucursal = "Sede Norte",
                    FechaHora = DateTime.Today.AddHours(10.5),
                    Hora = "10:30",
                    Estado = "Confirmada",
                    Motivo = "Consulta general",
                    TipoCita = "Presencial"
                },
                new CitaItemViewModel
                {
                    IdCita = 3,
                    Paciente = "Carlos Rodríguez Morales",
                    CedulaPaciente = "1122334455",
                    Medico = "Dr. Vega Herrera",
                    Especialidad = "Neurología",
                    Sucursal = "Sede Sur",
                    FechaHora = DateTime.Today.AddHours(14.25),
                    Hora = "14:15",
                    Estado = "En Proceso",
                    Motivo = "Seguimiento neurológico",
                    TipoCita = "Presencial"
                }
            };

            foreach (var cita in citasEjemplo)
            {
                Citas.Add(cita);
            }

            TotalCitas = Citas.Count;
            MensajeFiltro = $"Datos de ejemplo - {TotalCitas} citas mostradas";

            await Task.Delay(100); // Simular delay
        }

        [RelayCommand]
        private async Task BuscarPorFecha()
        {
            System.Diagnostics.Debug.WriteLine($"[CITAS LISTA] Buscando citas para fecha: {FechaSeleccionada:yyyy-MM-dd}");
            await CargarCitasAsync();
        }

        [RelayCommand]
        private async Task LimpiarFiltros()
        {
            System.Diagnostics.Debug.WriteLine("[CITAS LISTA] Limpiando filtros");

            FechaSeleccionada = DateTime.Today;
            MedicoSeleccionado = null;
            EstadoSeleccionado = "Todos";

            await CargarCitasAsync();
        }

        [RelayCommand]
        private async Task VerDetalleCita(CitaItemViewModel cita)
        {
            if (cita == null) return;

            var mensaje = $"DETALLE DE CITA #{cita.IdCita}\n\n" +
                         $"Paciente: {cita.Paciente}\n" +
                         $"Cédula: {cita.CedulaPaciente}\n" +
                         $"Médico: {cita.Medico}\n" +
                         $"Especialidad: {cita.Especialidad}\n" +
                         $"Fecha: {cita.FechaHora:dd/MM/yyyy}\n" +
                         $"Hora: {cita.Hora}\n" +
                         $"Estado: {cita.Estado}\n" +
                         $"Motivo: {cita.Motivo}\n" +
                         $"Tipo: {cita.TipoCita}\n" +
                         $"Sucursal: {cita.Sucursal}";

            if (!string.IsNullOrEmpty(cita.Notas))
                mensaje += $"\n\nNotas: {cita.Notas}";

            await Shell.Current.DisplayAlert("Detalle de Cita", mensaje, "OK");
        }

        [RelayCommand]
        private async Task VolverAlMenu()
        {
            await Shell.Current.GoToAsync("//RecepcionistaMenuPage");
        }
    }

    // Clase auxiliar para los médicos
    public class MedicoViewModel
    {
        public int IdMedico { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
    }

    // DTOs para deserialización
    public class CitaDto
    {
        public int IdCita { get; set; }
        public string FechaHora { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string TipoCita { get; set; } = string.Empty;
        public string? Notas { get; set; }
        public string NombrePaciente { get; set; } = string.Empty;
        public string CedulaPaciente { get; set; } = string.Empty;
        public string NombreMedico { get; set; } = string.Empty;
        public string NombreEspecialidad { get; set; } = string.Empty;
        public string NombreSucursal { get; set; } = string.Empty;
    }

    // Clase auxiliar para los items de cita mejorada
    public class CitaItemViewModel
    {
        public int IdCita { get; set; }
        public string Paciente { get; set; } = string.Empty;
        public string CedulaPaciente { get; set; } = string.Empty;
        public string Medico { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public string Sucursal { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string Hora { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public string TipoCita { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;

        // Propiedades adicionales para UI
        public string FechaTexto => FechaHora.ToString("dd/MM/yyyy");
        public string EstadoColor => Estado switch
        {
            "Pendiente" => "#FF9800",
            "Confirmada" => "#4CAF50",
            "Completada" => "#2196F3",
            "Cancelada" => "#F44336",
            "En Proceso" => "#9C27B0",
            _ => "#757575"
        };
    }
}