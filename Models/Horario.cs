namespace ClinicaApp.Models
{
    public class Horario
    {
        public int IdHorario { get; set; }
        public int IdMedico { get; set; }
        public string DiaSemana { get; set; } = string.Empty;
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public int IntervaloMinutos { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }

        // Propiedades adicionales para mostrar horarios disponibles
        public DateTime? Fecha { get; set; }
        public DateTime? FechaHora { get; set; }
        public bool Disponible { get; set; } = true;
        public string NombreMedico { get; set; } = string.Empty;

        // Propiedades calculadas
        public string HorarioTexto => $"{DiaSemana}: {HoraInicio:hh\\:mm} - {HoraFin:hh\\:mm}";
        public string FechaHoraTexto => FechaHora?.ToString("dd/MM/yyyy HH:mm") ?? "";
        public string SlotTexto => Fecha.HasValue ?
            $"{Fecha.Value:dd/MM/yyyy} {HoraInicio:hh\\:mm}" :
            HorarioTexto;
    }
}