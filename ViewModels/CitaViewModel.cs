// ViewModels/CitaViewModel.cs - CORREGIDO PARA TU APISERVICE
using ClinicaApp.Models;
using ClinicaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ClinicaApp.ViewModels
{
    [QueryProperty(nameof(CedulaRegistrada), "cedulaRegistrada")]
    public partial class CitaViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public CitaViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "PUNTO 3: Crear Cita";
            Medicos = new ObservableCollection<Medico>();
            HorariosDisponibles = new ObservableCollection<HorarioDisponible>();
            FechaSeleccionada = DateTime.Today.AddDays(1);

            System.Diagnostics.Debug.WriteLine("[CITA VM] 🎯 PUNTO 3: ViewModel inicializado");

            // Cargar datos iniciales
            _ = CargarDatosInicialesAsync();
        }
        [ObservableProperty]
        private string cedulaRegistrada = string.Empty;

        [ObservableProperty]
        private string cedulaPaciente = string.Empty;

        [ObservableProperty]
        private Paciente? pacienteEncontrado;

        [ObservableProperty]
        private bool pacienteExiste;

        [ObservableProperty]
        private Medico? medicoSeleccionado;

        [ObservableProperty]
        private HorarioDisponible? horarioSeleccionado;

        [ObservableProperty]
        private DateTime fechaSeleccionada;

        [ObservableProperty]
        private string motivoCita = string.Empty;

        [ObservableProperty]
        private bool mostrandoPaciente;
       

        public ObservableCollection<Medico> Medicos { get; }
        public ObservableCollection<HorarioDisponible> HorariosDisponibles { get; private set; }

        // COMANDO PRINCIPAL - PUNTO 3: Búsqueda por cédula
        [RelayCommand]
        private async Task BuscarPacienteAsync()
        {
            if (string.IsNullOrWhiteSpace(CedulaPaciente))
            {
                System.Diagnostics.Debug.WriteLine("[CITA] ❌ Cédula vacía");
                ShowError("Ingrese el número de cédula del paciente");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"[CITA] 🔍 PUNTO 3: Buscando paciente con cédula: {CedulaPaciente.Trim()}");

                ShowLoading(true);
                ClearError();

                // ✅ CORREGIDO: Tu ApiService.BuscarPacientePorCedulaAsync devuelve ApiResponse<Paciente>
                var response = await _apiService.BuscarPacientePorCedulaAsync(CedulaPaciente.Trim());

                System.Diagnostics.Debug.WriteLine($"[CITA] 📡 Respuesta API: Success={response.Success}, Message={response.Message}");

                if (response.Success && response.Data != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[CITA] ✅ PACIENTE ENCONTRADO: {response.Data.NombreCompleto}");

                    PacienteEncontrado = response.Data;
                    PacienteExiste = true;
                    MostrandoPaciente = true;

                    // Cargar médicos disponibles
                    await CargarMedicosAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[CITA] 🔍 PUNTO 4: PACIENTE NO ENCONTRADO - Mostrando opción 'Añadir paciente'");

                    PacienteExiste = false;
                    MostrandoPaciente = true;
                    PacienteEncontrado = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CITA] ❌ Error búsqueda: {ex.Message}");
                ShowError($"Error al buscar paciente: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        // PUNTO 4: Ir a registro de paciente cuando no existe
        [RelayCommand]
        private async Task IrARegistroPacienteAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[CITA] 👤 PUNTO 4 → 5: Navegando a registro de paciente");

                var parametros = new Dictionary<string, object>
                {
                    ["cedula"] = CedulaPaciente.Trim()
                };

                // ✅ CORREGIDO: Usar ruta absoluta con ///
                await Shell.Current.GoToAsync("///PacienteRegistroPage", parametros);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CITA] ❌ Error navegación registro: {ex.Message}");
                ShowError("Error al navegar al registro de pacientes");
            }
        }

        // ✅ CORREGIDO: Tu ApiService.ObtenerMedicosAsync() devuelve List<Medico> directamente
        private async Task CargarMedicosAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[CITA] 📋 Cargando médicos...");

                // ✅ CORREGIDO: ObtenerMedicosAsync devuelve List<Medico> directamente, no ApiResponse
                var medicosLista = await _apiService.ObtenerMedicosAsync();

                if (medicosLista != null && medicosLista.Any())
                {
                    Medicos.Clear();
                    foreach (var medico in medicosLista)
                    {
                        Medicos.Add(medico);
                    }

                    System.Diagnostics.Debug.WriteLine($"[CITA] ✅ {Medicos.Count} médicos cargados");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[CITA] ❌ No se encontraron médicos");
                    ShowError("No se encontraron médicos disponibles");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CITA] ❌ Excepción cargando médicos: {ex.Message}");
                ShowError($"Error al cargar médicos: {ex.Message}");
            }
        }

        // Actualizar el método CargarHorariosDisponiblesAsync
        private async Task CargarHorariosDisponiblesAsync()
        {
            if (MedicoSeleccionado == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[CITA] 🗓️ PUNTO 7: Cargando horarios para médico ID: {MedicoSeleccionado.IdMedico}");
                System.Diagnostics.Debug.WriteLine($"[CITA] 📅 Fecha seleccionada: {FechaSeleccionada:yyyy-MM-dd}");

                ShowLoading(true);

                // ✅ PUNTO 7: API call para obtener horarios disponibles
                var response = await _apiService.ObtenerHorariosDisponiblesAsync(
                    MedicoSeleccionado.IdMedico,
                    FechaSeleccionada);

                if (response.Success && response.Data != null && response.Data.Any())
                {
                    HorariosDisponibles.Clear();
                    foreach (var horario in response.Data)
                    {
                        HorariosDisponibles.Add(horario);
                    }

                    System.Diagnostics.Debug.WriteLine($"[CITA] ✅ PUNTO 7: {HorariosDisponibles.Count} horarios disponibles cargados");
                }
                else
                {
                    HorariosDisponibles.Clear();
                    System.Diagnostics.Debug.WriteLine("[CITA] ⚠️ PUNTO 7: No hay horarios disponibles para esta fecha");
                    ShowError("No hay horarios disponibles para esta fecha y médico seleccionado");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CITA] ❌ PUNTO 7 Error: {ex.Message}");
                ShowError($"Error al cargar horarios: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }


        // Crear la cita
        [RelayCommand]
        // Actualizar el método CrearCitaAsync para usar el nuevo modelo
        private async Task CrearCitaAsync()
        {
            if (!ValidarCita())
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine("[CITA] 💾 PUNTO 3-7: Creando cita médica...");

                ShowLoading(true);
                ClearError();

                // Construir fecha y hora de la cita usando HorarioDisponible
                DateTime fechaHoraCita = HorarioSeleccionado!.FechaHoraCompleta;

                System.Diagnostics.Debug.WriteLine($"[CITA] 📊 Datos de la cita:");
                System.Diagnostics.Debug.WriteLine($"  - Paciente ID: {PacienteEncontrado!.IdPaciente}");
                System.Diagnostics.Debug.WriteLine($"  - Médico ID: {MedicoSeleccionado!.IdMedico}");
                System.Diagnostics.Debug.WriteLine($"  - Sucursal ID: {HorarioSeleccionado!.IdSucursal}");
                System.Diagnostics.Debug.WriteLine($"  - Fecha/Hora: {fechaHoraCita:yyyy-MM-dd HH:mm:ss}");
                System.Diagnostics.Debug.WriteLine($"  - Motivo: {MotivoCita.Trim()}");

                var cita = new Cita
                {
                    IdPaciente = PacienteEncontrado!.IdPaciente,
                    IdDoctor = MedicoSeleccionado!.IdMedico,
                    IdSucursal = HorarioSeleccionado!.IdSucursal, // ✅ Ahora viene del horario seleccionado
                    FechaHora = fechaHoraCita,
                    Motivo = MotivoCita.Trim(),
                    TipoCita = "presencial",
                    Estado = "Pendiente"
                };

                var response = await _apiService.CrearCitaAsync(cita);

                if (response.Success)
                {
                    System.Diagnostics.Debug.WriteLine("[CITA] ✅ PUNTO 3-7 COMPLETADO: Cita creada exitosamente");

                    await Shell.Current.DisplayAlert("🎉 Éxito",
                        "Cita médica creada correctamente", "OK");

                    LimpiarFormulario();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[CITA] ❌ Error API: {response.Message}");
                    ShowError(response.Message ?? "Error al crear la cita");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CITA] ❌ Excepción: {ex.Message}");
                ShowError($"Error: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        // COMANDOS DE NAVEGACIÓN Y UTILIDAD
        [RelayCommand]
        private async Task VolverAlMenuAsync()
        {
            try
            {
                var roleId = await SecureStorage.GetAsync("UserRoleId");

                if (roleId == "72") // Recepcionista
                {
                    await Shell.Current.GoToAsync("//RecepcionistaMenuPage");
                }
                else
                {
                    await Shell.Current.GoToAsync("//AdminMenuPage");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CITA] Error navegación: {ex.Message}");
            }
        }

        [RelayCommand]
        private void LimpiarFormulario()
        {
            System.Diagnostics.Debug.WriteLine("[CITA] 🧹 Limpiando formulario...");

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

        // MÉTODOS AUXILIARES
        private async Task CargarDatosInicialesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[CITA] 🔄 Cargando datos iniciales...");
                // Podríamos pre-cargar médicos si es necesario
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CITA] Error datos iniciales: {ex.Message}");
            }
        }

        private DateTime ConstruirFechaHora(DateTime fecha, string horaString)
        {
            try
            {
                if (TimeSpan.TryParse(horaString, out TimeSpan hora))
                {
                    return fecha.Date.Add(hora);
                }
                else
                {
                    return fecha.Date.AddHours(8); // 8:00 AM por defecto
                }
            }
            catch
            {
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
        // 3. ✅ MÉTODO que se ejecuta cuando se recibe cedulaRegistrada (PUNTO 6)
        partial void OnCedulaRegistradaChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                System.Diagnostics.Debug.WriteLine($"[CITA] 🔄 PUNTO 6: Recibida cédula de paciente registrado: {value}");

                // Establecer la cédula y buscar automáticamente
                CedulaPaciente = value;

                // Buscar automáticamente el paciente recién registrado
                _ = BuscarPacienteAutomaticoAsync();
            }
        }
        // 4. ✅ MÉTODO auxiliar para búsqueda automática (PUNTO 6)
        private async Task BuscarPacienteAutomaticoAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[CITA] 🔍 PUNTO 6: Búsqueda automática del paciente recién registrado");

                await Task.Delay(500); // Pequeña pausa para que la UI se actualice

                await BuscarPacienteAsync();

                // Si el paciente se encontró, mostrar mensaje de éxito
                if (PacienteExiste && PacienteEncontrado != null)
                {
                    System.Diagnostics.Debug.WriteLine("[CITA] ✅ PUNTO 6 COMPLETADO: Paciente encontrado automáticamente, flujo continúa");

                    await Shell.Current.DisplayAlert("Flujo Completado",
                        $"Paciente {PacienteEncontrado.NombreCompleto} registrado exitosamente.\n\nContinúe completando la cita.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CITA] ❌ Error en búsqueda automática: {ex.Message}");
            }
        }

        // Property Changed Handlers para auto-actualizaciones
        partial void OnFechaSeleccionadaChanged(DateTime value)
        {
            if (MedicoSeleccionado != null)
            {
                System.Diagnostics.Debug.WriteLine($"[CITA] 📅 PUNTO 7: Fecha cambiada, recargando horarios");
                _ = CargarHorariosDisponiblesAsync();
            }
        }

        // ✅ PUNTO 7: Auto-trigger cuando cambia médico o fecha
        partial void OnMedicoSeleccionadoChanged(Medico? value)
        {
            if (value != null)
            {
                System.Diagnostics.Debug.WriteLine($"[CITA] 🗓️ PUNTO 7: Médico seleccionado, cargando horarios automáticamente");
                _ = CargarHorariosDisponiblesAsync();
            }
        }
    }
}