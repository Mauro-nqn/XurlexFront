using IurixBlazor.Pages;
using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

public class EscritoMovimientoService
{
    private readonly IApiRequestSender _sender;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Base del controller: api/escrito-movimiento
    private const string BasePath = "api/escritomovimiento";

    public EscritoMovimientoService(IApiRequestSender sender)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _sender = sender;
    }

    // GET api/escrito-movimiento/por-movimiento/{movimientoId}
    public async Task<EscritoMovimientoDto?> ObtenerPorMovimientoAsync(int movimientoId, CancellationToken ct = default)
    {
        using var resp = await _sender.SendAsync(HttpMethod.Get, $"{BasePath}/por-movimiento/{movimientoId}", cancellationToken: ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<EscritoMovimientoDto>(JsonOpts, ct);
    }

    // POST api/escrito-movimiento
    public async Task<EscritoMovimientoDto?> CrearAsync(CrearEscritoMovimientoDto dto, CancellationToken ct = default)
    {
        // (Opcional) Log del JSON
        var jsonLog = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
        System.Diagnostics.Debug.WriteLine("📤 Crear EscritoMovimiento JSON: " + jsonLog);

        using var resp = await _sender.SendAsync(HttpMethod.Post, BasePath, JsonContent.Create(dto, options: JsonOpts), ct);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<EscritoMovimientoDto>(JsonOpts, ct);
    }

    // PATCH api/escrito-movimiento/{movimientoId}
    public async Task<bool> ActualizarAsync(int movimientoId, CrearEscritoMovimientoDto dto, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var resp = await _sender.SendAsync(HttpMethod.Patch, $"{BasePath}/{movimientoId}", content, ct);
        return resp.IsSuccessStatusCode;
    }

    // DELETE api/escrito-movimiento/por-movimiento/{movimientoId}?usuarioId=123
    public async Task<bool> EliminarPorMovimientoAsync(int movimientoId, int usuarioId, CancellationToken ct = default)
    {
        using var resp = await _sender.SendAsync(HttpMethod.Delete, $"{BasePath}/por-movimiento/{movimientoId}?usuarioId={usuarioId}", cancellationToken: ct);
        return resp.IsSuccessStatusCode;
    }
}

