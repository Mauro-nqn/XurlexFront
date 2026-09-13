using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

public class EscritoMovimientoService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Base del controller: api/escrito-movimiento
    private const string BasePath = "api/escritomovimiento";

    public EscritoMovimientoService(IHttpClientFactory factory, ConfigService config)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _httpClient = factory.CreateClient("Api");
    }

    // GET api/escrito-movimiento/por-movimiento/{movimientoId}
    public async Task<EscritoMovimientoDto?> ObtenerPorMovimientoAsync(int movimientoId, CancellationToken ct = default)
    {
        var resp = await _httpClient.GetAsync($"{BasePath}/por-movimiento/{movimientoId}", ct);
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

        var resp = await _httpClient.PostAsJsonAsync(BasePath, dto, JsonOpts, ct);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<EscritoMovimientoDto>(JsonOpts, ct);
    }

    // PATCH api/escrito-movimiento/{movimientoId}
    public async Task<bool> ActualizarAsync(int movimientoId, CrearEscritoMovimientoDto dto, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await _httpClient.PatchAsync($"{BasePath}/{movimientoId}", content, ct);
        return resp.IsSuccessStatusCode;
    }

    // DELETE api/escrito-movimiento/por-movimiento/{movimientoId}?usuarioId=123
    public async Task<bool> EliminarPorMovimientoAsync(int movimientoId, int usuarioId, CancellationToken ct = default)
    {
        var resp = await _httpClient.DeleteAsync($"{BasePath}/por-movimiento/{movimientoId}?usuarioId={usuarioId}", ct);
        return resp.IsSuccessStatusCode;
    }
}

