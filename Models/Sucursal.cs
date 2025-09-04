namespace ClinicaApp.Models
{
    public class Sucursal
    {
        public int IdSucursal { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public bool Activa { get; set; } = true;
        public DateTime FechaCreacion { get; set; }

        public override string ToString()
        {
            return Nombre;
        }
    }
}