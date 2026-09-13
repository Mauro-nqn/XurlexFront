using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class AfipAuthService
    {
        private readonly HttpClient _httpClient;

        public AfipAuthService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient = factory.CreateClient("Api");
        }

        /// <summary>
        /// Obtiene la configuración AFIP asociada a un usuario.
        /// </summary>
        //public async Task<AfipAuthDto?> ObtenerPorUsuarioId(int usuarioId)
        //{
        //    return await _httpClient.GetFromJsonAsync<AfipAuthDto>($"api/afipauth/usuario/{usuarioId}");
        //}

        public async Task<AfipAuthDto?> ObtenerPorUsuarioId(int usuarioId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<AfipAuthDto>($"api/Afipauth/usuario/{usuarioId}");
            }
            catch (HttpRequestException ex)
            {
                // Si es 404 devolvemos un DTO vacío
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return new AfipAuthDto { UsuarioId = usuarioId };

                throw; // Otros errores sí los propagamos
            }
        }

        /// <summary>
        /// Guarda o actualiza los datos AFIP de un usuario.
        /// </summary>
        //public async Task GuardarAsync(AfipAuthDto dto)
        //{
        //    var response = await _httpClient.PostAsJsonAsync("api/afipauth/guardar", dto);
        //    response.EnsureSuccessStatusCode();
        //}

        public async Task<AfipAuthDto> GuardarAsync(AfipAuthDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Afipauth/guardar", dto);
            response.EnsureSuccessStatusCode();
            var saved = await response.Content.ReadFromJsonAsync<AfipAuthDto>();
            return saved!;
        }





        /// <summary>
        /// Obtiene token y sign de AFIP (homologación).
        /// </summary>
        //public async Task<(string TA, string Sign, DateTime TokenExpiration)> ObtenerAuthAsync()
        //{
        //    var result = await _httpClient.GetFromJsonAsync<AuthResponseDto>("api/afipauth/auth");
        //    return (result!.TA, result.Sign, result.TokenExpiration);
        //}

        ///// <summary>
        ///// Obtiene token y sign de AFIP (producción).
        ///// </summary>
        //public async Task<(string TA, string Sign, DateTime TokenExpiration)> ObtenerAuthProduccionAsync()
        //{
        //    var result = await _httpClient.GetFromJsonAsync<AuthResponseDto>("api/afipauth/auth-prod");
        //    return (result!.TA, result.Sign, result.TokenExpiration);
        //}



        public async Task<(string TA, string Sign, DateTime TokenExpiration)> ObtenerAuthAsync(int usuarioId)
        {
            var result = await _httpClient.GetFromJsonAsync<AuthResponseDto>($"api/Afipauth/auth/{usuarioId}");
            return (result!.TA, result.Sign, result.TokenExpiration);
        }

        public async Task<(string TA, string Sign, DateTime TokenExpiration)> ObtenerAuthProduccionAsync(int usuarioId)
        {
            var result = await _httpClient.GetFromJsonAsync<AuthResponseDto>($"api/Afipauth/auth-prod/{usuarioId}");
            return (result!.TA, result.Sign, result.TokenExpiration);
        }


        /// <summary>
        /// Consulta la constancia de inscripción de un CUIT en AFIP.
        /// </summary>
        //public async Task<DatosAfipDto> ConsultarConstanciaAsync(string cuit)
        //{
        //    var result = await _httpClient.GetFromJsonAsync<DatosAfipDto>($"api/afipauth/consultar-constancia/{cuit}");
        //    return result!;
        //}


        //public async Task<DatosAfipDto?> ConsultarConstanciaAsync(int usuarioId, string cuit)
        //{
        //    var response = await _httpClient.GetAsync($"api/AfipAuth/consultar-constancia/{usuarioId}/{cuit}");
        //    if (!response.IsSuccessStatusCode)
        //        return null;

        //    return await response.Content.ReadFromJsonAsync<DatosAfipDto>();
        //}



        public async Task<(DatosAfipDto? datos, string? error)> ConsultarConstanciaAsync(int usuarioId, string cuit)
        {
            var response = await _httpClient.GetAsync($"api/AfipAuth/consultar-constancia/{usuarioId}/{cuit}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    // Intentar deserializar el objeto de error que envía el backend
                    var errorObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);

                    if (errorObj != null && errorObj.ContainsKey("error"))
                        return (null, errorObj["error"]);
                    else
                        return (null, $"No se pudo consultar AFIP. Código: {response.StatusCode}");
                }
                catch
                {
                    // Si no se puede parsear el JSON, devolver texto plano
                    return (null, "No se pudo consultar en AFIP. Verifique certificados en producción.");
                }
            }

            // ✅ Si todo OK, deserializamos los datos
            var datos = await response.Content.ReadFromJsonAsync<DatosAfipDto>();
            return (datos, null);
        }


        // Consulta global si el usuario no tiene certificado
        //public async Task<DatosAfipDto?> ConsultarConstanciaGlobalAsync(string cuit)
        //{
        //    var response = await _httpClient.GetAsync($"api/AfipAuth/consultar-constancia-global/{cuit}");
        //    if (!response.IsSuccessStatusCode)
        //        return null;

        //    return await response.Content.ReadFromJsonAsync<DatosAfipDto>();
        //}

        public async Task<(DatosAfipDto? datos, string? error)> ConsultarConstanciaGlobalAsync(string cuit)
{
    var response = await _httpClient.GetAsync($"api/AfipAuth/consultar-constancia-global/{cuit}");

    if (!response.IsSuccessStatusCode)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        try
        {
            var errorObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent);
            return (null, errorObj?["error"] ?? "Error desconocido al consultar AFIP.");
        }
        catch
        {
            return (null, "No se pudo consultar en AFIP.");
        }
    }

    var datos = await response.Content.ReadFromJsonAsync<DatosAfipDto>();
    return (datos, null);
}
    }

}
