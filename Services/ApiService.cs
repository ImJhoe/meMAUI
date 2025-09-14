using ClinicaApp.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ClinicaApp.Converters;

namespace ClinicaApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService>? _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        // Constructor del ApiService
        public ApiService(HttpClient httpClient, ILogger<ApiService>? logger = null)
        {
            _httpClient = httpClient;
            _logger = logger;

            // ✅ CONFIGURACIÓN CORRECTA para tu API PHP
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, // Importante para PHP
                WriteIndented = true
            };
        }

        // Test de conexión
        public async Task<(bool Success, string Message)> TestConnectionAsync()
        {
            try
            {
                // Verificar conectividad de red
                var networkAccess = Connectivity.Current.NetworkAccess;
                System.Diagnostics.Debug.WriteLine($"[NETWORK] Estado: {networkAccess}");

                if (networkAccess != NetworkAccess.Internet)
                {
                    return (false, "Sin conexión a internet");
                }

                System.Diagnostics.Debug.WriteLine($"[TEST] Probando conexión...");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var response = await _httpClient.GetAsync("", cts.Token);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[TEST] Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[TEST] Content: {content}");

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Conexión exitosa");
                }
                else
                {
                    return (false, $"HTTP {response.StatusCode}: {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"[TEST HTTP ERROR] {httpEx.Message}");

                if (httpEx.Message.Contains("Cleartext HTTP"))
                {
                    return (false, "Android bloquea HTTP. Verifica network_security_config.xml");
                }
                if (httpEx.Message.Contains("No address associated"))
                {
                    return (false, "No se puede resolver la dirección IP. Verifica la red WiFi");
                }
                if (httpEx.Message.Contains("Connection refused"))
                {
                    return (false, "Conexión rechazada. Verifica que el servidor esté ejecutándose");
                }

                return (false, $"Error HTTP: {httpEx.Message}");
            }
            catch (TaskCanceledException)
            {
                return (false, "Timeout - El servidor no responde en 10 segundos");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TEST ERROR] {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }

        // En ApiService.cs, reemplaza el método LoginAsync
        // En ApiService.cs, reemplaza el método LoginAsync
        public async Task<ApiResponse<Usuario>> LoginAsync(string username, string password)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN] Iniciando login para: {username}");

                // Preparar datos
                var loginData = new { username, password };
                var json = JsonSerializer.Serialize(loginData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine($"[LOGIN] Datos enviados: {json}");

                // Hacer petición
                var response = await _httpClient.PostAsync("auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[LOGIN] Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[LOGIN] Response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    // ⭐ CAMBIO IMPORTANTE: Deserializar como LoginData primero
                    var loginResponse = JsonSerializer.Deserialize<ApiResponse<LoginData>>(responseContent, _jsonOptions);

                    if (loginResponse?.Success == true && loginResponse.Data?.User != null)
                    {
                        var usuario = loginResponse.Data.User;
                        System.Diagnostics.Debug.WriteLine($"[LOGIN] Login exitoso para: {usuario.NombreCompleto} - Rol: {usuario.NombreRol}");

                        // Devolver el usuario dentro de ApiResponse<Usuario>
                        return new ApiResponse<Usuario>
                        {
                            Success = true,
                            Message = loginResponse.Message,
                            Data = usuario
                        };
                    }
                    else
                    {
                        return new ApiResponse<Usuario>
                        {
                            Success = false,
                            Message = loginResponse?.Message ?? "Error desconocido en login"
                        };
                    }
                }
                else
                {
                    return new ApiResponse<Usuario>
                    {
                        Success = false,
                        Message = $"Error HTTP {response.StatusCode}: {responseContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN ERROR] {ex}");
                return new ApiResponse<Usuario>
                {
                    Success = false,
                    Message = $"Error inesperado: {ex.Message}"
                };
            }
        }

        // Métodos genéricos GET y POST
        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[GET] {endpoint}");

                var response = await _httpClient.GetAsync(endpoint);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[GET] Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                    return result ?? new ApiResponse<T> { Success = false, Message = "Response was null" };
                }

                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"HTTP Error: {response.StatusCode}",
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GET ERROR] {ex.Message}");
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Connection error: {ex.Message}"
                };
            }
        }

        // ACTUALIZAR TU MÉTODO PostAsync EN ApiService.cs PARA VER EL ERROR ESPECÍFICO

        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine($"[POST] {endpoint} - Data: {json}");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[POST] Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[POST] Response: {responseContent}"); // ✅ IMPORTANTE: Ver el error del servidor

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);
                    return result ?? new ApiResponse<T> { Success = false, Message = "Response was null" };
                }
                else
                {
                    // ✅ MOSTRAR EL ERROR ESPECÍFICO DEL SERVIDOR
                    return new ApiResponse<T>
                    {
                        Success = false,
                        Message = $"HTTP {response.StatusCode}: {responseContent}",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[POST ERROR] {ex.Message}");
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Connection error: {ex.Message}"
                };
            }
        }

        // Métodos específicos del negocio
        // Métodos para especialidades
        public async Task<ApiResponse<List<Especialidad>>> ObtenerEspecialidadesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[API] Obteniendo especialidades");
                return await GetAsync<List<Especialidad>>("api/especialidades");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API ERROR] Especialidades: {ex.Message}");
                return new ApiResponse<List<Especialidad>>
                {
                    Success = false,
                    Message = $"Error obteniendo especialidades: {ex.Message}"
                };
            }
        }

        // Métodos para sucursales
        public async Task<ApiResponse<List<Sucursal>>> ObtenerSucursalesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[API] Obteniendo sucursales");
                return await GetAsync<List<Sucursal>>("api/sucursales");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API ERROR] Sucursales: {ex.Message}");
                return new ApiResponse<List<Sucursal>>
                {
                    Success = false,
                    Message = $"Error obteniendo sucursales: {ex.Message}"
                };
            }
        }

        // Métodos para médicos
        public async Task<ApiResponse<Medico>> CrearMedicoAsync(Medico medico)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[API] Creando médico: {medico.NombreCompleto}");
                System.Diagnostics.Debug.WriteLine($"[API] Especialidad: {medico.IdEspecialidad}");
                System.Diagnostics.Debug.WriteLine($"[API] Cédula: {medico.Cedula}");

                // ✅ CORREGIR: Enviar solo los campos que necesita la API
                var datosParaAPI = new
                {
                    nombres = medico.Nombres,
                    apellidos = medico.Apellidos,
                    cedula = medico.Cedula,
                    correo = medico.Correo,
                    contrasena = medico.Contrasena,
                    sexo = medico.Sexo,
                    nacionalidad = medico.Nacionalidad,
                    titulo_profesional = medico.TituloProfesional, // Usar snake_case
                    numero_colegio = medico.NumeroColegio,          // Usar snake_case
                    id_especialidad = medico.IdEspecialidad,
                    id_sucursal = medico.IdSucursal
                };

                var json = JsonSerializer.Serialize(datosParaAPI, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine($"[API] JSON enviado: {json}");

                var response = await _httpClient.PostAsync("api/medicos", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[API] Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[API] Response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<Medico>>(responseContent, _jsonOptions);
                    return result ?? new ApiResponse<Medico> { Success = false, Message = "Respuesta vacía" };
                }
                else
                {
                    // ✅ AGREGAR: Mostrar el error específico del servidor
                    return new ApiResponse<Medico>
                    {
                        Success = false,
                        Message = $"Error {response.StatusCode}: {responseContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API ERROR] Crear médico: {ex}");
                return new ApiResponse<Medico>
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
        }

        // Agregar este método a tu ApiService.cs
        public async Task<List<Medico>> ObtenerMedicosAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[API] Obteniendo médicos");
                System.Diagnostics.Debug.WriteLine("[GET] api/medicos");

                var response = await _httpClient.GetAsync("api/medicos");
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[GET] Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[GET] Content: {content}");

                if (response.IsSuccessStatusCode)
                {
                    // ✅ USAR OPTIONS CON CONVERTER PARA MANEJAR CEDULA
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new StringOrNumberConverter() }
                    };

                    var result = JsonSerializer.Deserialize<ApiResponse<List<Medico>>>(content, options);
                    return result?.Data ?? new List<Medico>();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[GET ERROR] {response.StatusCode}: {content}");
                    return new List<Medico>();
                }
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GET ERROR] JSON Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[GET ERROR] Path: {ex.Path}");

                // ✅ DEVOLVER LISTA VACÍA EN LUGAR DE LANZAR EXCEPCIÓN
                return new List<Medico>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GET ERROR] {ex.Message}");
                return new List<Medico>();
            }
        }
        // Métodos para horarios - USAR TU MODELO HORARIO
        // ✅ CORREGIR: Método para asignar horarios
        public async Task<ApiResponse<bool>> AsignarHorariosAsync(int idMedico, List<Horario> horarios)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[API] Asignando {horarios.Count} horarios al médico {idMedico}");

                // ✅ CAMBIO CRÍTICO: Enviar horarios uno por uno
                var resultados = new List<bool>();

                foreach (var horario in horarios)
                {
                    // Preparar datos en el formato que espera tu API PHP
                    var data = new
                    {
                        id_medico = idMedico,
                        id_sucursal = horario.IdSucursal,
                        dia_semana = horario.DiaSemana,
                        hora_inicio = horario.HoraInicio,
                        hora_fin = horario.HoraFin,
                        duracion_cita = horario.IntervaloMinutos  // ✅ CORREGIR: Usar IntervaloMinutos
                    };

                    var json = JsonSerializer.Serialize(data, _jsonOptions);
                    System.Diagnostics.Debug.WriteLine($"[API] Enviando horario: {json}");

                    var response = await PostAsync<object>("api/horarios", data);
                    resultados.Add(response.Success);

                    if (!response.Success)
                    {
                        System.Diagnostics.Debug.WriteLine($"[API ERROR] Horario falló: {response.Message}");
                        return new ApiResponse<bool>
                        {
                            Success = false,
                            Message = $"Error asignando horario: {response.Message}"
                        };
                    }
                }

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = $"Se asignaron {resultados.Count} horarios exitosamente"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API ERROR] Asignar horarios: {ex.Message}");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error asignando horarios: {ex.Message}"
                };
            }
        }
        // Método mejorado para obtener horarios de un médico
        public async Task<List<Horario>> ObtenerHorariosMedicoAsync(int idMedico)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[API] Obteniendo horarios para médico {idMedico}");

                var response = await _httpClient.GetAsync($"api/horarios/medico/{idMedico}");
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[API] Status horarios: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[API] Content horarios: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonDocument = JsonDocument.Parse(content);
                    var root = jsonDocument.RootElement;

                    if (root.TryGetProperty("success", out var successProperty) &&
                        successProperty.GetBoolean() &&
                        root.TryGetProperty("data", out var dataProperty))
                    {
                        var horarios = new List<Horario>();

                        foreach (var item in dataProperty.EnumerateArray())
                        {
                            try
                            {
                                var horario = new Horario
                                {
                                    IdHorario = item.TryGetProperty("id_horario", out var idProp) ? idProp.GetInt32() : 0,
                                    IdMedico = item.TryGetProperty("id_medico", out var medicoIdProp) ? medicoIdProp.GetInt32() : idMedico,
                                    IdSucursal = item.TryGetProperty("id_sucursal", out var sucursalProp) ? sucursalProp.GetInt32() : 0,
                                    DiaSemana = item.TryGetProperty("dia_semana", out var diaProp) ? diaProp.GetInt32() : 0,
                                    HoraInicio = item.TryGetProperty("hora_inicio", out var inicioP) ? inicioP.GetString() ?? "" : "",
                                    HoraFin = item.TryGetProperty("hora_fin", out var finProp) ? finProp.GetString() ?? "" : "",
                                    IntervaloMinutos = item.TryGetProperty("duracion_cita", out var durProp) ? durProp.GetInt32() : 30,
                                    NombreSucursal = item.TryGetProperty("nombre_sucursal", out var sucNomProp) ? sucNomProp.GetString() ?? "" : "",
                                    Activo = true
                                };

                                horarios.Add(horario);
                                System.Diagnostics.Debug.WriteLine($"[API] Horario agregado: {horario.HorarioTexto}");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[API] Error procesando horario: {ex.Message}");
                            }
                        }

                        return horarios;
                    }
                }

                return new List<Horario>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API ERROR] Error obteniendo horarios: {ex.Message}");
                return new List<Horario>();
            }
        }
        // Método para actualizar un horario
        public async Task<ApiResponse<bool>> ActualizarHorarioAsync(Horario horario)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[API] Actualizando horario {horario.IdHorario}");

                var data = new
                {
                    dia_semana = horario.DiaSemana,
                    hora_inicio = horario.HoraInicio,
                    hora_fin = horario.HoraFin,
                    duracion_cita = horario.IntervaloMinutos,
                    id_sucursal = horario.IdSucursal
                };

                // ✅ USAR PUT en lugar de POST
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/horarios/{horario.IdHorario}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[API] Status actualizar: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Horario actualizado correctamente"
                    };
                }
                else
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Error del servidor: {response.StatusCode} - {responseContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API ERROR] Error actualizando horario: {ex.Message}");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error actualizando horario: {ex.Message}"
                };
            }
        }
        // Método para eliminar un horario
        public async Task<ApiResponse<bool>> EliminarHorarioAsync(int idHorario)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[API] Eliminando horario {idHorario}");

                var response = await _httpClient.DeleteAsync($"api/horarios/{idHorario}");
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[API] Status eliminar: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Horario eliminado correctamente"
                    };
                }
                else
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Error del servidor: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API ERROR] Error eliminando horario: {ex.Message}");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error eliminando horario: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<Paciente>> BuscarPacientePorCedulaAsync(string cedula)
        {
            return await GetAsync<Paciente>($"api/pacientes/{cedula}");
        }

        public async Task<ApiResponse<Paciente>> CrearPacienteAsync(Paciente paciente)
        {
            return await PostAsync<Paciente>("api/pacientes", paciente);
        }

        public async Task<ApiResponse<List<Horario>>> ObtenerHorariosDisponiblesAsync(int medicoId, DateTime fecha)
        {
            var fechaStr = fecha.ToString("yyyy-MM-dd");
            return await GetAsync<List<Horario>>($"api/horarios/medico/{medicoId}/disponibles?fecha={fechaStr}");
        }

        public async Task<ApiResponse<Cita>> CrearCitaAsync(Cita cita)
        {
            return await PostAsync<Cita>("api/citas", cita);
        }
    }
}