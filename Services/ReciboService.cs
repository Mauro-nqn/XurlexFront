// IurixBlazor/Services/ReciboService.cs
using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;
using static Org.BouncyCastle.Math.EC.ECCurve;
using static System.Net.WebRequestMethods;

namespace IurixBlazor.Services;

public class ReciboService
{
    private readonly HttpClient _httpClient;
    public ReciboService(IHttpClientFactory factory, ConfigService config)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _httpClient = factory.CreateClient("Api");
    }

    public async Task<ReciboDto?> CrearAsync(CrearReciboDto dto)
    {
        var r = await _httpClient.PostAsJsonAsync("api/Recibos", dto);
        r.EnsureSuccessStatusCode();
        return await r.Content.ReadFromJsonAsync<ReciboDto>();
    }

    public Task<ReciboDto?> ObtenerPorIdAsync(int id)
        => _httpClient.GetFromJsonAsync<ReciboDto>($"api/Recibos/{id}");


    //public async Task<ReciboDto?> ObtenerPorIdAsync(int id)
    //{
    //    return await _httpClient.GetFromJsonAsync<ReciboDto>($"api/Recibos/{id}");
    //}

    public async Task PatchAsync(int id, ReciboPatchDto dto)
    {
        var r = await _httpClient.PatchAsJsonAsync($"api/Recibos/{id}", dto);
        r.EnsureSuccessStatusCode();
    }

    public async Task AjustarMontoAsync(int reciboId, decimal nuevoTotal)
    {
        var r = await _httpClient.PostAsJsonAsync("api/Recibos/ajustar-monto", new AjustarReciboDto
        {
            ReciboId = reciboId,
            NuevoTotal = nuevoTotal
        });
        r.EnsureSuccessStatusCode();
    }

    //public async Task ReversarAsync(int reciboId)
    //{
    //    var r = await _httpClient.PostAsJsonAsync("api/Recibos/reversar", new { ReciboId = reciboId });
    //    r.EnsureSuccessStatusCode();
    //}


    public async Task<ReciboVistaDto?> ObtenerVistaAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<ReciboVistaDto>($"api/Recibos/{id}/vista");
    }

    public async Task<int> ObtenerMovimientoCreditoIdAsync(int reciboId)
    {
        // GET api/recibos/{id}/mov-credito-id  -> devuelve un int (JSON)
        var movId = await _httpClient.GetFromJsonAsync<int>($"api/recibos/{reciboId}/mov-credito-id");
        if (movId == 0)
            throw new HttpRequestException("No se encontró el movimiento crédito del recibo.", null, System.Net.HttpStatusCode.NotFound);
        return movId;
    }

   

    public async Task EliminarAsync(int id)
    {
        var r = await _httpClient.DeleteAsync($"api/Recibos/{id}");
        // si hay imputaciones, el back debería devolver 409
        if (!r.IsSuccessStatusCode) throw new HttpRequestException(await r.Content.ReadAsStringAsync(), null, r.StatusCode);
    }

    public async Task DeshacerAnulacionAsync(int id)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/Recibos/{id}/deshacer-anulacion", new { });
        response.EnsureSuccessStatusCode();
    }


    public async Task DeshacerReversaMovimientoAsync(int movimientoId)
    {
        var response = await _httpClient.DeleteAsync($"api/Recibos/reversas/{movimientoId}");
        response.EnsureSuccessStatusCode();
    }


    public async Task AnularAsync(int id, AnularReciboDto dto)
    {
        var r = await _httpClient.PostAsJsonAsync($"api/Recibos/{id}/anular", dto);

        if (!r.IsSuccessStatusCode)
        {
            var body = await r.Content.ReadAsStringAsync();
            throw new HttpRequestException(body, null, r.StatusCode);
        }
    }


}

