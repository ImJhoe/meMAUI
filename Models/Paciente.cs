namespace ClinicaApp.Models
{
    public class Paciente
    {
        public int IdPaciente { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Genero { get; set; } = string.Empty;
        public string GrupoSanguineo { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }

        // Propiedades calculadas
        public string NombreCompleto => $"{Nombres} {Apellidos}";
        public int Edad
        {
            get
            {
                var hoy = DateTime.Today;
                var edad = hoy.Year - FechaNacimiento.Year;
                if (FechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
                return edad;
            }
        }
        public string InfoBasica => $"{NombreCompleto} - {Cedula} ({Edad} años)";
    }
}