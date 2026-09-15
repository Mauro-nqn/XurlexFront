using IurixBlazor.Pages;
using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class SecretariaService
    {
        private readonly IApiRequestSender _sender;

        public SecretariaService(IApiRequestSender sender)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _sender = sender;
        }

        public async Task<List<SecretariaDto>> ObtenerTodosAsync()
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, "api/Secretaria");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<SecretariaDto>>() ?? new();
        }

        public async Task<SecretariaDto?> ObtenerPorIdAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/Secretaria/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SecretariaDto>();
        }

        public async Task<List<SecretariaDto>> ObtenerPorJuzgadoAsync(int juzgadoId)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/Secretaria/por-juzgado/{juzgadoId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new List<SecretariaDto>(); // Si no hay resultados, devolvemos lista vacía

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<SecretariaDto>>() ?? new();
        }

        public async Task CrearAsync(CrearSecretariaDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Post, "api/Secretaria", JsonContent.Create(dto));
        }

        public async Task ActualizarAsync(int id, CrearSecretariaDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/Secretaria/{id}", JsonContent.Create(dto));
        }

        public async Task EliminarAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/Secretaria/{id}");
        }
    }
}
