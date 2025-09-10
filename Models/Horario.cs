using System.Text.Json.Serialization;

namespace ClinicaApp.Models
{
    public class Horario
    {
        public int IdHorario { get; set; }

        [JsonPropertyName("id_medico")]
        public int IdMedico { get; set; }

        [JsonPropertyName("id_sucursal")]
        public int IdSucursal { get; set; }

        [JsonPropertyName("dia_semana")]
        public int DiaSemana { get; set; }

        [JsonPropertyName("hora_inicio")]
        public string HoraInicio { get; set; } = string.Empty;

        [JsonPropertyName("hora_fin")]
        public string HoraFin { get; set; } = string.Empty;

        [JsonPropertyName("duracion_cita")]
        public int IntervaloMinutos { get; set; } = 30;

        public bool Activo { get; set; } = true;

        // ✅ CORREGIR: Propiedad calculada para mostrar días correctamente
        public string DiaTexto => DiaSemana switch
        {
            1 => "Lunes",
            2 => "Martes",
            3 => "Miércoles",
            4 => "Jueves",
            5 => "Viernes",
            6 => "Sábado",
            7 => "Domingo",
            _ => "Desconocido"
        };

        // ✅ CORREGIR: Texto completo del horario para mostrar en UI
        public string HorarioTexto => $"{DiaTexto}: {HoraInicio} - {HoraFin} ({IntervaloMinutos}min)";

        // ✅ AGREGAR: Propiedad para obtener el nombre de la sucursal (si lo necesitas)
        public string NombreSucursal { get; set; } = string.Empty;
    }
}