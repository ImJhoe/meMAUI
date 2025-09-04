namespace ClinicaApp.Models
{
    public class Medico
    {
        public int IdMedico { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public int IdEspecialidad { get; set; }
        public string NombreEspecialidad { get; set; } = string.Empty;
        public string TituloProfesional { get; set; } = string.Empty;
        public int IdSucursal { get; set; }
        public string NombreSucursal { get; set; } = string.Empty;
        public string NumeroColegio { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }

        // Propiedades calculadas
        public string NombreCompleto => $"{TituloProfesional} {Nombres} {Apellidos}".Trim();
        public string MedicoInfo => $"{NombreCompleto} - {NombreEspecialidad}";
    }
}