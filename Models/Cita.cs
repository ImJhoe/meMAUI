using System.Text.Json.Serialization;

namespace ClinicaApp.Models
{
    public class Cita
    {
        [JsonPropertyName("id_cita")]
        public int IdCita { get; set; }

        [JsonPropertyName("id_paciente")]
        public int IdPaciente { get; set; }

        [JsonPropertyName("id_doctor")]
        public int IdDoctor { get; set; }

        [JsonPropertyName("id_sucursal")]
        public int IdSucursal { get; set; }

        [JsonPropertyName("id_tipo_cita")]
        public int IdTipoCita { get; set; } = 1;

        [JsonPropertyName("fecha_hora")]
        public DateTime FechaHora { get; set; }

        [JsonPropertyName("motivo")]
        public string Motivo { get; set; } = string.Empty;

        [JsonPropertyName("tipo_cita")]
        public string TipoCita { get; set; } = "presencial";

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = "Pendiente";

        [JsonPropertyName("notas")]
        public string Notas { get; set; } = string.Empty;

        [JsonPropertyName("enlace_virtual")]
        public string? EnlaceVirtual { get; set; }

        // Propiedades adicionales para mostrar información
        public string NombrePaciente { get; set; } = string.Empty;
        public string NombreMedico { get; set; } = string.Empty;
        public string NombreSucursal { get; set; } = string.Empty;
        public string FechaFormateada => FechaHora.ToString("dd/MM/yyyy HH:mm");
    }
}