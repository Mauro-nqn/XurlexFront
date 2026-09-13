using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using static Org.BouncyCastle.Math.EC.ECCurve;

public class MovimientoService
{
    private readonly HttpClient _http;

    public MovimientoService(IHttpClientFactory factory, ConfigService config)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _http = factory.CreateClient("Api");
    }



    public async Task<MovimientoTtsDto?> ObtenerTtsAsync(int id)
    {
        return await _http.GetFromJsonAsync<MovimientoTtsDto>(
            $"api/movimientos/{id}/tts");
    }

    public async Task<ArchivoTtsDto?> ObtenerEscritoTtsAsync(int movId, int escritoId)
    {
        try
        {
            return await _http.GetFromJsonAsync<ArchivoTtsDto>(
                $"api/movimientos/{movId}/escrito/{escritoId}/tts"
            );
        }
        catch
        {
            return new ArchivoTtsDto
            {
                Ok = false,
                Error = "No se pudo obtener el texto del escrito para lectura."
            };
        }
    }





    public async Task<List<MovimientoDto>> ObtenerTodosAsync()
    {
        var result = await _http.GetFromJsonAsync<List<MovimientoDto>>("api/movimiento");
        return result ?? new List<MovimientoDto>();
    }

    public async Task<MovimientoDto?> ObtenerPorIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<MovimientoDto>($"api/movimiento/{id}");
    }

    public async Task<List<MovimientoDto>> ObtenerPorProcesoAsync(string tipo, int procesoId)
    {
        if (string.IsNullOrWhiteSpace(tipo) || procesoId <= 0)
            return new List<MovimientoDto>();

        var url = $"api/movimiento/por-proceso?tipo={tipo}&procesoId={procesoId}";
        return await _http.GetFromJsonAsync<List<MovimientoDto>>(url) ?? new List<MovimientoDto>();
    }



    //public async Task<bool> CrearAsync(CrearMovimientoDto dto)
    //{
    //    var response = await _http.PostAsJsonAsync("api/movimiento", dto);
    //    return response.IsSuccessStatusCode;
    //}

    //public async Task<MovimientoDto?> CrearYDevolverAsync(CrearMovimientoDto dto)
    //{
    //    var resp = await _http.PostAsJsonAsync("api/movimiento", dto);
    //    if (!resp.IsSuccessStatusCode) return null;
    //    return await resp.Content.ReadFromJsonAsync<MovimientoDto>();
    //}

    public async Task<MovimientoDto?> CrearYDevolverAsync(CrearMovimientoDto dto)
    {
        var resp = await _http.PostAsJsonAsync("api/movimiento", dto);
        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException($"POST movimientos -> {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {body}");

        return JsonSerializer.Deserialize<MovimientoDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<bool> CrearAsync(CrearMovimientoDto dto) // tu método viejo (si querés mantenerlo)
    {
        var creado = await CrearYDevolverAsync(dto);
        return creado != null;
    }


    public async Task ActualizarAsync(int id, CrearMovimientoDto dto)
    {
        var response = await _http.PatchAsJsonAsync($"api/movimiento/{id}", dto);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"PATCH {id} -> {(int)response.StatusCode}. Body: {body}");
    }

    //public async Task EliminarAsync(int id)
    //{
    //    var response = await _http.DeleteAsync($"api/movimiento/{id}");
    //    response.EnsureSuccessStatusCode();
    //}

    public async Task<bool> EliminarAsync(int id, bool force = false)
    {
        var url = force ? $"/api/movimiento/{id}?force=true" : $"/api/movimiento/{id}";
        using var resp = await _http.DeleteAsync(url);
        return resp.IsSuccessStatusCode; // NO EnsureSuccessStatusCode
    }



    public async Task<bool> EliminarTodosPorProcesoAsync(string tipo, int procesoId, bool force = false)
    {
        if (string.IsNullOrWhiteSpace(tipo) || procesoId <= 0)
            return false;

        var query = $"tipo={Uri.EscapeDataString(tipo)}&procesoId={procesoId}";
        if (force)
            query += "&force=true";

        var url = $"/api/movimiento/por-proceso?{query}";

        using var resp = await _http.DeleteAsync(url);

        // acá podría interesarte distinguir 409, 424, etc. si querés lógica fina
        return resp.IsSuccessStatusCode;
    }





    // Excepciones específicas para capturar en la UI
    public class MovimientoAgendadoException : Exception
    {
        public string? AgendaId { get; }
        public string? GoogleRegistrado { get; }

        public MovimientoAgendadoException(string message, string? agendaId, string? googleRegistrado)
            : base(message)
        {
            AgendaId = agendaId;
            GoogleRegistrado = googleRegistrado;
        }
    }

    public class GoogleDeleteFailedException : Exception
    {
        public GoogleDeleteFailedException(string message) : base(message) { }
    }
}
