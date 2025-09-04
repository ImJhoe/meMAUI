using ClinicaApp.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace ClinicaApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService>? _logger;
        private readonly string _baseUrl = "http://192.168.1.8/webservice-slim/"; // Tu IP
        private readonly JsonSerializerOptions _jsonOptions;

        // Constructor para testing (sin logger)
        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
        }

        // Constructor principal con DI
        public ApiService(HttpClient httpClient, ILogger<ApiService>? logger = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _logger = logger;

            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
            }

            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // Método genérico GET mejorado con logging detallado
        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                var fullUrl = $"{_baseUrl}/{endpoint.TrimStart('/')}";

                // Log detallado para debugging
                System.Diagnostics.Debug.WriteLine($"[GET REQUEST] URL: {fullUrl}");
                _logger?.LogInformation("GET Request: {Endpoint}", endpoint);

                var response = await _httpClient.GetAsync(endpoint);
                var content = await response.Content.ReadAsStringAsync();

                // Log detallado de respuesta
                System.Diagnostics.Debug.WriteLine($"[GET RESPONSE] Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[GET RESPONSE] Content: {content}");
                _logger?.LogInformation("Response: {StatusCode} - {Content}", response.StatusCode, content);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                    return result ?? new ApiResponse<T> { Success = false, Message = "Response was null" };
                }

                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"HTTP Error: {response.StatusCode} - {content}",
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GET ERROR] {ex.Message}");
                _logger?.LogError(ex, "Error in GET request to {Endpoint}", endpoint);
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Connection error: {ex.Message}"
                };
            }
        }

        // Método genérico POST mejorado con logging detallado
        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var fullUrl = $"{_baseUrl}/{endpoint.TrimStart('/')}";

                // Log detallado para debugging
                System.Diagnostics.Debug.WriteLine($"[POST REQUEST] URL: {fullUrl}");
                System.Diagnostics.Debug.WriteLine($"[POST REQUEST] Data: {json}");
                _logger?.LogInformation("POST Request: {Endpoint} - Data: {Data}", endpoint, json);

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Log detallado de respuesta
                System.Diagnostics.Debug.WriteLine($"[POST RESPONSE] Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[POST RESPONSE] Content: {responseContent}");
                _logger?.LogInformation("Response: {StatusCode} - {Content}", response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);
                    return result ?? new ApiResponse<T> { Success = false, Message = "Response was null" };
                }

                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"HTTP Error: {response.StatusCode} - {responseContent}",
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[POST ERROR] {ex.Message}");
                _logger?.LogError(ex, "Error in POST request to {Endpoint}", endpoint);
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Connection error: {ex.Message}"
                };
            }
        }

        // Test de conectividad mejorado
        public async Task<(bool Success, string Message)> TestConnectionAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[TEST] Probando conexión a: {_baseUrl}");

                var response = await _httpClient.GetAsync("/");
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[TEST] Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[TEST] Content: {content}");

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Conexión exitosa");
                }
                else
                {
                    return (false, $"Error HTTP: {response.StatusCode}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"[TEST ERROR HTTP] {httpEx.Message}");

                if (httpEx.Message.Contains("Cleartext HTTP traffic"))
                {
                    return (false, "Android bloquea HTTP. Configura Network Security Config.");
                }

                return (false, $"Error HTTP: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TEST ERROR] {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }

        // MÉTODOS ESPECÍFICOS DEL NEGOCIO

        // Login con logging detallado
        public async Task<ApiResponse<Usuario>> LoginAsync(string username, string password)
        {
            try
            {
                // Probar conexión primero
                var (connectionSuccess, connectionMessage) = await TestConnectionAsync();
                if (!connectionSuccess)
                {
                    return new ApiResponse<Usuario>
                    {
                        Success = false,
                        Message = $"No hay conexión: {connectionMessage}"
                    };
                }

                // Enviar username y password (no correo)
                var loginData = new { username, password };
                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine($"[LOGIN] Datos enviados: {json}");

                var response = await _httpClient.PostAsync("auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[LOGIN] Respuesta: {response.StatusCode} - {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<Usuario>>(responseContent, _jsonOptions);
                    return result ?? new ApiResponse<Usuario> { Success = false, Message = "Respuesta vacía" };
                }

                return new ApiResponse<Usuario>
                {
                    Success = false,
                    Message = $"Error {response.StatusCode}: {responseContent}"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN ERROR] {ex}");
                return new ApiResponse<Usuario>
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
        }

        // Especialidades
        public async Task<ApiResponse<List<Especialidad>>> ObtenerEspecialidadesAsync()
        {
            return await GetAsync<List<Especialidad>>("api/especialidades");
        }

        // Sucursales
        public async Task<ApiResponse<List<Sucursal>>> ObtenerSucursalesAsync()
        {
            return await GetAsync<List<Sucursal>>("api/sucursales");
        }

        // Médicos
        public async Task<ApiResponse<Medico>> CrearMedicoAsync(Medico medico)
        {
            return await PostAsync<Medico>("api/medicos", medico);
        }

        public async Task<ApiResponse<List<Medico>>> ObtenerMedicosAsync()
        {
            return await GetAsync<List<Medico>>("api/medicos");
        }

        // Pacientes
        public async Task<ApiResponse<Paciente>> BuscarPacientePorCedulaAsync(string cedula)
        {
            return await GetAsync<Paciente>($"api/pacientes/{cedula}");
        }

        public async Task<ApiResponse<Paciente>> CrearPacienteAsync(Paciente paciente)
        {
            return await PostAsync<Paciente>("api/pacientes", paciente);
        }

        // Horarios
        public async Task<ApiResponse<List<Horario>>> ObtenerHorariosDisponiblesAsync(int medicoId, DateTime fecha)
        {
            var fechaStr = fecha.ToString("yyyy-MM-dd");
            return await GetAsync<List<Horario>>($"api/horarios/medico/{medicoId}/disponibles?fecha={fechaStr}");
        }

        // Citas
        public async Task<ApiResponse<Cita>> CrearCitaAsync(Cita cita)
        {
            return await PostAsync<Cita>("api/citas", cita);
        }

        // Método para limpiar recursos
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}