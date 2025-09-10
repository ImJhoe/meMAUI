using System.Text.Json.Serialization;

namespace ClinicaApp.Models
{
    public class Paciente
    {
        [JsonPropertyName("id_paciente")]
        public int IdPaciente { get; set; }

        [JsonPropertyName("id_usuario")]
        public int IdUsuario { get; set; }

        [JsonPropertyName("cedula")]
        public string Cedula { get; set; } = string.Empty;

        [JsonPropertyName("nombres")]
        public string Nombres { get; set; } = string.Empty;

        [JsonPropertyName("apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [JsonPropertyName("correo")]
        public string Correo { get; set; } = string.Empty;

        [JsonPropertyName("telefono")]
        public string Telefono { get; set; } = string.Empty;

        [JsonPropertyName("fecha_nacimiento")]
        public DateTime FechaNacimiento { get; set; }

        [JsonPropertyName("sexo")]
        public string Sexo { get; set; } = string.Empty;

        [JsonPropertyName("tipo_sangre")]
        public string TipoSangre { get; set; } = string.Empty;

        [JsonPropertyName("alergias")]
        public string Alergias { get; set; } = string.Empty;

        [JsonPropertyName("contacto_emergencia")]
        public string ContactoEmergencia { get; set; } = string.Empty;

        [JsonPropertyName("telefono_emergencia")]
        public string TelefonoEmergencia { get; set; } = string.Empty;

        // Propiedades calculadas
        public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
        public int Edad => DateTime.Now.Year - FechaNacimiento.Year;
    }
}