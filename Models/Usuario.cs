// Models/Usuario.cs
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

        [JsonPropertyName("correo")]
        public string Correo { get; set; } = string.Empty;

        [JsonPropertyName("rol_id")]
        public int IdRol { get; set; }

        // Propiedad computada para nombre completo
        public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
    }
}