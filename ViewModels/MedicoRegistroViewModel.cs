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

        public MedicoRegistroViewModel(ApiService apiService)
        {
            _apiService = apiService;
            Title = "Registro de Médico";
            Especialidades = new ObservableCollection<Especialidad>();
            Sucursales = new ObservableCollection<Sucursal>();

            // Cargar datos iniciales
            _ = LoadDataAsync();
        }

        [ObservableProperty]
        private string nombres = string.Empty;

        [ObservableProperty]
        private string apellidos = string.Empty;

        [ObservableProperty]
        private string cedula = string.Empty;

        [ObservableProperty]
        private string correo = string.Empty;

        [ObservableProperty]
        private string contrasena = string.Empty;

        [ObservableProperty]
        private string tituloProfesional = string.Empty;

        [ObservableProperty]
        private string numeroColegio = string.Empty;

        [ObservableProperty]
        private Especialidad? especialidadSeleccionada;

        [ObservableProperty]
        private Sucursal? sucursalSeleccionada;

        public ObservableCollection<Especialidad> Especialidades { get; }
        public ObservableCollection<Sucursal> Sucursales { get; }

        private async Task LoadDataAsync()
        {
            try
            {
                ShowLoading(true);

                // Cargar especialidades
                var especialidadesResponse = await _apiService.ObtenerEspecialidadesAsync();
                if (especialidadesResponse.Success && especialidadesResponse.Data != null)
                {
                    Especialidades.Clear();
                    foreach (var especialidad in especialidadesResponse.Data)
                    {
                        Especialidades.Add(especialidad);
                    }
                }

                // Cargar sucursales
                var sucursalesResponse = await _apiService.ObtenerSucursalesAsync();
                if (sucursalesResponse.Success && sucursalesResponse.Data != null)
                {
                    Sucursales.Clear();
                    foreach (var sucursal in sucursalesResponse.Data)
                    {
                        Sucursales.Add(sucursal);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error al cargar datos: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        [RelayCommand]
        private async Task GuardarMedicoAsync()
        {
            if (!ValidarFormulario())
                return;

            try
            {
                ShowLoading(true);
                ClearError();

                var medico = new Medico
                {
                    Nombres = Nombres.Trim(),
                    Apellidos = Apellidos.Trim(),
                    Cedula = Cedula.Trim(),
                    Correo = Correo.Trim(),
                    Contrasena = Contrasena.Trim(),
                    IdEspecialidad = EspecialidadSeleccionada!.IdEspecialidad,
                    TituloProfesional = TituloProfesional.Trim(),
                    NumeroColegio = NumeroColegio.Trim(),
                    IdSucursal = SucursalSeleccionada!.IdSucursal
                };

                var response = await _apiService.CrearMedicoAsync(medico);

                if (response.Success)
                {
                    await Shell.Current.DisplayAlert("Éxito", "Médico registrado correctamente", "OK");
                    LimpiarFormulario();
                }
                else
                {
                    ShowError(response.Message ?? "Error al registrar médico");
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

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(Nombres))
            {
                ShowError("El nombre es requerido");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Apellidos))
            {
                ShowError("Los apellidos son requeridos");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Cedula))
            {
                ShowError("La cédula es requerida");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Correo) || !Correo.Contains("@"))
            {
                ShowError("Ingrese un correo válido");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Contrasena) || Contrasena.Length < 6)
            {
                ShowError("La contraseña debe tener al menos 6 caracteres");
                return false;
            }

            if (EspecialidadSeleccionada == null)
            {
                ShowError("Seleccione una especialidad");
                return false;
            }

            if (SucursalSeleccionada == null)
            {
                ShowError("Seleccione una sucursal");
                return false;
            }

            return true;
        }

        [RelayCommand]
        private void LimpiarFormulario()
        {
            Nombres = string.Empty;
            Apellidos = string.Empty;
            Cedula = string.Empty;
            Correo = string.Empty;
            Contrasena = string.Empty;
            TituloProfesional = string.Empty;
            NumeroColegio = string.Empty;
            EspecialidadSeleccionada = null;
            SucursalSeleccionada = null;
            ClearError();
        }
    }
}