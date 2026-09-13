// IurixBlazor/Services/RubroService.cs
using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace IurixBlazor.Services;

public class RubroService
{
    private readonly HttpClient _http;
    public RubroService(IHttpClientFactory factory, ConfigService config)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _http = factory.CreateClient("Api");
    }

    public Task<List<RubroDto>?> ObtenerTodosAsync()
        => _http.GetFromJsonAsync<List<RubroDto>>("api/Rubros");

    public Task<RubroDto?> ObtenerPorIdAsync(int id)
        => _http.GetFromJsonAsync<RubroDto>($"api/Rubros/{id}");

    public async Task CrearAsync(CrearRubroDto dto)
    {
        var r = await _http.PostAsJsonAsync("api/Rubros", dto);
        r.EnsureSuccessStatusCode();
    }

    public async Task ActualizarAsync(int id, RubroDto dto)
    {
        var r = await _http.PatchAsJsonAsync($"api/Rubros/{id}", dto);
        r.EnsureSuccessStatusCode();
    }

    public async Task EliminarAsync(int id)
    {
        var r = await _http.DeleteAsync($"api/Rubros/{id}");
        r.EnsureSuccessStatusCode();
    }
}

