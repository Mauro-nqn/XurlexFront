// IurixBlazor/Services/CentroCostoService.cs
using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace IurixBlazor.Services;

public class CentroCostoService
{
    private readonly HttpClient _http;
    
    public CentroCostoService(IHttpClientFactory factory, ConfigService config) 
     {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _http = factory.CreateClient("Api");
    }

public Task<List<CentroCostoDto>?> ObtenerTodosAsync()
        => _http.GetFromJsonAsync<List<CentroCostoDto>>("api/CentrosCosto");

    public Task<CentroCostoDto?> ObtenerPorIdAsync(int id)
        => _http.GetFromJsonAsync<CentroCostoDto>($"api/CentrosCosto/{id}");

    public async Task CrearAsync(CrearCentroCostoDto dto)
    {
        var r = await _http.PostAsJsonAsync("api/CentrosCosto", dto);
        r.EnsureSuccessStatusCode();
    }

    public async Task ActualizarAsync(int id, CentroCostoDto dto)
    {
        var r = await _http.PatchAsJsonAsync($"api/CentrosCosto/{id}", dto);
        r.EnsureSuccessStatusCode();
    }

    public async Task EliminarAsync(int id)
    {
        var r = await _http.DeleteAsync($"api/CentrosCosto/{id}");
        r.EnsureSuccessStatusCode();
    }
}

