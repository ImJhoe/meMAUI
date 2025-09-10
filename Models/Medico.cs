// Models/Medico.cs - VERSIÓN CORREGIDA
using System.Text.Json.Serialization;

namespace ClinicaApp.Models
{
    public class Medico
    {
        public int IdMedico { get; set; }

        [JsonPropertyName("id_doctor")] // Para compatibilidad con API
        public int IdDoctor { get; set; }

        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;

        // ✅ AGREGAR: Propiedad Sexo que faltaba
        public string Sexo { get; set; } = "M";

        // ✅ AGREGAR: Propiedad Nacionalidad que usas en ViewModel
        public string Nacionalidad { get; set; } = "Ecuatoriana";

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