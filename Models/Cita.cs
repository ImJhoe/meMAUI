using System.Text.Json.Serialization;
using System.Globalization;

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

        // ✅ CAMBIO TEMPORAL: Usar nombre completamente diferente
        [JsonPropertyName("fecha_hora")]
        public string FechaHoraFromServer { get; set; } = string.Empty;

        // ✅ Propiedad computada sin JsonPropertyName
        public DateTime FechaHora
        {
            get
            {
                if (DateTime.TryParseExact(FechaHoraFromServer, "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    return result;
                }

                if (DateTime.TryParse(FechaHoraFromServer, out var fallback))
                {
                    return fallback;
                }

                return DateTime.MinValue;
            }
            set
            {
                FechaHoraFromServer = value.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

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

        // ✅ CAMBIO TEMPORAL: Usar nombre completamente diferente
        [JsonPropertyName("fecha_creacion")]
        public string FechaCreacionFromServer { get; set; } = string.Empty;

        public DateTime FechaCreacion
        {
            get
            {
                if (DateTime.TryParseExact(FechaCreacionFromServer, "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    return result;
                }

                if (DateTime.TryParse(FechaCreacionFromServer, out var fallback))
                {
                    return fallback;
                }

                return DateTime.MinValue;
            }
        }

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