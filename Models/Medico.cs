// Models/Medico.cs - VERSIÓN FINAL CORREGIDA
using System.Text.Json.Serialization;
using ClinicaApp.Converters;

namespace ClinicaApp.Models
{
    public class Medico
    {
        [JsonPropertyName("id_medico")]
        public int IdMedico { get; set; }

        [JsonPropertyName("id_doctor")] // Para compatibilidad con API
        public int IdDoctor { get; set; }

        [JsonPropertyName("nombres")]
        public string Nombres { get; set; } = string.Empty;

        [JsonPropertyName("apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        // ✅ USAR CONVERTER PARA MANEJAR CEDULA COMO STRING O NUMBER
        [JsonPropertyName("cedula")]
        [JsonConverter(typeof(StringOrNumberConverter))]
        public string Cedula { get; set; } = string.Empty;

        [JsonPropertyName("correo")]
        public string Correo { get; set; } = string.Empty;

        [JsonPropertyName("contrasena")]
        public string Contrasena { get; set; } = string.Empty;

        [JsonPropertyName("sexo")]
        public string Sexo { get; set; } = "M";

        [JsonPropertyName("nacionalidad")]
        public string Nacionalidad { get; set; } = "Ecuatoriana";

        [JsonPropertyName("id_especialidad")]
        public int IdEspecialidad { get; set; }

        [JsonPropertyName("nombre_especialidad")]
        public string NombreEspecialidad { get; set; } = string.Empty;

        [JsonPropertyName("titulo_profesional")]
        public string TituloProfesional { get; set; } = string.Empty;

        [JsonPropertyName("id_sucursal")]
        public int IdSucursal { get; set; }

        [JsonPropertyName("nombre_sucursal")]
        public string NombreSucursal { get; set; } = string.Empty;

        [JsonPropertyName("numero_colegio")]
        public string NumeroColegio { get; set; } = string.Empty;

        [JsonPropertyName("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        // Propiedades calculadas
        public string NombreCompleto => $"{TituloProfesional} {Nombres} {Apellidos}".Trim();
        public string MedicoInfo => $"{NombreCompleto} - {NombreEspecialidad}";
    }
}