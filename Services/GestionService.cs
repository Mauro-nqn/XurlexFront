using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Text.Json;
using static System.Net.WebRequestMethods;

public class GestionService
{
    private readonly HttpClient _httpClient;

    public GestionService(IHttpClientFactory factory, ConfigService config)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _httpClient = factory.CreateClient("Api");
    }

    public async Task<List<GestionDto>> ObtenerTodasAsync()
        => await _httpClient.GetFromJsonAsync<List<GestionDto>>("api/Gestion") ?? new();

    //public async Task<GestionDto?> ObtenerPorIdAsync(int id)

    //    => await _httpClient.GetFromJsonAsync<GestionDto>($"api/Gestion/{id}");

    public async Task<GestionDto?> ObtenerPorIdAsync(int id)
    {
        System.Diagnostics.Debug.WriteLine($"[GestionService] GET api/gestiones/{id}");
        return await _httpClient.GetFromJsonAsync<GestionDto>($"api/Gestion/{id}");
    }

    public async Task<List<GestionDto>> FiltrarGestionesAsync(int? responsableId, string? tipo, string? nombreProceso, int? personaId)
    {
        var url = $"api/gestion/filtrar?responsableId={responsableId}&tipo={tipo}&nombreProceso={nombreProceso}&personaId={personaId}";
        return await _httpClient.GetFromJsonAsync<List<GestionDto>>(url) ?? new List<GestionDto>();
    }


    //public async Task CrearAsync(CrearGestionDto dto)
    //    => await _httpClient.PostAsJsonAsync("api/Gestion", dto);

    public async Task CrearAsync(CrearGestionDto dto)
    => await _httpClient.PostAsJsonAsync("api/Gestion/crear-con-proceso", dto);

    public async Task ActualizarAsync(int id, CrearGestionDto dto)
    {
        // 🔍 Serializar el DTO a JSON para inspección
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
        System.Diagnostics.Debug.WriteLine($"[PATCH] Enviando JSON para actualizar gestión (ID={id}):\n{json}");

        // Ejecutar la petición PATCH
        await _httpClient.PatchAsync($"api/Gestion/{id}", JsonContent.Create(dto));
    }



    public async Task<ProcesoInfoDto?> ObtenerDatosProcesoAsync(string tipo, int procesoId)
    {
        var url = $"api/gestion/datos-proceso/{tipo}/{procesoId}";
        return await _httpClient.GetFromJsonAsync<ProcesoInfoDto>(url);
    }


    public Task<GestionPersonaDto?> ObtenerPersonaPorMovimientoAsync(int movimientoId)
    => _httpClient.GetFromJsonAsync<GestionPersonaDto>($"api/gestion/persona-por-movimiento/{movimientoId}");



    public async Task<List<ParteProcesoDto>> ObtenerPartesPorProcesoJudicialAsync(int procesoId)
    => await _httpClient.GetFromJsonAsync<List<ParteProcesoDto>>(
        $"api/parteproceso/por-proceso-judicial/{procesoId}")
       ?? new List<ParteProcesoDto>();

    public async Task<List<ParteProcesoDto>> ObtenerPartesPorProcesoExtrajudicialAsync(int procesoId)
        => await _httpClient.GetFromJsonAsync<List<ParteProcesoDto>>(
            $"api/parteproceso/por-proceso-extrajudicial/{procesoId}")
           ?? new List<ParteProcesoDto>();



    public async Task EliminarAsync(int id)
        => await _httpClient.DeleteAsync($"api/Gestion/{id}");
}
