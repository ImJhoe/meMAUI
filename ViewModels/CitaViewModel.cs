// ViewModels/CitaViewModel.cs - CORREGIDO
using ClinicaApp.Models;
using ClinicaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ClinicaApp.ViewModels
{
    public partial class CitaViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public CitaViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Crear Cita";
            Medicos = new ObservableCollection<Medico>();
            HorariosDisponibles = new ObservableCollection<Horario>();
            FechaSeleccionada = DateTime.Today.AddDays(1);
        }

        [ObservableProperty]
        private string cedulaPaciente = string.Empty;

        [ObservableProperty]
        private Paciente? pacienteEncontrado;

        [ObservableProperty]
        private bool pacienteExiste;

        [ObservableProperty]
        private Medico? medicoSeleccionado;

        [ObservableProperty]
        private Horario? horarioSeleccionado;

        [ObservableProperty]
        private DateTime fechaSeleccionada;

        [ObservableProperty]
        private string motivoCita = string.Empty;

        [ObservableProperty]
        private bool mostrandoPaciente;

        public ObservableCollection<Medico> Medicos { get; }
        public ObservableCollection<Horario> HorariosDisponibles { get; }

        [RelayCommand]
        private async Task BuscarPacienteAsync()
        {
            if (string.IsNullOrWhiteSpace(CedulaPaciente))
            {
                ShowError("Ingrese el número de cédula");
                return;
            }

            try
            {
                ShowLoading(true);
                ClearError();

                var response = await _apiService.BuscarPacientePorCedulaAsync(CedulaPaciente.Trim());

                if (response.Success && response.Data != null)
                {
                    PacienteEncontrado = response.Data;
                    PacienteExiste = true;
                    MostrandoPaciente = true;

                    await CargarMedicosAsync();
                }
                else
                {
                    PacienteExiste = false;
                    MostrandoPaciente = true;
                    PacienteEncontrado = null;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error al buscar paciente: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        [RelayCommand]
        private async Task IrARegistroPacienteAsync()
        {
            // Navegar a la página de registro de paciente
            var parameters = new Dictionary<string, object>
            {
                ["Cedula"] = CedulaPaciente
            };

            await Shell.Current.GoToAsync("PacienteRegistroPage", parameters);
        }

        // Reemplazar solo el método CargarMedicosAsync en tu CitaViewModel.cs

        private async Task CargarMedicosAsync()
        {
            try
            {
                // ✅ CORREGIDO: ObtenerMedicosAsync devuelve List<Medico> directamente
                var medicos = await _apiService.ObtenerMedicosAsync();

                Medicos.Clear();
                foreach (var medico in medicos)
                {
                    Medicos.Add(medico);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error al cargar médicos: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task CargarHorariosDisponiblesAsync()
        {
            if (MedicoSeleccionado == null)
                return;

            try
            {
                ShowLoading(true);

                // Usar IdMedico en lugar de IdDoctor para consistencia
                var response = await _apiService.ObtenerHorariosDisponiblesAsync(
                    MedicoSeleccionado.IdMedico,
                    FechaSeleccionada);

                if (response.Success && response.Data != null)
                {
                    HorariosDisponibles.Clear();
                    foreach (var horario in response.Data)
                    {
                        HorariosDisponibles.Add(horario);
                    }
                }
                else
                {
                    HorariosDisponibles.Clear();
                    ShowError("No hay horarios disponibles para esta fecha");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error al cargar horarios: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        [RelayCommand]
        private async Task CrearCitaAsync()
        {
            if (!ValidarCita())
                return;

            try
            {
                ShowLoading(true);
                ClearError();

                // ✅ CORREGIDO: Construir FechaHora combinando fecha y hora
                DateTime fechaHoraCita = ConstruirFechaHora(FechaSeleccionada, HorarioSeleccionado!.HoraInicio);

                var cita = new Cita
                {
                    IdPaciente = PacienteEncontrado!.IdPaciente,
                    IdDoctor = MedicoSeleccionado!.IdMedico, // Usar IdMedico
                    IdSucursal = HorarioSeleccionado!.IdSucursal, // Usar sucursal del horario
                    FechaHora = fechaHoraCita, // ✅ CORREGIDO: Usar fecha construida
                    Motivo = MotivoCita.Trim(),
                    TipoCita = "presencial",
                    Estado = "Pendiente"
                };

                var response = await _apiService.CrearCitaAsync(cita);

                if (response.Success)
                {
                    await Shell.Current.DisplayAlert("Éxito", "Cita creada correctamente", "OK");
                    LimpiarFormulario();
                }
                else
                {
                    ShowError(response.Message ?? "Error al crear la cita");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        // ✅ MÉTODO NUEVO: Construir DateTime desde fecha y hora string
        private DateTime ConstruirFechaHora(DateTime fecha, string horaString)
        {
            try
            {
                // Parsear la hora desde string "HH:mm"
                if (TimeSpan.TryParse(horaString, out TimeSpan hora))
                {
                    return fecha.Date.Add(hora);
                }
                else
                {
                    // Si no se puede parsear, usar hora por defecto
                    return fecha.Date.AddHours(8); // 8:00 AM por defecto
                }
            }
            catch
            {
                // En caso de error, devolver fecha con hora por defecto
                return fecha.Date.AddHours(8);
            }
        }

        private bool ValidarCita()
        {
            if (PacienteEncontrado == null)
            {
                ShowError("Debe seleccionar un paciente");
                return false;
            }

            if (MedicoSeleccionado == null)
            {
                ShowError("Debe seleccionar un médico");
                return false;
            }

            if (HorarioSeleccionado == null)
            {
                ShowError("Debe seleccionar un horario");
                return false;
            }

            if (string.IsNullOrWhiteSpace(MotivoCita))
            {
                ShowError("Debe indicar el motivo de la cita");
                return false;
            }

            return true;
        }

        private void LimpiarFormulario()
        {
            CedulaPaciente = string.Empty;
            PacienteEncontrado = null;
            PacienteExiste = false;
            MostrandoPaciente = false;
            MedicoSeleccionado = null;
            HorarioSeleccionado = null;
            MotivoCita = string.Empty;
            FechaSeleccionada = DateTime.Today.AddDays(1);

            Medicos.Clear();
            HorariosDisponibles.Clear();
            ClearError();
        }

        // Property Changed para actualizar horarios cuando cambia la fecha o médico
        partial void OnFechaSeleccionadaChanged(DateTime value)
        {
            if (MedicoSeleccionado != null)
            {
                _ = CargarHorariosDisponiblesAsync();
            }
        }

        partial void OnMedicoSeleccionadoChanged(Medico? value)
        {
            if (value != null)
            {
                _ = CargarHorariosDisponiblesAsync();
            }
        }
    }
}