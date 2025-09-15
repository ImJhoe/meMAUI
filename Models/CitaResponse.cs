using System.Text.Json.Serialization;
using System.Globalization;

namespace ClinicaApp.Models
{
    // Modelo específico para recibir respuestas del servidor
    public class CitaResponse
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
        public string FechaHora { get; set; } = string.Empty;

        [JsonPropertyName("motivo")]
        public string Motivo { get; set; } = string.Empty;

        [JsonPropertyName("tipo_cita")]
        public string TipoCita { get; set; } = "presencial";

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = "Pendiente";

        [JsonPropertyName("notas")]
        public string Notas { get; set; } = string.Empty;

        [JsonPropertyName("enlace_virtual")]
        public string? EnlaceVirtual { get; set; } = null;

        [JsonPropertyName("sala_virtual")]
        public string? SalaVirtual { get; set; } = null;

        [JsonPropertyName("nombre_paciente")]
        public string NombrePaciente { get; set; } = string.Empty;

        [JsonPropertyName("cedula_paciente")]
        public string CedulaPaciente { get; set; } = string.Empty;

        [JsonPropertyName("nombre_doctor")]
        public string NombreDoctor { get; set; } = string.Empty;

        [JsonPropertyName("nombre_especialidad")]
        public string NombreEspecialidad { get; set; } = string.Empty;

        [JsonPropertyName("nombre_sucursal")]
        public string NombreSucursal { get; set; } = string.Empty;

        [JsonPropertyName("fecha_creacion")]
        public string FechaCreacion { get; set; } = string.Empty;

        // Métodos para convertir a DateTime cuando sea necesario
        public DateTime GetFechaHora()
        {
            if (DateTime.TryParseExact(FechaHora, "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
            return DateTime.MinValue;
        }

        public DateTime GetFechaCreacion()
        {
            if (DateTime.TryParseExact(FechaCreacion, "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
            return DateTime.MinValue;
        }
    }
}