// Models/TriajeCompleto.cs - Modelo principal de Triaje
namespace ClinicaApp.Models
{
    public class TriajeCompleto
    {
        public int IdTriaje { get; set; }
        public int IdCita { get; set; }
        public int IdEnfermero { get; set; }
        public string NombreEnfermero { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public int NivelUrgencia { get; set; }
        public string EstadoTriaje { get; set; } = string.Empty;

        // Signos vitales
        public double? Temperatura { get; set; }
        public string? PresionArterial { get; set; }
        public int? FrecuenciaCardiaca { get; set; }
        public int? FrecuenciaRespiratoria { get; set; }
        public int? SaturacionOxigeno { get; set; }

        // Medidas antropométricas
        public double? Peso { get; set; }
        public double? Talla { get; set; }
        public double? IMC { get; set; }

        // Observaciones
        public string? Observaciones { get; set; }
    }
}