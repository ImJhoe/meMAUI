using System.Text.Json.Serialization;

namespace ClinicaApp.Models
{
    public class Sucursal
    {
        [JsonPropertyName("id_sucursal")]
        public int IdSucursal { get; set; }

        [JsonPropertyName("nombre_sucursal")]
        public string NombreSucursal { get; set; } = string.Empty;

        [JsonPropertyName("direccion")]
        public string Direccion { get; set; } = string.Empty;

        [JsonPropertyName("telefono")]
        public string Telefono { get; set; } = string.Empty;
    }
}