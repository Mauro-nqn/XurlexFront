    using IurixBlazor.Shared.Config;
    using IurixBlazor.Shared.Dtos;
    using Microsoft.JSInterop;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text;
    using System.Text.Json;
    using static System.Net.WebRequestMethods;

    namespace IurixBlazor.Shared.Services
    {
        public class UsuarioService
        {
            private readonly HttpClient _httpClient;
            private readonly IJSRuntime _js;
            //private readonly ConfigService _config;

            //public UsuarioService(ConfigService config, IJSRuntime js)
            //{
            //    _config = config;
            //    _js = js;

            //    // Configuramos el HttpClient con la URL dinámica desde el config.ini
            //    var handler = new HttpClientHandler { AllowAutoRedirect = false };
            //    _httpClient = new HttpClient(handler)
            //    {
            //        BaseAddress = new Uri(_config.ServidorBackendUrl)
            //    };
            //}


        public UsuarioService(IHttpClientFactory factory, IJSRuntime js)
        {
            _js = js;
            _httpClient = factory.CreateClient("Api");

            //_httpClient = http ?? throw new ArgumentNullException(nameof(http));
            //if (_httpClient.BaseAddress is null)
            //    throw new InvalidOperationException("AgendaService: BaseAddress no está configurada.");


        }


        // 🔑 Agrega token a cada request
        //private async Task AgregarTokenAsync()
        //    {
        //        var token = await _js.InvokeAsync<string>("localStorage.getItem", "token");
        //        if (!string.IsNullOrEmpty(token))
        //            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //    }


        //private async Task AgregarTokenAsync()
        //{
        //    var token = await _js.InvokeAsync<string>("localStorage.getItem", "token");
        //    Console.WriteLine($"🔑 Token (primeros 20): {token?.Substring(0, Math.Min(20, token.Length))}...");
        //    if (!string.IsNullOrWhiteSpace(token))
        //        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //}

        // 🟢 Obtener lista de usuarios
        public async Task<List<UsuarioDto>> ObtenerUsuariosAsync()
            {
                //await AgregarTokenAsync();
                var response = await _httpClient.GetAsync("api/usuarios");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<UsuarioDto>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<UsuarioDto>();
                }
                return new List<UsuarioDto>();
            }

            // 🟢 Obtener usuario por ID
            public async Task<UsuarioDto?> ObtenerUsuarioPorIdAsync(int id)
            {
                //await AgregarTokenAsync();
                var response = await _httpClient.GetAsync($"api/usuarios/{id}");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<UsuarioDto>();
                return null;
            }

            // 🟢 Crear usuario
            public async Task<bool> CrearUsuarioAsync(UsuarioDto dto)
            {
                //await AgregarTokenAsync();
                var response = await _httpClient.PostAsJsonAsync("api/usuarios", dto);
                return response.IsSuccessStatusCode;
            }

            // 🟢 Actualizar usuario (PATCH)
            public async Task<bool> ActualizarUsuarioAsync(UsuarioDto dto)
            {
                //await AgregarTokenAsync();
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Patch, $"api/usuarios/{dto.Id}")
                {
                    Content = content
                };

            //var response = await _httpClient.SendAsync(request);
            //return response.IsSuccessStatusCode;
            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"PATCH /api/usuarios/{dto.Id} => {(int)response.StatusCode} {response.ReasonPhrase}\n{body}");
            return response.IsSuccessStatusCode;
        }

            // 🟢 Eliminar usuario
            public async Task<bool> EliminarUsuarioAsync(int id)
            {
                //await AgregarTokenAsync();
                var response = await _httpClient.DeleteAsync($"api/usuarios/{id}");
                return response.IsSuccessStatusCode;
            }

            // 🟢 Obtener roles
            public async Task<List<RolDto>> ObtenerRolesAsync()
            {
                //await AgregarTokenAsync();
                var response = await _httpClient.GetAsync("api/roles");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<RolDto>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<RolDto>();
                }
                return new List<RolDto>();
            }

            //  Google: URL de login
            //public async Task<string> ObtenerUrlLoginGoogleAsync(int usuarioId)
            //{
            //    var response = await _httpClient.GetAsync($"api/google/login?usuarioId={usuarioId}");
            //    if (response.StatusCode == HttpStatusCode.Redirect)
            //        return response.Headers.Location?.ToString() ?? "";
            //    return "";


            //}

        public class GoogleAuthResponse
        {
            public string? Url { get; set; }
        }

        public async Task<string> ObtenerUrlLoginGoogleAsync(int usuarioId)
        {
            var response = await _httpClient.GetFromJsonAsync<GoogleAuthResponse>($"api/google/login?usuarioId={usuarioId}");
            return response?.Url ?? "";
        }

        //  Google: Estado de autenticación
        public async Task<GoogleAuthDto?> ObtenerEstadoGoogleAsync(int usuarioId)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"api/google/estado?usuarioId={usuarioId}");
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<GoogleAuthDto>(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error al obtener estado Google: {ex.Message}");
                    return null;
                }
            }


            public async Task DesvincularGoogleAsync(int usuarioId)
            {
                var resp = await _httpClient.PostAsync($"api/google/desvincular?usuarioId={usuarioId}", content: null);
                resp.EnsureSuccessStatusCode();
            }






        // --- Dextra: listar ---
        public async Task<List<DextraCredListItemDto>> ListarCredsAsync(int userId, CancellationToken ct = default)
        {
            var url = $"api/dextraCreds/{userId}/dextra";
            var res = await _httpClient.GetAsync(url, ct);
            res.EnsureSuccessStatusCode();
            var data = await res.Content.ReadFromJsonAsync<List<DextraCredListItemDto>>(cancellationToken: ct);
            return data ?? new();
        }

        // --- Dextra: crear ---
        public async Task<int> CrearCredAsync(int userId, DextraCreateRequestDto dto, CancellationToken ct = default)
        {
            var url = $"api/dextraCreds/{userId}/dextra";
            var res = await _httpClient.PostAsJsonAsync(url, dto, ct);
            res.EnsureSuccessStatusCode();
            // backend devuelve { mensaje, id }
            var obj = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>(cancellationToken: ct);
            if (obj != null && obj.TryGetValue("id", out var idObj) && int.TryParse(idObj?.ToString(), out var id))
                return id;
            return 0;
        }



        // --- Dextra: actualizar (patch) ---
        public async Task<bool> ActualizarCredAsync(int userId, int credId, object patchDto, CancellationToken ct = default)
        {
            var url = $"api/dextraCreds/{userId}/dextra/{credId}";
            var req = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = JsonContent.Create(patchDto)
            };
            var res = await _httpClient.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode) return false;
            return true;
        }

        // Helper para setear default:
        public Task<bool> SetDefaultCredAsync(int userId, int credId, CancellationToken ct = default)
            => ActualizarCredAsync(userId, credId, new DextraCredUpsertDto { IsDefault = true }, ct);

        // --- Dextra: eliminar ---
        public async Task<bool> EliminarCredAsync(int userId, int credId, CancellationToken ct = default)
        {
            var url = $"api/dextraCreds/{userId}/dextra/{credId}";
            var res = await _httpClient.DeleteAsync(url, ct);
            return res.IsSuccessStatusCode;
        }

        // --- Dextra: verificar ---
        //public async Task<DextraVerifyResultDto?> VerificarCredAsync(int userId, int credId, CancellationToken ct = default)
        //{
        //    var url = $"api/dextraCreds/{userId}/dextra/{credId}/verify";
        //    var res = await _httpClient.PostAsync(url, content: null, ct);
        //    if (!res.IsSuccessStatusCode) return null;
        //    var dto = await res.Content.ReadFromJsonAsync<DextraVerifyResultDto>(cancellationToken: ct);
        //    return dto;
        //}

        public async Task<DextraVerifyResultDto?> VerificarCredAsync(
    int userId,
    int credId,
    CancellationToken ct = default)
        {
            var url = $"api/dextraCreds/{userId}/dextra/{credId}/verify";
            var res = await _httpClient.PostAsync(url, content: null, ct);

            if (!res.IsSuccessStatusCode)
            {
                var error = await res.Content.ReadAsStringAsync(ct);
                throw new Exception($"No se pudo verificar la credencial. HTTP {(int)res.StatusCode}: {error}");
            }

            return await res.Content.ReadFromJsonAsync<DextraVerifyResultDto>(cancellationToken: ct);
        }


    }
}


