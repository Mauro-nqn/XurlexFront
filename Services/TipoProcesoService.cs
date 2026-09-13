using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;

public class TipoProcesoService
{
    private readonly HttpClient _http;

    public TipoProcesoService(IHttpClientFactory factory, ConfigService config)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _http = factory.CreateClient("Api");
    }

    public async Task<List<TipoProcesoDto>> ObtenerTodosAsync()
        => await _http.GetFromJsonAsync<List<TipoProcesoDto>>("api/TipoProceso") ?? new();

    public async Task<TipoProcesoDto?> ObtenerPorIdAsync(int id)
        => await _http.GetFromJsonAsync<TipoProcesoDto>($"api/TipoProceso/{id}");

    public async Task CrearAsync(CrearTipoProcesoDto dto)
        => await _http.PostAsJsonAsync("api/TipoProceso", dto);

    public async Task ActualizarAsync(int id, CrearTipoProcesoDto dto)
        => await _http.PatchAsync($"api/TipoProceso/{id}", JsonContent.Create(dto));

    public async Task EliminarAsync(int id)
        => await _http.DeleteAsync($"api/TipoProceso/{id}");
}
