using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class CaracterIntervencionService
    {
        private readonly IApiRequestSender _sender;

        public CaracterIntervencionService(IApiRequestSender sender)
        {
            _sender = sender;
        }

        public async Task<List<CaracterIntervencionDto>> ObtenerTodosAsync()
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, "api/CaracterIntervencion");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<CaracterIntervencionDto>>() ?? new();
        }

        public async Task<CaracterIntervencionDto?> ObtenerPorIdAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/CaracterIntervencion/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CaracterIntervencionDto>();
        }

        public async Task CrearAsync(CaracterIntervencionDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Post, "api/CaracterIntervencion", JsonContent.Create(dto));
        }

        public async Task ActualizarAsync(int id, CaracterIntervencionDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/CaracterIntervencion/{id}", JsonContent.Create(dto));
        }

        public async Task EliminarAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/CaracterIntervencion/{id}");
        }
    }
}
