// IurixBlazor/Services/CentroCostoService.cs
using IurixBlazor.Pages;
using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace IurixBlazor.Services;

public class CentroCostoService
{
    private readonly IApiRequestSender _sender;
    
    public CentroCostoService(IApiRequestSender sender)
     {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _sender = sender;
    }

public async Task<List<CentroCostoDto>?> ObtenerTodosAsync()
    {
        using var response = await _sender.SendAsync(HttpMethod.Get, "api/CentrosCosto");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<CentroCostoDto>>();
    }

    public async Task<CentroCostoDto?> ObtenerPorIdAsync(int id)
    {
        using var response = await _sender.SendAsync(HttpMethod.Get, $"api/CentrosCosto/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CentroCostoDto>();
    }

    public async Task CrearAsync(CrearCentroCostoDto dto)
    {
        using var r = await _sender.SendAsync(HttpMethod.Post, "api/CentrosCosto", JsonContent.Create(dto));
        r.EnsureSuccessStatusCode();
    }

    public async Task ActualizarAsync(int id, CentroCostoDto dto)
    {
        using var r = await _sender.SendAsync(HttpMethod.Patch, $"api/CentrosCosto/{id}", JsonContent.Create(dto));
        r.EnsureSuccessStatusCode();
    }

    public async Task EliminarAsync(int id)
    {
        using var r = await _sender.SendAsync(HttpMethod.Delete, $"api/CentrosCosto/{id}");
        r.EnsureSuccessStatusCode();
    }
}

