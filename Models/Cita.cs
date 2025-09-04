namespace ClinicaApp.Models
{
    public class Cita
    {
        public int IdCita { get; set; }
        public int IdPaciente { get; set; }
        public int IdDoctor { get; set; }
        public int IdSucursal { get; set; }
        public int IdTipoCita { get; set; } = 1;
        public DateTime FechaHora { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string TipoCita { get; set; } = "presencial";
        public string Estado { get; set; } = "Pendiente";
        public DateTime FechaCreacion { get; set; }
        public string? Notas { get; set; }
        public string? EnlaceVirtual { get; set; }
        public string? SalaVirtual { get; set; }

        // Información adicional para mostrar
        public string NombrePaciente { get; set; } = string.Empty;
        public string NombreDoctor { get; set; } = string.Empty;
        public string NombreSucursal { get; set; } = string.Empty;
        public string EspecialidadDoctor { get; set; } = string.Empty;

        // Propiedades calculadas
        public string FechaHoraTexto => FechaHora.ToString("dd/MM/yyyy HH:mm");
        public string CitaInfo => $"{NombrePaciente} - {NombreDoctor}";
        public string EstadoColor => Estado?.ToLower() switch
        {
            "pendiente" => "#f39c12",
            "confirmada" => "#27ae60",
            "completada" => "#2ecc71",
            "cancelada" => "#e74c3c",
            _ => "#95a5a6"
        };
    }
}