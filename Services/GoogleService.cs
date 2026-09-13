using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace IurixBlazor.Services
{
    public class GoogleService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime? _js;




        public GoogleService(IHttpClientFactory factory, ConfigService config, IJSRuntime js)
        {
            // Configurar base URL del backend
            //httpClient.BaseAddress = new Uri($"http://{config.IpServidor}:{config.Puerto}/");
            //_httpClient = httpClient;
            _httpClient = factory.CreateClient("Api");

            _js = js;

             //✅ Obtener token desde LocalStorage y agregarlo al header
            js.InvokeAsync<string>("localStorage.getItem", "token").AsTask().ContinueWith(task =>
            {
                var token = task.Result;
                if (!string.IsNullOrEmpty(token))
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            });


        }

        

            // Llamar una sola vez al iniciar la app o antes de las llamadas
            public async Task InitAsync()
            {
                var token = await _js.InvokeAsync<string>("localStorage.getItem", "token");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }


        /// <summary>
        /// Consulta el estado del token de Google para un usuario.
        /// </summary>
        public async Task<GoogleAuthDto?> ObtenerEstadoGoogleAsync(int usuarioId)
        {
            var response = await _httpClient.GetAsync($"api/google/estado?usuarioId={usuarioId}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<GoogleAuthDto>();
        }

        /// <summary>
        /// Obtiene el email de Google vinculado.
        /// </summary>
        public async Task<GoogleAuthDto?> ObtenerEmailGoogleAsync(int usuarioId)
        {
            var response = await _httpClient.GetAsync($"api/google/estado?usuarioId={usuarioId}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<GoogleAuthDto>();
        }

        /// <summary>
        /// Refresca el token de Google.
        /// </summary>
        //public async Task<bool> RefrescarTokenAsync(int usuarioId)
        //{
        //    var response = await _httpClient.PostAsync($"api/google/refresh-token?usuarioId={usuarioId}", null);
        //    return response.IsSuccessStatusCode;
        //}

        public async Task<(bool ok, string raw)> RefrescarTokenDebugAsync(int usuarioId)
        {
            var response = await _httpClient.PostAsync($"api/google/refresh-token?usuarioId={usuarioId}", null);
            var raw = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, raw);
        }






        //METODOS EN TEST DE GOOGLE PARA CORROBORAR SI FUNCIONAN EN API


        /// <summary>
        /// Envía un correo electrónico usando Gmail API.
        /// </summary>
        public async Task<string> EnviarCorreoAsync(int usuarioId, CrearEmailDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/google/gmail/enviar?usuarioId={usuarioId}", dto);
            return await response.Content.ReadAsStringAsync();
        }


        /// <summary>
        /// Crea un evento en Google Calendar.
        /// </summary>
        public async Task<string> CrearEventoAsync(int usuarioId, CrearEventoDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/google/calendario/crear?usuarioId={usuarioId}", dto);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Sube un archivo a Google Drive.
        /// </summary>
        public async Task<string> SubirArchivoAsync(int usuarioId, Stream archivo, string nombreArchivo)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(archivo);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "archivo", nombreArchivo);

            var response = await _httpClient.PostAsync($"api/google/drive/subir?usuarioId={usuarioId}", content);
            return await response.Content.ReadAsStringAsync();
        }









        public async Task<GmailSendResultDto?> EnviarCorreoConAdjuntoAsync(int usuarioId, CrearEmailConAdjuntoDto dto)
        {
        // ⚠️ Verificá que esta ruta coincida con tu controller real.
        // Si tu controller se llama GoogleAuthController => "/api/googleauth/..."
        var resp = await _httpClient.PostAsJsonAsync($"api/google/{usuarioId}/email-adjunto", dto);

        var content = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            return new GmailSendResultDto { Ok = false };

        return JsonSerializer.Deserialize<GmailSendResultDto>(
            content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
    }







        public async Task<GoogleSyncResultDto> SincronizarAgendaAsync(int agendaId, int usuarioId)
        {
            var resp = await _httpClient.PostAsJsonAsync(
                $"api/agenda/{agendaId}/google/sync?usuarioId={usuarioId}", new { });

            if (resp.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                var payload = await resp.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                var detail = payload != null && payload.TryGetValue("detail", out var d) ? d : "Debes volver a vincular Google.";
                throw new InvalidOperationException($"GoogleAuthConflict: {detail}");
            }

            resp.EnsureSuccessStatusCode();

            var data = await resp.Content.ReadFromJsonAsync<GoogleSyncResultDto>();
            if (data is null) throw new InvalidOperationException("Respuesta vacía de sincronización.");
            return data;
        }

       


        public async Task<bool> EliminarEventoAgendaAsync(int agendaId, int usuarioId)
        {
            var resp = await _httpClient.DeleteAsync($"api/agenda/{agendaId}/google?usuarioId={usuarioId}");
            if (resp.IsSuccessStatusCode) return true;

            // Si aún no cambiaste el backend, tolerá 404 por idempotencia
            if (resp.StatusCode == HttpStatusCode.NotFound) return true;

            var body = await resp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"EliminarEventoAgendaAsync: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{body}");
        }







        //public async Task<bool> ValidarYRefrescarTokenAsync(int usuarioId)
        //{
        //    var estado = await ObtenerEstadoGoogleAsync(usuarioId);
        //    if (estado == null) return false;

        //    if (estado.Expira <= DateTime.UtcNow)
        //    {
        //        return await RefrescarTokenAsync(usuarioId);
        //    }
        //    return true;
        //}


        //Validar y refrescar con margen de 5 minutos

        //public async Task<bool> ValidarYRefrescarTokenAsync(int usuarioId)
        //{
        //    var estado = await ObtenerEstadoGoogleAsync(usuarioId);
        //    if (estado == null) return false;

        //    var skew = TimeSpan.FromMinutes(5);

        //    // si no viene Expira, forzá refresh
        //    if (estado.Expira == null || estado.Expira <= DateTimeOffset.UtcNow.Add(skew))
        //    {
        //        //return await RefrescarTokenAsync(usuarioId);
        //        return await RefrescarTokenAsync(usuarioId);
        //    }
        //    return true;
        //}


        public async Task<bool> ValidarYRefrescarTokenAsync(int usuarioId)
        {
            var estado = await ObtenerEstadoGoogleAsync(usuarioId);
            if (estado == null) return false;

            var skew = TimeSpan.FromMinutes(5);

            if (estado.Expira == null || estado.Expira <= DateTimeOffset.UtcNow.Add(skew))
            {
                var (ok, raw) = await RefrescarTokenDebugAsync(usuarioId);
                if (!ok)
                {
                    //Console.WriteLine($"[GOOGLE REFRESH FAIL] usuarioId={usuarioId} raw={raw}");
                    await _js.InvokeVoidAsync("console.error", $"[GOOGLE REFRESH FAIL] {raw}");
                }
                return ok;
            }

            return true;
        }




        public void ConfigurarToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}

