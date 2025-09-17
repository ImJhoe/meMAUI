using ClinicaApp.Models;
using ClinicaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ClinicaApp.ViewModels
{
    public partial class MedicoRegistroViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        // Constructor sin parámetros para XAML
        public MedicoRegistroViewModel()
        {
            var httpClient = new HttpClient();
            // Línea 17, cambiar:
            httpClient.BaseAddress = new Uri("http://192.168.93.154:8081/webservice-slim/");
            _apiService = new ApiService(httpClient);
            InitializeViewModel();
        }

        // Constructor con DI
        public MedicoRegistroViewModel(ApiService apiService)
        {
            _apiService = apiService;
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            Title = "Registro de Médicos - Punto 1";
            System.Diagnostics.Debug.WriteLine("[MEDICO REG] ViewModel inicializado");

            // Inicializar colecciones
            Especialidades = new ObservableCollection<Especialidad>();
            Sucursales = new ObservableCollection<Sucursal>();
            SucursalesSeleccionadas = new ObservableCollection<Sucursal>();
            HorariosAsignados = new ObservableCollection<Horario>();

            // ✅ AGREGAR: Inicializar días de la semana
            DiasDisponibles = new ObservableCollection<string>
    {
        "Lunes", "Martes", "Miércoles", "Jueves", "Viernes"
        // Solo lunes a viernes como solicitaste
    };

            CargarDatosIniciales();
        }

        // Propiedades del médico
        [ObservableProperty]
        private string cedula = string.Empty;

        [ObservableProperty]
        private string nombres = string.Empty;

        [ObservableProperty]
        private string apellidos = string.Empty;

        [ObservableProperty]
        private string correo = string.Empty;

        [ObservableProperty]
        private string tituloProfesional = string.Empty;

        // ✅ CORREGIR: Agregar propiedades que faltaban
        [ObservableProperty]
        private string sexoSeleccionado = "M";

        [ObservableProperty]
        private string contrasena = "123456";

        [ObservableProperty]
        private Especialidad? especialidadSeleccionada;

        // ✅ CORREGIR: Colecciones como propiedades, no get-only
        public ObservableCollection<string> DiasDisponibles { get; private set; }
        public ObservableCollection<Especialidad> Especialidades { get; private set; }
        public ObservableCollection<Sucursal> Sucursales { get; private set; }
        public ObservableCollection<Sucursal> SucursalesSeleccionadas { get; private set; }
        public ObservableCollection<Horario> HorariosAsignados { get; private set; }

        // Propiedades para horarios
        [ObservableProperty]
        private string diaSeleccionadoTexto = "Lunes";

        [ObservableProperty]
        private TimeSpan horaInicio = new TimeSpan(8, 0, 0);

        [ObservableProperty]
        private TimeSpan horaFin = new TimeSpan(17, 0, 0);

        [ObservableProperty]
        private int duracionCita = 30;

        [ObservableProperty]
        private Sucursal? sucursalHorario;

        private async void CargarDatosIniciales()
        {
            try
            {
                ShowLoading(true);
                System.Diagnostics.Debug.WriteLine("[MEDICO REG] Cargando datos iniciales...");

                // Cargar especialidades
                var especialidadesResponse = await _apiService.ObtenerEspecialidadesAsync();
                if (especialidadesResponse.Success && especialidadesResponse.Data != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[MEDICO REG] Cargadas {especialidadesResponse.Data.Count} especialidades");
                    Especialidades.Clear();
                    foreach (var esp in especialidadesResponse.Data)
                    {
                        Especialidades.Add(esp);
                        System.Diagnostics.Debug.WriteLine($"  - {esp.NombreEspecialidad}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[MEDICO REG] Error cargando especialidades: {especialidadesResponse.Message}");
                    ShowError("Error cargando especialidades");
                }

                // Cargar sucursales
                var sucursalesResponse = await _apiService.ObtenerSucursalesAsync();
                if (sucursalesResponse.Success && sucursalesResponse.Data != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[MEDICO REG] Cargadas {sucursalesResponse.Data.Count} sucursales");
                    Sucursales.Clear();
                    foreach (var suc in sucursalesResponse.Data)
                    {
                        Sucursales.Add(suc);
                        System.Diagnostics.Debug.WriteLine($"  - {suc.NombreSucursal}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[MEDICO REG] Error cargando sucursales: {sucursalesResponse.Message}");
                    ShowError("Error cargando sucursales");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO REG] Exception cargando datos: {ex.Message}");
                ShowError($"Error cargando datos: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        [RelayCommand]
        private void AgregarSucursal(Sucursal sucursal)
        {
            if (sucursal != null && !SucursalesSeleccionadas.Contains(sucursal))
            {
                SucursalesSeleccionadas.Add(sucursal);
                System.Diagnostics.Debug.WriteLine($"[MEDICO REG] Sucursal agregada: {sucursal.NombreSucursal}");
            }
        }

        [RelayCommand]
        private void QuitarSucursal(Sucursal sucursal)
        {
            if (sucursal != null && SucursalesSeleccionadas.Contains(sucursal))
            {
                SucursalesSeleccionadas.Remove(sucursal);
                System.Diagnostics.Debug.WriteLine($"[MEDICO REG] Sucursal removida: {sucursal.NombreSucursal}");

                // Remover horarios de esta sucursal
                var horariosARemover = HorariosAsignados.Where(h => h.IdSucursal == sucursal.IdSucursal).ToList();
                foreach (var horario in horariosARemover)
                {
                    HorariosAsignados.Remove(horario);
                }
            }
        }

        [RelayCommand]
        private void AgregarHorario()
        {
            try
            {
                if (SucursalHorario == null)
                {
                    ShowError("Seleccione una sucursal para el horario");
                    return;
                }

                if (HoraInicio >= HoraFin)
                {
                    ShowError("La hora de inicio debe ser menor que la hora de fin");
                    return;
                }

                // ✅ CONVERTIR: Texto del día a número
                int diaNumero = DiaSeleccionadoTexto switch
                {
                    "Lunes" => 1,
                    "Martes" => 2,
                    "Miércoles" => 3,
                    "Jueves" => 4,
                    "Viernes" => 5,
                    _ => 1
                };

                var nuevoHorario = new Horario
                {
                    DiaSemana = diaNumero, // Usar el número convertido
                    HoraInicio = HoraInicio.ToString(@"hh\:mm"),
                    HoraFin = HoraFin.ToString(@"hh\:mm"),
                    IntervaloMinutos = DuracionCita,
                    IdSucursal = SucursalHorario.IdSucursal,
                    Activo = true
                };

                // Verificar conflictos
                var conflicto = HorariosAsignados.FirstOrDefault(h =>
                    h.DiaSemana == nuevoHorario.DiaSemana &&
                    h.IdSucursal == nuevoHorario.IdSucursal);

                if (conflicto != null)
                {
                    ShowError($"Ya existe un horario para {DiaSeleccionadoTexto} en esta sucursal");
                    return;
                }

                HorariosAsignados.Add(nuevoHorario);
                System.Diagnostics.Debug.WriteLine($"[MEDICO REG] Horario agregado: {nuevoHorario.HorarioTexto}");

                // Limpiar formulario
                DiaSeleccionadoTexto = "Lunes";
                HoraInicio = new TimeSpan(8, 0, 0);
                HoraFin = new TimeSpan(17, 0, 0);
                SucursalHorario = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO REG] Error agregando horario: {ex.Message}");
                ShowError($"Error agregando horario: {ex.Message}");
            }
        }

        [RelayCommand]
        private void QuitarHorario(Horario horario)
        {
            if (horario != null && HorariosAsignados.Contains(horario))
            {
                HorariosAsignados.Remove(horario);
                System.Diagnostics.Debug.WriteLine($"[MEDICO REG] Horario removido: {horario.HorarioTexto}");
            }
        }

        [RelayCommand]
        private async Task GuardarMedico()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MEDICO REG] Iniciando guardado...");

                ShowLoading(true);

                if (!ValidarFormulario())
                {
                    ShowLoading(false);
                    return;
                }

                var medico = new Medico
                {
                    Nombres = Nombres.Trim(),
                    Apellidos = Apellidos.Trim(),
                    Cedula = Cedula.Trim(),
                    Correo = Correo.Trim(),
                    Contrasena = "123456", // Contraseña por defecto
                    Sexo = "M", // Por defecto masculino
                    Nacionalidad = "Ecuatoriana",
                    TituloProfesional = TituloProfesional?.Trim() ?? "",
                    NumeroColegio = "", // Campo opcional
                    IdEspecialidad = EspecialidadSeleccionada?.IdEspecialidad ?? 0,
                    IdSucursal = SucursalesSeleccionadas.FirstOrDefault()?.IdSucursal ?? 0
                };

                System.Diagnostics.Debug.WriteLine($"[MEDICO REG] Médico a guardar:");
                System.Diagnostics.Debug.WriteLine($"  - Nombre: {medico.TituloProfesional} {medico.Nombres} {medico.Apellidos}");
                System.Diagnostics.Debug.WriteLine($"  - Cédula: {medico.Cedula}");
                System.Diagnostics.Debug.WriteLine($"  - Especialidad: {EspecialidadSeleccionada?.NombreEspecialidad}");

                var response = await _apiService.CrearMedicoAsync(medico);

                if (response.Success)
                {
                    System.Diagnostics.Debug.WriteLine($"[MEDICO REG] Médico guardado exitosamente");

                    // ✅ CORREGIR: Usar el ID correcto del médico creado
                    var medicoCreado = response.Data;
                    int idMedicoCreado = medicoCreado.IdMedico > 0 ? medicoCreado.IdMedico : medicoCreado.IdDoctor;

                    System.Diagnostics.Debug.WriteLine($"[MEDICO REG] ID del médico creado: {idMedicoCreado}");

                    if (HorariosAsignados.Any())
                    {
                        System.Diagnostics.Debug.WriteLine($"[MEDICO REG] Asignando {HorariosAsignados.Count} horarios");

                        // ✅ CORREGIR: Asignar el ID correcto a cada horario
                        foreach (var horario in HorariosAsignados)
                        {
                            horario.IdMedico = idMedicoCreado;
                            System.Diagnostics.Debug.WriteLine($"[MEDICO REG] Horario: Sucursal={horario.IdSucursal}, Día={horario.DiaSemana}, {horario.HoraInicio}-{horario.HoraFin}");
                        }

                        var horariosResponse = await _apiService.AsignarHorariosAsync(idMedicoCreado, HorariosAsignados.ToList());

                        if (horariosResponse.Success)
                        {
                            ShowError("Médico y horarios registrados exitosamente");
                        }
                        else
                        {
                            ShowError($"Médico creado pero error en horarios: {horariosResponse.Message}");
                        }
                    }
                    else
                    {
                        ShowError("Médico registrado exitosamente");
                    }

                    LimpiarFormulario();
                }
                else
                {
                    ShowError($"Error: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MEDICO REG] Exception: {ex.Message}");
                ShowError($"Error inesperado: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }
        // ✅ AGREGAR: Método ValidarFormulario que falta
        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(Cedula))
            {
                ShowError("La cédula es requerida");
                return false;
            }

            if (Cedula.Length != 10)
            {
                ShowError("La cédula debe tener 10 dígitos");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Nombres))
            {
                ShowError("Los nombres son requeridos");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Apellidos))
            {
                ShowError("Los apellidos son requeridos");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Correo))
            {
                ShowError("El correo es requerido");
                return false;
            }

            if (!Correo.Contains("@") || !Correo.Contains("."))
            {
                ShowError("El formato del correo es inválido");
                return false;
            }

            if (EspecialidadSeleccionada == null)
            {
                ShowError("Debe seleccionar una especialidad");
                return false;
            }

            if (!SucursalesSeleccionadas.Any())
            {
                ShowError("Debe seleccionar al menos una sucursal");
                return false;
            }

            return true;
        }

        private void LimpiarFormulario()
        {
            Cedula = string.Empty;
            Nombres = string.Empty;
            Apellidos = string.Empty;
            Correo = string.Empty;
            TituloProfesional = string.Empty;
            EspecialidadSeleccionada = null;
            SucursalesSeleccionadas.Clear();
            HorariosAsignados.Clear();

            // ✅ AGREGAR: Limpiar nuevas propiedades
            DiaSeleccionadoTexto = "Lunes";
            SucursalHorario = null;
        }

        [RelayCommand]
        private async Task VolverAlMenu()
        {
            await Shell.Current.GoToAsync("//AdminMenuPage");
        }

    }
}