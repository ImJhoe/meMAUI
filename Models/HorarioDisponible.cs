using System.Text.Json.Serialization;

namespace ClinicaApp.Models
{
    public class HorarioDisponible
    {
        [JsonPropertyName("hora")]
        public string Hora { get; set; } = string.Empty;

        [JsonPropertyName("fecha_hora")]
        public string FechaHora { get; set; } = string.Empty;

        [JsonPropertyName("id_sucursal")]
        public int IdSucursal { get; set; }

        [JsonPropertyName("disponible")]
        public bool Disponible { get; set; } = true;

        // Propiedades calculadas para mostrar en UI
        public string HoraTexto => Hora;
        public DateTime FechaHoraCompleta => DateTime.TryParse(FechaHora, out var fecha) ? fecha : DateTime.Now;
    }
}