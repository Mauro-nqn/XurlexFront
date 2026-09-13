using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace IurixBlazor.Services
{
    public class PersonaService
    {
        private readonly HttpClient _httpClient;

        public PersonaService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient = factory.CreateClient("Api");
        }

        //  Personas
        public async Task<List<PersonaDto>> ObtenerPersonasAsync()
        {
            
            var response = await _httpClient.GetAsync("api/persona");
            if (!response.IsSuccessStatusCode)
                return new List<PersonaDto>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PersonaDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<PersonaDto>();
        }

        public async Task<PersonaDto?> ObtenerPersonaPorIdAsync(int id, CancellationToken ct = default)
        {
            // Ajustá el path si tu controlador se llama distinto (e.g. api/persona)
            var response = await _httpClient.GetAsync($"api/persona/{id}", ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null; // no existe

            response.EnsureSuccessStatusCode(); // lanza si no es 2xx

            var dto = await response.Content.ReadFromJsonAsync<PersonaDto>(cancellationToken: ct);
            return dto;
        }

        //public async Task<bool> CrearPersonaAsync(PersonaDto dto)
        //{
        //    // Serializar el DTO a JSON
        //    var json = System.Text.Json.JsonSerializer.Serialize(dto, new JsonSerializerOptions
        //    {
        //        WriteIndented = true // Formato legible
        //    });

        //    // Mostrar en consola o salida de depuración
        //    Console.WriteLine("📤 JSON a enviar: " + json);
        //    System.Diagnostics.Debug.WriteLine("📤 JSON a enviar: " + json);

        //    var response = await _httpClient.PostAsJsonAsync("api/persona", dto);
        //    return response.IsSuccessStatusCode;
        //}


        public async Task<List<PersonaDto>> BuscarAsync(string query, int take = 15)
        {
            query = (query ?? "").Trim();
            if (query.Length < 2) return new();

            var url = $"api/persona/buscar?q={Uri.EscapeDataString(query)}&take={take}";
            var res = await _httpClient.GetFromJsonAsync<List<PersonaDto>>(url);
            return res ?? new();
        }




        public async Task CrearPersonaAsync(PersonaDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/persona", dto);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorRespuestaDto>();
                var mensaje = error?.Mensaje ?? "No se pudo crear la persona (conflicto de datos).";
                throw new InvalidOperationException(mensaje);
            }

            response.EnsureSuccessStatusCode();
        }

    

        public async Task<bool> ActualizarPersonaAsync(int id, PersonaPatchDto patch)
        {
            var json = JsonSerializer.Serialize(patch);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync($"api/persona/{id}", content);
            return response.IsSuccessStatusCode;
        }

        //public async Task<bool> EliminarPersonaAsync(int id)
        //{
        //    var response = await _httpClient.DeleteAsync($"api/persona/{id}");
        //    return response.IsSuccessStatusCode;
        //}

        public async Task EliminarPersonaAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/persona/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                // Leer el mensaje del backend
                var contenido = await response.Content.ReadFromJsonAsync<ErrorRespuestaDto>();
                var mensaje = contenido?.Mensaje
                              ?? "No se puede eliminar la persona porque está vinculada a otros registros.";
                throw new InvalidOperationException(mensaje);
            }

            response.EnsureSuccessStatusCode();
        }

        public class ErrorRespuestaDto
        {
            public string? Mensaje { get; set; }
        }

        // ✅ Clasificación Personas
        public async Task<List<ClasificacionPersonaDto>> ObtenerClasificacionesAsync()
        {
            var response = await _httpClient.GetAsync("api/ClasificacionPersona");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ClasificacionPersonaDto>>() ?? new();
        }

        public async Task CrearClasificacionAsync(ClasificacionPersonaDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/ClasificacionPersona", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task ActualizarClasificacionAsync(int id, ClasificacionPersonaDto dto)
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/ClasificacionPersona/{id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task EliminarClasificacionAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/ClasificacionPersona/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
