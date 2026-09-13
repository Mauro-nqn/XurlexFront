using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;

public class GrupoGestionService
{
    private readonly HttpClient _http;

    public GrupoGestionService(IHttpClientFactory factory, ConfigService config)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _http = factory.CreateClient("Api");

    }

    public async Task<List<GrupoGestionDto>> ObtenerTodosAsync()
        => await _http.GetFromJsonAsync<List<GrupoGestionDto>>("api/GrupoGestion") ?? new();

    public async Task<GrupoGestionDto?> ObtenerPorIdAsync(int id)
        => await _http.GetFromJsonAsync<GrupoGestionDto>($"api/GrupoGestion/{id}");

    public async Task CrearAsync(CrearGrupoGestionDto dto)
        => await _http.PostAsJsonAsync("api/GrupoGestion", dto);

    public async Task ActualizarAsync(int id, CrearGrupoGestionDto dto)
        => await _http.PatchAsync($"api/GrupoGestion/{id}", JsonContent.Create(dto));

    public async Task EliminarAsync(int id)
        => await _http.DeleteAsync($"api/GrupoGestion/{id}");
}
