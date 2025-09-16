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
        public HorariosMedicosViewModel() : this(new ApiService(new HttpClient()))
        {
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
                            // ✅ CORREGIDO: Acceso seguro a especialidad
                            Especialidad = medico.Especialidad?.NombreEspecialidad ?? "Especialidad médica",
                            Horario = "Clic para ver horarios detallados",
                            Disponible = true,
                            CedulaMedico = medico.Cedula
                        };

                        Medicos.Add(medicoViewModel);
                    }

                    System.Diagnostics.Debug.WriteLine($"[HORARIOS] Médicos cargados: {Medicos.Count}");
                    MensajeInfo = $"Se encontraron {Medicos.Count} médicos disponibles";
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[HORARIOS] No se obtuvieron médicos del API");
                    await CargarDatosEjemploAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HORARIOS] Error cargando médicos: {ex.Message}");
                ShowError($"Error al cargar médicos: {ex.Message}");
                await CargarDatosEjemploAsync();
            }
            finally
            {
                ShowLoading(false);
            }
        }

        [RelayCommand]
        private async Task VerHorariosDisponibles()
        {
            try
            {
                ShowLoading(true);
                System.Diagnostics.Debug.WriteLine($"[HORARIOS] Viendo horarios para fecha: {FechaSeleccionada:yyyy-MM-dd}");

                HorariosDisponibles.Clear();
                MostrandoHorarios = true;

                // Obtener horarios disponibles para todos los médicos en la fecha seleccionada
                var fechaParam = FechaSeleccionada.ToString("yyyy-MM-dd");

                foreach (var medico in Medicos)
                {
                    await CargarHorariosParaMedicoAsync(medico, fechaParam);
                }

                if (HorariosDisponibles.Any())
                {
                    MensajeInfo = $"Se encontraron {HorariosDisponibles.Count} horarios disponibles para el {FechaSeleccionada:dd/MM/yyyy}";
                }
                else
                {
                    MensajeInfo = $"No hay horarios disponibles para el {FechaSeleccionada:dd/MM/yyyy}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HORARIOS] Error: {ex.Message}");
                ShowError($"Error al cargar horarios: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async Task CargarHorariosParaMedicoAsync(MedicoHorarioViewModel medico, string fecha)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[HORARIOS] Cargando horarios para médico {medico.IdMedico} en fecha {fecha}");

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
                    }

                    // Actualizar info del médico con sus horarios
                    var totalHorarios = horariosDisponiblesDelMedico.Count;
                    medico.Horario = totalHorarios > 0 ? $"{totalHorarios} horarios disponibles" : "Sin horarios disponibles";
                    medico.Disponible = totalHorarios > 0;

                    System.Diagnostics.Debug.WriteLine($"[HORARIOS] Médico {medico.Nombre}: {totalHorarios} horarios disponibles");
                }
                else
                {
                    medico.Horario = "Sin horarios disponibles";
                    medico.Disponible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HORARIOS] Error cargando horarios médico {medico.IdMedico}: {ex.Message}");
                medico.Horario = "Error al cargar horarios";
                medico.Disponible = false;
            }
        }

        [RelayCommand]
        private async Task VerHorariosDetallados(MedicoHorarioViewModel medico)
        {
            if (medico == null) return;

            try
            {
                ShowLoading(true);
                System.Diagnostics.Debug.WriteLine($"[HORARIOS] Viendo horarios detallados del médico {medico.IdMedico}");

                // ✅ CORREGIDO: Usar el método existente del ApiService
                var horariosResponse = await _apiService.ObtenerHorariosDisponiblesAsync(medico.IdMedico, FechaSeleccionada);

                if (horariosResponse.Success && horariosResponse.Data != null && horariosResponse.Data.Any())
                {
                    var horarios = horariosResponse.Data.Where(h => h.Disponible).ToList();

                    if (horarios.Any())
                    {
                        var mensaje = $"HORARIOS DISPONIBLES\n" +
                                     $"{medico.Nombre}\n" +
                                     $"{medico.Especialidad}\n" +
                                     $"Fecha: {FechaSeleccionada:dd/MM/yyyy}\n\n" +
                                     $"Horarios disponibles:\n";

                        foreach (var horario in horarios.Take(10)) // Mostrar máximo 10 horarios
                        {
                            var hora = DateTime.Parse(horario.FechaHora).ToString("HH:mm");
                            mensaje += $"• {hora}\n";
                        }

                        if (horarios.Count > 10)
                        {
                            mensaje += $"\n... y {horarios.Count - 10} horarios más";
                        }

                        mensaje += $"\n\nTotal: {horarios.Count} horarios disponibles";

                        await Shell.Current.DisplayAlert("Horarios Detallados", mensaje, "OK");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Sin Horarios",
                            $"El Dr(a). {medico.Nombre} no tiene horarios disponibles para el {FechaSeleccionada:dd/MM/yyyy}", "OK");
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Sin Horarios",
                        $"No se encontraron horarios para el Dr(a). {medico.Nombre} en la fecha seleccionada", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HORARIOS] Error horarios detallados: {ex.Message}");
                ShowError($"Error al cargar horarios detallados: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        [RelayCommand]
        private async Task AgendarCita(HorarioDisponibleViewModel horario)
        {
            if (horario == null) return;

            var confirmar = await Shell.Current.DisplayAlert(
                "Agendar Cita",
                $"¿Desea agendar una cita con {horario.NombreMedico} el {horario.Fecha:dd/MM/yyyy} a las {horario.Hora}?",
                "Sí", "No");

            if (confirmar)
            {
                // Navegar a la página de creación de citas con datos pre-cargados
                var parametros = new Dictionary<string, object>
                {
                    ["MedicoId"] = horario.IdMedico,
                    ["FechaHora"] = horario.FechaHora,
                    ["SucursalId"] = horario.IdSucursal
                };

                await Shell.Current.GoToAsync("//CitaCreacionPage", parametros);
            }
        }

        private async Task CargarDatosEjemploAsync()
        {
            // Datos de ejemplo cuando falla la API
            Medicos.Clear();

            var medicosEjemplo = new List<MedicoHorarioViewModel>
            {
                new MedicoHorarioViewModel
                {
                    IdMedico = 1,
                    Nombre = "Dr. García Ramírez",
                    Especialidad = "Cardiología",
                    Horario = "Lun-Vie 9:00-17:00",
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