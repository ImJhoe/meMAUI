namespace ClinicaApp.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int StatusCode { get; set; }
        public string? ErrorCode { get; set; }
    }

    // Clase base sin genérico para respuestas simples
    public class ApiResponse : ApiResponse<object>
    {
    }
}