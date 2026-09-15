// IurixBlazor/Services/RubroService.cs
using IurixBlazor.Pages;
using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace IurixBlazor.Services;

public class RubroService
{
    private readonly IApiRequestSender _sender;
    public RubroService(IApiRequestSender sender)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _sender = sender;
    }

    public async Task<List<RubroDto>?> ObtenerTodosAsync()
    {
        using var response = await _sender.SendAsync(HttpMethod.Get, "api/Rubros");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<RubroDto>>();
    }

    public async Task<RubroDto?> ObtenerPorIdAsync(int id)
    {
        using var response = await _sender.SendAsync(HttpMethod.Get, $"api/Rubros/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RubroDto>();
    }

    public async Task CrearAsync(CrearRubroDto dto)
    {
        using var r = await _sender.SendAsync(HttpMethod.Post, "api/Rubros", JsonContent.Create(dto));
        r.EnsureSuccessStatusCode();
    }

    public async Task ActualizarAsync(int id, RubroDto dto)
    {
        using var r = await _sender.SendAsync(HttpMethod.Patch, $"api/Rubros/{id}", JsonContent.Create(dto));
        r.EnsureSuccessStatusCode();
    }

    public async Task EliminarAsync(int id)
    {
        using var r = await _sender.SendAsync(HttpMethod.Delete, $"api/Rubros/{id}");
        r.EnsureSuccessStatusCode();
    }
}

