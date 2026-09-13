using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using static Org.BouncyCastle.Math.EC.ECCurve;

public class TipoAgendamientoService
{
    private readonly HttpClient _http;

    public TipoAgendamientoService(IHttpClientFactory factory, ConfigService config)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _http = factory.CreateClient("Api");

    }

    public async Task<List<TipoAgendamientoDto>> ObtenerTodosAsync()
        => await _http.GetFromJsonAsync<List<TipoAgendamientoDto>>("api/tipoagendamiento") ?? new();

    public async Task<TipoAgendamientoDto?> ObtenerPorIdAsync(int id)
        => await _http.GetFromJsonAsync<TipoAgendamientoDto?>($"api/tipoagendamiento/{id}");

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
        var response = await _http.PostAsJsonAsync("api/tipoagendamiento", dto);

        // Por si querés ver la respuesta
        var body = await response.Content.ReadAsStringAsync();
        System.Diagnostics.Debug.WriteLine($"📥 Respuesta: {response.StatusCode} {body}");

        response.EnsureSuccessStatusCode();
    }

    public async Task ActualizarAsync(int id, TipoAgendamientoDto dto)
        => await _http.PatchAsJsonAsync($"api/tipoagendamiento/{id}", dto);

    public async Task EliminarAsync(int id)
        => await _http.DeleteAsync($"api/tipoagendamiento/{id}");
}
