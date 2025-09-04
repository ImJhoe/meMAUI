namespace ClinicaApp.Models
{
    public class Especialidad
    {
        public int IdEspecialidad { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public bool Activa { get; set; } = true;

        public override string ToString()
        {
            return Nombre;
        }
    }
}