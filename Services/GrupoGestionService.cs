using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;


public class GrupoGestionService
{
    private readonly IApiRequestSender _sender;

    public GrupoGestionService(IApiRequestSender sender)
    {
        _sender = sender;
    }

    public async Task<List<GrupoGestionDto>> ObtenerTodosAsync()
    {
        using var response = await _sender.SendAsync(HttpMethod.Get, "api/GrupoGestion");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<GrupoGestionDto>>() ?? new();
    }

    public async Task<GrupoGestionDto?> ObtenerPorIdAsync(int id)
    {
        using var response = await _sender.SendAsync(HttpMethod.Get, $"api/GrupoGestion/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GrupoGestionDto>();
    }

    public async Task CrearAsync(CrearGrupoGestionDto dto)
    {
        using var response = await _sender.SendAsync(HttpMethod.Post, "api/GrupoGestion", JsonContent.Create(dto));
    }

    public async Task ActualizarAsync(int id, CrearGrupoGestionDto dto)
    {
        using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/GrupoGestion/{id}", JsonContent.Create(dto));
    }

    public async Task EliminarAsync(int id)
    {
        using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/GrupoGestion/{id}");
    }
}
