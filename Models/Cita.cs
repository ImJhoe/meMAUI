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

        // Propiedades adicionales que pueden venir del servidor
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
        public DateTime FechaCreacion { get; set; }

        // Propiedades calculadas
        public string CitaInfo => $"{NombrePaciente} - {NombreDoctor} ({FechaHora:dd/MM/yyyy HH:mm})";
        public string EstadoColor => Estado switch
        {
            "Pendiente" => "Orange",
            "Confirmada" => "Green",
            "Cancelada" => "Red",
            "Completada" => "Blue",
            _ => "Gray"
        };
    }
}