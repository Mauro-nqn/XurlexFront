using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using static Org.BouncyCastle.Math.EC.ECCurve;

public class TipoAgendamientoService
{
    private readonly IApiRequestSender _sender;

    public TipoAgendamientoService(IApiRequestSender sender)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _sender = sender;

    }

    public async Task<List<TipoAgendamientoDto>> ObtenerTodosAsync()
    {
        using var response = await _sender.SendAsync(HttpMethod.Get, "api/tipoagendamiento");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TipoAgendamientoDto>>() ?? new();
    }

    public async Task<TipoAgendamientoDto?> ObtenerPorIdAsync(int id)
    {
        using var response = await _sender.SendAsync(HttpMethod.Get, $"api/tipoagendamiento/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TipoAgendamientoDto?>();
    }

    //public async Task CrearAsync(CrearTipoAgendamientoDto dto)
    //    => await _http.PostAsJsonAsync("api/tipoagendamiento", dto);
    public async Task CrearAsync(CrearTipoAgendamientoDto dto)
    {
        // 🔍 Mostrar en consola / debug el JSON antes de enviarlo
        var jsonDebug = JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        System.Diagnostics.Debug.WriteLine("📤 JSON enviado a backend (CrearTipoAgendamiento):");
        System.Diagnostics.Debug.WriteLine(jsonDebug);

        // Enviar al backend
        using var response = await _sender.SendAsync(HttpMethod.Post, "api/tipoagendamiento", JsonContent.Create(dto));

        // Por si querés ver la respuesta
        var body = await response.Content.ReadAsStringAsync();
        System.Diagnostics.Debug.WriteLine($"📥 Respuesta: {response.StatusCode} {body}");

        response.EnsureSuccessStatusCode();
    }

    public async Task ActualizarAsync(int id, TipoAgendamientoDto dto)
    {
        using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/tipoagendamiento/{id}", JsonContent.Create(dto));
    }

    public async Task EliminarAsync(int id)
    {
        using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/tipoagendamiento/{id}");
    }
}
