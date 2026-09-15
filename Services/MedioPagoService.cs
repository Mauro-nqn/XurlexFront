// IurixBlazor/Services/MedioPagoService.cs
using IurixBlazor.Pages;
using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace IurixBlazor.Services;

public class MedioPagoService
{
    private readonly IApiRequestSender _sender;
    public MedioPagoService(IApiRequestSender sender)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _sender = sender;
    }

    public async Task<List<MedioPagoDto>?> ObtenerTodosAsync()
    {
        using var response = await _sender.SendAsync(HttpMethod.Get, "api/MediosPago");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<MedioPagoDto>>();
    }

    public async Task<MedioPagoDto?> ObtenerPorIdAsync(int id)
    {
        using var response = await _sender.SendAsync(HttpMethod.Get, $"api/MediosPago/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MedioPagoDto>();
    }

    public async Task CrearAsync(CrearMedioPagoDto dto)
    {
        using var r = await _sender.SendAsync(HttpMethod.Post, "api/MediosPago", JsonContent.Create(dto));
        r.EnsureSuccessStatusCode();
    }

    public async Task ActualizarAsync(int id, MedioPagoDto dto)
    {
        using var r = await _sender.SendAsync(HttpMethod.Patch, $"api/MediosPago/{id}", JsonContent.Create(dto));
        r.EnsureSuccessStatusCode();
    }

    public async Task EliminarAsync(int id)
    {
        using var r = await _sender.SendAsync(HttpMethod.Delete, $"api/MediosPago/{id}");
        r.EnsureSuccessStatusCode();
    }
}
