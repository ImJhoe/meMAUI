using System.Text.Json.Serialization;

namespace ClinicaApp.Models
{
    public class Usuario
    {
        [JsonPropertyName("id")]
        public int IdUsuario { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("nombres")]
        public string Nombres { get; set; } = string.Empty;

        [JsonPropertyName("apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [JsonPropertyName("rol")]
        public string NombreRol { get; set; } = string.Empty;

        [JsonPropertyName("rol_id")]
        public int RolId { get; set; }

        [JsonPropertyName("correo")]
        public string Correo { get; set; } = string.Empty;
        // Propiedades calculadas
        public string NombreCompleto => $"{Nombres} {Apellidos}";
        public bool IsAdmin => NombreRol?.ToLower() == "administrador";
        public bool IsRecepcionista => NombreRol?.ToLower() == "recepcionista";
        public bool IsMedico => NombreRol?.ToLower() == "medico";
        public bool IsPaciente => NombreRol?.ToLower() == "paciente";
    }
}