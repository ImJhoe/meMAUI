// ViewModels/MedicoConsultaViewModel.cs - VERSIÓN COMPLETA CON GESTIÓN DE HORARIOS
using ClinicaApp.Models;
using ClinicaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace ClinicaApp.ViewModels
{
    public partial class MedicoConsultaViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public MedicoConsultaViewModel()
        {
            var httpClient = new HttpClient();
            // Cambiar línea 18:
            httpClient.BaseAddress = new Uri("http://192.168.93.154:8081/webservice-slim/");
            _apiService = new ApiService(httpClient);

            InitializeViewModel();
        }

        public MedicoConsultaViewModel(ApiService apiService)
        {
            _apiService = apiService;
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            Title = "Consultar Médicos - Punto 2";
            Medicos = new ObservableCollection<Medico>();
            HorariosDelMedico = new ObservableCollection<Horario>();

            System.Diagnostics.Debug.WriteLine("[MEDICO CONSULTA] ViewModel inicializado");
            _ = CargarMedicosAsync();
        }

        // PROPIEDADES OBSERVABLES
        public ObservableCollection<Medico> Medicos { get; private set; }
        public ObservableCollection<Horario> HorariosDelMedico { get; private set; }

        [ObservableProperty]
        private Medico? medicoSeleccionado;

        [ObservableProperty]
        private bool mostrandoHorarios;

        [ObservableProperty]
        private bool editandoHorario;

        [ObservableProperty]
        private Horario? horarioEditando;

        public bool IsNotLoading => !IsLoading;
        public bool NoHayMedicos => !IsLoading && Medicos.Count == 0;

        // ✅ MEJORAR: Comando de recargar médicos también recarga horarios si están visibles
        [RelayCommand]
        public async Task CargarMedicosAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MEDICO CONSULTA] === INICIANDO CARGA DE MÉDICOS ===");

                IsLoading = true;
                OnPropertyChanged(nameof(IsNotLoading));
                OnPropertyChanged(nameof(NoHayMedicos));

                ClearError();

                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("http://192.168.93.154:8081/webservice-slim/");

                var response = await httpClient.GetAsync("api/medicos");
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonDocument = JsonDocument.Parse(content);
                    var root = jsonDocument.RootElement;

                    if (root.TryGetProperty("success", out var successProperty) &&
                        successProperty.GetBoolean() &&
                        root.TryGetProperty("data", out var dataProperty))
                    {
                        // Guardar el médico seleccionado antes de limpiar
                        var medicoAnteriormenteSeleccionado = MedicoSeleccionado;

                        Medicos.Clear();

                        foreach (var item in dataProperty.EnumerateArray())
                        {
                            try
                            {
                                var medico = new Medico
                                {
                                    IdMedico = item.TryGetProperty("id_medico", out var idProp) ? idProp.GetInt32() : 0,
                                    Nombres = item.TryGetProperty("nombres", out var nombresProp) ? nombresProp.GetString() ?? "" : "",
                                    Apellidos = item.TryGetProperty("apellidos", out var apellidosProp) ? apellidosProp.GetString() ?? "" : "",
                                    Cedula = item.TryGetProperty("cedula", out var cedulaProp) ?
                                        (cedulaProp.ValueKind == JsonValueKind.String ? cedulaProp.GetString() ?? "" : cedulaProp.GetInt64().ToString()) : "",
                                    Correo = item.TryGetProperty("correo", out var correoProp) ? correoProp.GetString() ?? "" : "",
                                    NombreEspecialidad = item.TryGetProperty("nombre_especialidad", out var especialidadProp) ? especialidadProp.GetString() ?? "" : "",
                                    TituloProfesional = item.TryGetProperty("titulo_profesional", out var tituloProp) ? tituloProp.GetString() ?? "" : ""
                                };

                                Medicos.Add(medico);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Error procesando médico: {ex.Message}");
                            }
                        }

                        System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Total médicos cargados: {Medicos.Count}");

                        // ✅ AUTO-REFRESH: Si se estaban mostrando horarios, recargarlos también
                        if (MostrandoHorarios && medicoAnteriormenteSeleccionado != null)
                        {
                            // Buscar el médico actualizado que corresponde al seleccionado anteriormente
                            var medicoActualizado = Medicos.FirstOrDefault(m => m.IdMedico == medicoAnteriormenteSeleccionado.IdMedico);
                            if (medicoActualizado != null)
                            {
                                MedicoSeleccionado = medicoActualizado;
                                await RecargarHorariosDelMedicoSeleccionadoAsync();
                                System.Diagnostics.Debug.WriteLine("[MEDICO CONSULTA] Horarios recargados automáticamente después de actualizar médicos");
                            }
                        }
                    }
                    else
                    {
                        ShowError("El servidor no devolvió datos válidos");
                    }
                }
                else
                {
                    ShowError($"Error del servidor: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Error general: {ex.Message}");
                ShowError($"Error inesperado: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(IsNotLoading));
                OnPropertyChanged(nameof(NoHayMedicos));
            }
        }

        // COMANDO para ver horarios de un médico
        [RelayCommand]
        public async Task VerHorariosAsync(Medico medico)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Consultando horarios para: {medico.NombreCompleto}");

                IsLoading = true;
                OnPropertyChanged(nameof(IsNotLoading));

                MedicoSeleccionado = medico;
                HorariosDelMedico.Clear();

                // Obtener horarios del médico desde la API
                var horarios = await _apiService.ObtenerHorariosMedicoAsync(medico.IdMedico);

                if (horarios.Any())
                {
                    foreach (var horario in horarios)
                    {
                        HorariosDelMedico.Add(horario);
                    }

                    MostrandoHorarios = true;
                    System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] {horarios.Count} horarios encontrados");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Sin Horarios",
                        $"El médico {medico.NombreCompleto} no tiene horarios asignados.\n\nPuede asignar horarios usando el botón 'Configurar'.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error al consultar horarios: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(IsNotLoading));
            }
        }

        // COMANDO para configurar horarios (navegar a pantalla de configuración)
        [RelayCommand]
        public async Task ConfigurarHorariosAsync(Medico medico)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Configurando horarios para: {medico.NombreCompleto}");

                // Navegar a página de configuración de horarios
                var parameters = new Dictionary<string, object>
                {
                    ["MedicoId"] = medico.IdMedico,
                    ["MedicoNombre"] = medico.NombreCompleto,
                    ["MedicoData"] = medico
                };

                await Shell.Current.GoToAsync("ConfigurarHorariosPage", parameters);
            }
            catch (Exception ex)
            {
                // Si no existe la página, mostrar modal de configuración
                await MostrarModalConfiguracionAsync(medico);
            }
        }

        // COMANDO para editar un horario específico
        [RelayCommand]
        public async Task EditarHorarioAsync(Horario horario)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Editando horario: {horario.HorarioTexto}");

                HorarioEditando = horario;
                EditandoHorario = true;

                // Aquí puedes abrir un modal o navegar a una página de edición
                await MostrarModalEdicionHorarioAsync(horario);
            }
            catch (Exception ex)
            {
                ShowError($"Error al editar horario: {ex.Message}");
            }
        }

        // COMANDO para eliminar un horario - CON AUTO-REFRESH
        [RelayCommand]
        public async Task EliminarHorarioAsync(Horario horario)
        {
            try
            {
                var confirmar = await App.Current.MainPage.DisplayAlert(
                    "Confirmar Eliminación",
                    $"¿Está seguro que desea eliminar el horario:\n{horario.HorarioTexto}?",
                    "Sí, Eliminar",
                    "Cancelar");

                if (confirmar)
                {
                    IsLoading = true;
                    OnPropertyChanged(nameof(IsNotLoading));

                    var resultado = await _apiService.EliminarHorarioAsync(horario.IdHorario);

                    if (resultado.Success)
                    {
                        await App.Current.MainPage.DisplayAlert("Éxito", "Horario eliminado correctamente", "OK");

                        // ✅ AUTO-REFRESH: Recargar horarios automáticamente
                        await RecargarHorariosDelMedicoSeleccionadoAsync();
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Error",
                            $"No se pudo eliminar el horario: {resultado.Message}", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error al eliminar horario: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(IsNotLoading));
            }
        }

        // COMANDO para cerrar vista de horarios
        [RelayCommand]
        public async Task CerrarHorariosAsync()
        {
            MostrandoHorarios = false;
            MedicoSeleccionado = null;
            HorariosDelMedico.Clear();
        }

        // COMANDO para volver al menú
        [RelayCommand]
        public async Task VolverAlMenuAsync()
        {
            await Shell.Current.GoToAsync("//AdminMenuPage");
        }

        // MÉTODO AUXILIAR para mostrar modal de configuración
        private async Task MostrarModalConfiguracionAsync(Medico medico)
        {
            var opciones = new string[] {
                "Ver Horarios Actuales",
                "Asignar Nuevos Horarios",
                "Modificar Horarios Existentes",
                "Cancelar"
            };

            var accion = await App.Current.MainPage.DisplayActionSheet(
                $"Configurar horarios para {medico.NombreCompleto}",
                "Cancelar",
                null,
                opciones);

            switch (accion)
            {
                case "Ver Horarios Actuales":
                    await VerHorariosAsync(medico);
                    break;
                case "Asignar Nuevos Horarios":
                    await MostrarFormularioNuevoHorarioAsync(medico);
                    break;
                case "Modificar Horarios Existentes":
                    await VerHorariosAsync(medico);
                    break;
            }
        }

        // MÉTODO AUXILIAR para mostrar modal de edición de horario
        private async Task MostrarModalEdicionHorarioAsync(Horario horario)
        {
            var opciones = new string[] {
                "Cambiar Horario",
                "Cambiar Día",
                "Eliminar Horario",
                "Cancelar"
            };

            var accion = await App.Current.MainPage.DisplayActionSheet(
                $"Editar: {horario.HorarioTexto}",
                "Cancelar",
                "Eliminar Horario",
                opciones.Take(3).ToArray());

            switch (accion)
            {
                case "Cambiar Horario":
                    await CambiarHorarioAsync(horario);
                    break;
                case "Cambiar Día":
                    await CambiarDiaAsync(horario);
                    break;
                case "Eliminar Horario":
                    await EliminarHorarioAsync(horario);
                    break;
            }
        }

        // MÉTODO para mostrar formulario de nuevo horario
        private async Task MostrarFormularioNuevoHorarioAsync(Medico medico)
        {
            await App.Current.MainPage.DisplayAlert("Información",
                $"Para asignar nuevos horarios a {medico.NombreCompleto}, utilice la pantalla de 'Registro de Médicos' (Punto 1).\n\nEsta funcionalidad permite consultar y modificar horarios existentes.",
                "Entendido");
        }

        // MÉTODO para cambiar horario - CON AUTO-REFRESH
        private async Task CambiarHorarioAsync(Horario horario)
        {
            var nuevoHorario = await App.Current.MainPage.DisplayPromptAsync(
                "Cambiar Horario",
                $"Horario actual: {horario.HoraInicio} - {horario.HoraFin}\nIngrese nuevo horario (ej: 08:00-17:00):",
                "Guardar",
                "Cancelar",
                placeholder: "08:00-17:00");

            if (!string.IsNullOrWhiteSpace(nuevoHorario))
            {
                var partes = nuevoHorario.Split('-');
                if (partes.Length == 2)
                {
                    try
                    {
                        IsLoading = true;
                        OnPropertyChanged(nameof(IsNotLoading));

                        var horarioActualizado = new Horario
                        {
                            IdHorario = horario.IdHorario,
                            IdMedico = horario.IdMedico,
                            IdSucursal = horario.IdSucursal,
                            DiaSemana = horario.DiaSemana,
                            HoraInicio = partes[0].Trim(),
                            HoraFin = partes[1].Trim(),
                            IntervaloMinutos = horario.IntervaloMinutos,
                            NombreSucursal = horario.NombreSucursal,
                            Activo = horario.Activo
                        };

                        var resultado = await _apiService.ActualizarHorarioAsync(horarioActualizado);

                        if (resultado.Success)
                        {
                            await App.Current.MainPage.DisplayAlert("Éxito", "Horario actualizado correctamente", "OK");

                            // ✅ AUTO-REFRESH: Recargar horarios automáticamente
                            await RecargarHorariosDelMedicoSeleccionadoAsync();
                        }
                        else
                        {
                            await App.Current.MainPage.DisplayAlert("Error",
                                $"No se pudo actualizar el horario: {resultado.Message}", "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowError($"Error actualizando horario: {ex.Message}");
                    }
                    finally
                    {
                        IsLoading = false;
                        OnPropertyChanged(nameof(IsNotLoading));
                    }
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Formato incorrecto. Use formato: HH:mm-HH:mm", "OK");
                }
            }
        }


        // MÉTODO para cambiar día - CON AUTO-REFRESH
        private async Task CambiarDiaAsync(Horario horario)
        {
            var dias = new string[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };

            var nuevoDia = await App.Current.MainPage.DisplayActionSheet(
                "Seleccionar nuevo día",
                "Cancelar",
                null,
                dias);

            if (!string.IsNullOrWhiteSpace(nuevoDia) && nuevoDia != "Cancelar")
            {
                try
                {
                    IsLoading = true;
                    OnPropertyChanged(nameof(IsNotLoading));

                    var numeroDia = Array.IndexOf(dias, nuevoDia) + 1;

                    var horarioActualizado = new Horario
                    {
                        IdHorario = horario.IdHorario,
                        IdMedico = horario.IdMedico,
                        IdSucursal = horario.IdSucursal,
                        DiaSemana = numeroDia,
                        HoraInicio = horario.HoraInicio,
                        HoraFin = horario.HoraFin,
                        IntervaloMinutos = horario.IntervaloMinutos,
                        NombreSucursal = horario.NombreSucursal,
                        Activo = horario.Activo
                    };

                    var resultado = await _apiService.ActualizarHorarioAsync(horarioActualizado);

                    if (resultado.Success)
                    {
                        await App.Current.MainPage.DisplayAlert("Éxito", $"Día cambiado a {nuevoDia}", "OK");

                        // ✅ AUTO-REFRESH: Recargar horarios automáticamente
                        await RecargarHorariosDelMedicoSeleccionadoAsync();
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Error",
                            $"No se pudo cambiar el día: {resultado.Message}", "OK");
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Error cambiando día: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                    OnPropertyChanged(nameof(IsNotLoading));
                }
            }
        }

        // Agregar esta propiedad observable a tu MedicoConsultaViewModel

        [ObservableProperty]
        private string mensajeActualizacion = string.Empty;

        [ObservableProperty]
        private bool mostrandoMensajeActualizacion;

        // Modificar el método RecargarHorariosDelMedicoSeleccionadoAsync para mostrar feedback

        private async Task RecargarHorariosDelMedicoSeleccionadoAsync()
        {
            if (MedicoSeleccionado != null)
            {
                try
                {
                    // ✅ MOSTRAR MENSAJE DE ACTUALIZACIÓN
                    MensajeActualizacion = "Actualizando horarios...";
                    MostrandoMensajeActualizacion = true;

                    System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Auto-recargando horarios para: {MedicoSeleccionado.NombreCompleto}");

                    HorariosDelMedico.Clear();
                    var horariosActualizados = await _apiService.ObtenerHorariosMedicoAsync(MedicoSeleccionado.IdMedico);

                    foreach (var horario in horariosActualizados)
                    {
                        HorariosDelMedico.Add(horario);
                    }

                    System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Horarios recargados: {horariosActualizados.Count}");

                    // ✅ MOSTRAR MENSAJE DE ÉXITO TEMPORAL
                    MensajeActualizacion = "✅ Horarios actualizados";

                    // Ocultar mensaje después de 2 segundos
                    await Task.Delay(2000);
                    MostrandoMensajeActualizacion = false;

                    OnPropertyChanged(nameof(HorariosDelMedico));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MEDICO CONSULTA] Error recargando horarios: {ex.Message}");

                    // ✅ MOSTRAR MENSAJE DE ERROR TEMPORAL
                    MensajeActualizacion = "❌ Error al actualizar";
                    await Task.Delay(3000);
                    MostrandoMensajeActualizacion = false;

                    ShowError($"Error al recargar horarios: {ex.Message}");
                }
            }
        }
    }
}