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

        private async Task CargarMedicosAsync()
        {
            try
            {
                var response = await _apiService.ObtenerMedicosAsync();

                if (response.Success && response.Data != null)
                {
                    Medicos.Clear();
                    foreach (var medico in response.Data)
                    {
                        Medicos.Add(medico);
                    }
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

                var cita = new Cita
                {
                    IdPaciente = PacienteEncontrado!.IdPaciente,
                    IdDoctor = MedicoSeleccionado!.IdMedico,
                    IdSucursal = 1, // Por defecto la primera sucursal
                    FechaHora = HorarioSeleccionado!.FechaHora ?? DateTime.Now,
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