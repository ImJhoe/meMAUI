// Models/ApiResponse.cs
using System.Text.Json.Serialization;

namespace ClinicaApp.Models
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        public int StatusCode { get; set; }
    }

    // Clase específica para la respuesta de login
    public class LoginData
    {
        [JsonPropertyName("user")]
        public Usuario User { get; set; } = new();

        [JsonPropertyName("permissions")]
        public object? Permissions { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}