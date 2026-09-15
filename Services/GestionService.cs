using IurixBlazor.Pages;
using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Text.Json;
using static System.Net.WebRequestMethods;

public class GestionService
{
    private readonly IApiRequestSender _sender;

    public GestionService(IApiRequestSender sender)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _sender = sender;
    }

    public async Task<List<GestionDto>> ObtenerTodasAsync()
    {
        using var response = await _sender.SendAsync(HttpMethod.Get, "api/Gestion");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<GestionDto>>() ?? new();
    }

    //public async Task<GestionDto?> ObtenerPorIdAsync(int id)

    //    => await _httpClient.GetFromJsonAsync<GestionDto>($"api/Gestion/{id}");

    public async Task<GestionDto?> ObtenerPorIdAsync(int id)
    {
        System.Diagnostics.Debug.WriteLine($"[GestionService] GET api/gestiones/{id}");
        using var response = await _sender.SendAsync(HttpMethod.Get, $"api/Gestion/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GestionDto>();
    }

    public async Task<List<GestionDto>> FiltrarGestionesAsync(int? responsableId, string? tipo, string? nombreProceso, int? personaId)
    {
        var url = $"api/gestion/filtrar?responsableId={responsableId}&tipo={tipo}&nombreProceso={nombreProceso}&personaId={personaId}";
        using var response = await _sender.SendAsync(HttpMethod.Get, url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<GestionDto>>() ?? new List<GestionDto>();
    }


    //public async Task CrearAsync(CrearGestionDto dto)
    //    => await _httpClient.PostAsJsonAsync("api/Gestion", dto);

    public async Task CrearAsync(CrearGestionDto dto)
    {
        using var response = await _sender.SendAsync(HttpMethod.Post, "api/Gestion/crear-con-proceso", JsonContent.Create(dto));
    }

    public async Task ActualizarAsync(int id, CrearGestionDto dto)
    {
        // 🔍 Serializar el DTO a JSON para inspección
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
        System.Diagnostics.Debug.WriteLine($"[PATCH] Enviando JSON para actualizar gestión (ID={id}):\n{json}");

        // Ejecutar la petición PATCH
        using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/Gestion/{id}", JsonContent.Create(dto));
    }



    public async Task<ProcesoInfoDto?> ObtenerDatosProcesoAsync(string tipo, int procesoId)
    {
        var url = $"api/gestion/datos-proceso/{tipo}/{procesoId}";
        using var response = await _sender.SendAsync(HttpMethod.Get, url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProcesoInfoDto>();
    }


    public async Task<GestionPersonaDto?> ObtenerPersonaPorMovimientoAsync(int movimientoId)
    {
        using var response = await _sender.SendAsync(HttpMethod.Get, $"api/gestion/persona-por-movimiento/{movimientoId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GestionPersonaDto>();
    }



    public async Task<List<ParteProcesoDto>> ObtenerPartesPorProcesoJudicialAsync(int procesoId)
    {
        using var response = await _sender.SendAsync(HttpMethod.Get, $"api/parteproceso/por-proceso-judicial/{procesoId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ParteProcesoDto>>() ?? new List<ParteProcesoDto>();
    }

    public async Task<List<ParteProcesoDto>> ObtenerPartesPorProcesoExtrajudicialAsync(int procesoId)
    {
        using var response = await _sender.SendAsync(HttpMethod.Get, $"api/parteproceso/por-proceso-extrajudicial/{procesoId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ParteProcesoDto>>() ?? new List<ParteProcesoDto>();
    }



    public async Task EliminarAsync(int id)
    {
        using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/Gestion/{id}");
    }
}
