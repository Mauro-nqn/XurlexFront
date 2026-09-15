using IurixBlazor.Pages;
using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace IurixBlazor.Services
{
    public class DomicilioService
    {
        private readonly IApiRequestSender _sender;



        public DomicilioService(IApiRequestSender sender)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _sender = sender;
        }



        public async Task<List<DomicilioDto>> ObtenerTodosAsync()
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, "api/Domicilio");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<DomicilioDto>>() ?? new();
        }

        public async Task<DomicilioDto?> ObtenerPorIdAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/Domicilio/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DomicilioDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<List<DomicilioDto>> ObtenerPorTipoAsync(TipoDomicilio tipo)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/Domicilio?tipo={tipo}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<DomicilioDto>>() ?? new();
        }



        //public async Task CrearAsync(CrearDomicilioDto dto)
        //{
        //    var response = await _httpClient.PostAsJsonAsync("api/Domicilio", dto);
        //    response.EnsureSuccessStatusCode();
        //}


        public async Task CrearAsync(CrearDomicilioDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Post, "api/Domicilio", JsonContent.Create(dto));

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                var error = await LeerMensajeError(response);
                throw new InvalidOperationException(error);
            }

            response.EnsureSuccessStatusCode();
        }

        //public async Task ActualizarAsync(int id, DomicilioDto dto)
        //{
        //    var response = await _httpClient.PatchAsync($"api/Domicilio/{id}", JsonContent.Create(dto));
        //    response.EnsureSuccessStatusCode();
        //}


        public async Task ActualizarAsync(int id, DomicilioDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/Domicilio/{id}", JsonContent.Create(dto));

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                var error = await LeerMensajeError(response);
                throw new InvalidOperationException(error);
            }

            response.EnsureSuccessStatusCode();
        }



        private static async Task<string> LeerMensajeError(HttpResponseMessage response)
        {
            try
            {
                var dto = await response.Content.ReadFromJsonAsync<ErrorRespuestaDto>();
                if (!string.IsNullOrWhiteSpace(dto?.Mensaje))
                    return dto.Mensaje!;
            }
            catch
            {
                // ignoramos errores de parseo
            }

            var texto = await response.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(texto)
                ? "No se pudo completar la operación."
                : texto;
        }

        //public async Task EliminarAsync(int id)
        //{
        //    var response = await _httpClient.DeleteAsync($"api/Domicilio/{id}");
        //    response.EnsureSuccessStatusCode();
        //}


        public async Task EliminarAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/Domicilio/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                // Intentamos leer un JSON con { mensaje = "..."}
                string mensaje;

                try
                {
                    var contenido = await response.Content.ReadFromJsonAsync<ErrorRespuestaDto>();
                    mensaje = contenido?.Mensaje
                              ?? "No se puede eliminar el domicilio porque está asociado a procesos judiciales.";
                }
                catch
                {
                    // Si no es JSON, leemos texto plano
                    mensaje = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(mensaje))
                        mensaje = "No se puede eliminar el domicilio porque está asociado a procesos judiciales.";
                }

                throw new InvalidOperationException(mensaje);
            }

            response.EnsureSuccessStatusCode();
        }

        public class ErrorRespuestaDto
        {
            public string? Mensaje { get; set; }
        }

    }
}
