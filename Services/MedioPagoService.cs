// IurixBlazor/Services/MedioPagoService.cs
using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace IurixBlazor.Services;

public class MedioPagoService
{
    private readonly HttpClient _httpClient;
    public MedioPagoService(IHttpClientFactory factory, ConfigService config)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _httpClient = factory.CreateClient("Api");
    }

    public Task<List<MedioPagoDto>?> ObtenerTodosAsync()
        => _httpClient.GetFromJsonAsync<List<MedioPagoDto>>("api/MediosPago");

    public Task<MedioPagoDto?> ObtenerPorIdAsync(int id)
        => _httpClient.GetFromJsonAsync<MedioPagoDto>($"api/MediosPago/{id}");

    public async Task CrearAsync(CrearMedioPagoDto dto)
    {
        var r = await _httpClient.PostAsJsonAsync("api/MediosPago", dto);
        r.EnsureSuccessStatusCode();
    }

    public async Task ActualizarAsync(int id, MedioPagoDto dto)
    {
        var r = await _httpClient.PatchAsJsonAsync($"api/MediosPago/{id}", dto);
        r.EnsureSuccessStatusCode();
    }

    public async Task EliminarAsync(int id)
    {
        var r = await _httpClient.DeleteAsync($"api/MediosPago/{id}");
        r.EnsureSuccessStatusCode();
    }
}
