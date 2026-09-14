using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;


public class TipoProcesoService
{
    private readonly IApiRequestSender _sender;

    public TipoProcesoService(IApiRequestSender sender)
    {
        _sender = sender;
    }

    public async Task<List<TipoProcesoDto>> ObtenerTodosAsync()
    {
        using var response = await _sender.SendAsync(HttpMethod.Get, "api/TipoProceso");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TipoProcesoDto>>() ?? new();
    }

    public async Task<TipoProcesoDto?> ObtenerPorIdAsync(int id)
    {
        using var response = await _sender.SendAsync(HttpMethod.Get, $"api/TipoProceso/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TipoProcesoDto>();
    }

    public async Task CrearAsync(CrearTipoProcesoDto dto)
    {
        using var response = await _sender.SendAsync(HttpMethod.Post, "api/TipoProceso", JsonContent.Create(dto));
    }

    public async Task ActualizarAsync(int id, CrearTipoProcesoDto dto)
    {
        using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/TipoProceso/{id}", JsonContent.Create(dto));
    }

    public async Task EliminarAsync(int id)
    {
        using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/TipoProceso/{id}");
    }
}
