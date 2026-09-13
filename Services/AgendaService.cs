using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

public class AgendaService
{
    //private readonly HttpClient _http;


    private readonly HttpClient _http;

    // ⬇️ Inyectado por DI (no crees HttpClient a mano)
    public AgendaService(IHttpClientFactory factory)
    {
        //_http = http ?? throw new ArgumentNullException(nameof(http));
        //if (_http.BaseAddress is null)
        //    throw new InvalidOperationException("AgendaService: BaseAddress no está configurada.");

        _http = factory.CreateClient("Api");


    }

    public async Task<List<AgendaDto>> ObtenerTodasAsync()
        => await _http.GetFromJsonAsync<List<AgendaDto>>("api/agenda") ?? new();

    public async Task<AgendaDto?> ObtenerPorIdAsync(int id)
        => await _http.GetFromJsonAsync<AgendaDto>($"api/agenda/{id}");


    //public async Task<List<AgendaDto>> ObtenerTodasAsync()
    //    => await _http.GetFromJsonAsync<List<AgendaDto>>("api/agenda") ?? new();

    //public async Task<AgendaDto?> ObtenerPorIdAsync(int id)
    //    => await _http.GetFromJsonAsync<AgendaDto>($"api/agenda/{id}");

    //public AgendaService(ConfigService config)
    //{
    //    var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
    //    _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    //}

    



    //  nuevo
    //public async Task<List<AgendaDto>> ObtenerPorFechaAsync(
    //DateOnly fecha, IEnumerable<int>? usuarios = null, bool incluirParaTodos = true, CancellationToken ct = default)
    //{
    //    var url = new StringBuilder($"api/agenda/por-fecha?fecha={fecha:yyyy-MM-dd}&incluirParaTodos={incluirParaTodos.ToString().ToLower()}");
    //    if (usuarios != null)
    //    {
    //        foreach (var u in usuarios) url.Append($"&usuarios={u}");
    //    }

    //    var resp = await _http.GetAsync(url.ToString(), ct);
    //    resp.EnsureSuccessStatusCode();
    //    return await resp.Content.ReadFromJsonAsync<List<AgendaDto>>(cancellationToken: ct) ?? new();
    //}


    public async Task<List<AgendaDto>> ObtenerPorFechaAsync(
    DateOnly fecha,
    IEnumerable<int>? usuarios = null,
    bool incluirParaTodos = true,
    EstadoAgenda? estado = null,
    string? q = null)
    {
        var qs = new List<string> { $"fecha={fecha:yyyy-MM-dd}", $"incluirParaTodos={incluirParaTodos.ToString().ToLower()}" };

        if (estado.HasValue) qs.Add($"estado={(int)estado.Value}");
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q)}");

        if (usuarios is not null)
        {
            // Modo key repetida => ?usuarios=5&usuarios=7
            foreach (var u in usuarios) qs.Add($"usuarios={u}");
        }

        var url = "/api/agenda/por-fecha?" + string.Join("&", qs);
        return await _http.GetFromJsonAsync<List<AgendaDto>>(url) ?? new();
    }









    //public async Task CrearAsync(CrearAgendaDto dto)
    //{
    //    var response = await _http.PostAsJsonAsync("api/agenda", dto);
    //    response.EnsureSuccessStatusCode();
    //}


    public async Task<AgendaDto?> ObtenerPorMovimientoAsync(int movimientoId)
    {
        var resp = await _http.GetAsync($"api/agenda/por-movimiento/{movimientoId}");

        if (resp.StatusCode == HttpStatusCode.NotFound || resp.StatusCode == HttpStatusCode.NoContent)
            return null; // no hay agenda asociada

        resp.EnsureSuccessStatusCode(); // si no es 2xx, que explote acá

        return await resp.Content.ReadFromJsonAsync<AgendaDto>();
    }

    public async Task<AgendaDto?> CrearAsync(CrearAgendaDto dto)
    {
        var resp = await _http.PostAsJsonAsync("api/agenda", dto);
        if (!resp.IsSuccessStatusCode)
        {
            // intenta leer ProblemDetails / texto
            var detalle = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(detalle)
                    ? $"No se pudo crear la agenda. Código {(int)resp.StatusCode}."
                    : detalle);
        }
        return await resp.Content.ReadFromJsonAsync<AgendaDto>();
    }

    //public async Task ActualizarAsync(int id, ActualizarAgendaDto dto)
    //{
    //    var response = await _http.PatchAsJsonAsync($"api/agenda/actualizar/{id}", dto);
    //    response.EnsureSuccessStatusCode();
    //}



    //public async Task<AgendaDto?> CrearDesdeMovimientoAsync(CrearAgendaDesdeMovimientoDto dto)
    //{
    //    var resp = await _http.PostAsJsonAsync("api/agenda/desde-movimiento", dto);
    //    if (!resp.IsSuccessStatusCode) return null;
    //    return await resp.Content.ReadFromJsonAsync<AgendaDto>();
    //}


    //public async Task<AgendaDto?> ActualizarDesdeMovimientoAsync(int agendaId, ActualizarAgendaDesdeMovimientoDto dto)
    //{
    //    var resp = await _http.PutAsJsonAsync($"api/agenda/desde-movimiento/{agendaId}", dto);
    //    if (!resp.IsSuccessStatusCode) return null;
    //    return await resp.Content.ReadFromJsonAsync<AgendaDto>();
    //}


    public async Task<AgendaDto> ActualizarDesdeMovimientoAsync(int id, ActualizarAgendaDesdeMovimientoDto dto, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"/api/agenda/desde-movimiento/{id}", dto, ct);

        var text = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(text)
                ? $"No se pudo actualizar la agenda (HTTP {(int)resp.StatusCode})."
                : text);

        // Si el backend devuelve 200 con body, parsealo:
        if (!string.IsNullOrWhiteSpace(text))
        {
            var updated = System.Text.Json.JsonSerializer.Deserialize<AgendaDto>(
                text, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (updated is not null) return updated;
        }

        // Si algún día volvés a 204 NoContent, pedí el recurso:
        var fallback = await _http.GetFromJsonAsync<AgendaDto>($"/api/agendas/{id}", ct);
        if (fallback is null)
            throw new InvalidOperationException("La agenda se actualizó pero no se pudo recuperar.");

        return fallback;
    }

    public async Task<AgendaDto> CrearDesdeMovimientoAsync(CrearAgendaDesdeMovimientoDto dto, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("/api/agenda/desde-movimiento", dto, ct);
        var text = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(text)
                ? $"No se pudo crear la agenda (HTTP {(int)resp.StatusCode})."
                : text);

        var created = System.Text.Json.JsonSerializer.Deserialize<AgendaDto>(
            text, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (created is null)
            throw new InvalidOperationException("El servidor no devolvió los datos de la agenda creada.");

        return created;
    }




    public async Task<bool> EliminarAsync(int id, bool deleteGoogle = false, int? usuarioId = null)
    {
        var url = $"api/agenda/{id}" + (deleteGoogle
            ? $"?deleteGoogle=true{(usuarioId.HasValue ? $"&usuarioId={usuarioId.Value}" : "")}"
            : "");
        var resp = await _http.DeleteAsync(url);
        return resp.IsSuccessStatusCode || resp.StatusCode == HttpStatusCode.NotFound;
    }

    public async Task ActualizarAsync(int id, ActualizarAgendaDto dto)
    {
        // 🔍 Serializamos a JSON para debug
        var jsonDebug = JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        System.Diagnostics.Debug.WriteLine("📤 JSON enviado a backend:");
        System.Diagnostics.Debug.WriteLine(jsonDebug);

        // Enviar la request
        var response = await _http.PatchAsJsonAsync($"api/agenda/actualizar/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var resp = await _http.DeleteAsync($"api/agenda/{id}");
        return resp.IsSuccessStatusCode || resp.StatusCode == HttpStatusCode.NotFound;
    }









    public async Task<List<AgendaDto>> BuscarAsync(
       DateOnly? fecha = null,
       DateOnly? desde = null,
       DateOnly? hasta = null,
       IEnumerable<int>? usuarios = null,
       bool incluirParaTodos = true,
       EstadoAgenda? estado = null,
       string? q = null)
    {
        var qs = new List<string>();

        // Fecha única o rango (si fecha viene, se usa eso)
        if (fecha.HasValue)
        {
            qs.Add($"fecha={fecha.Value:yyyy-MM-dd}");
        }
        else
        {
            if (desde.HasValue) qs.Add($"desde={desde.Value:yyyy-MM-dd}");
            if (hasta.HasValue) qs.Add($"hasta={hasta.Value:yyyy-MM-dd}");
        }

        // Usuarios como claves repetidas: ?usuarios=5&usuarios=7
        if (usuarios is not null)
        {
            foreach (var u in usuarios) qs.Add($"usuarios={u}");
        }

        // Incluir "para todos"
        qs.Add($"incluirParaTodos={incluirParaTodos.ToString().ToLower()}");

        // Estado (enum). Podés mandar el número o el nombre; el binder acepta ambos.
        if (estado.HasValue) qs.Add($"estado={(int)estado.Value}");

        // Texto libre
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q)}");

        var url = "/api/agenda/buscar" + (qs.Count > 0 ? "?" + string.Join("&", qs) : string.Empty);
        return await _http.GetFromJsonAsync<List<AgendaDto>>(url) ?? new();
    }

    // Helpers opcionales si querés APIs más específicas:
    public Task<List<AgendaDto>> BuscarPorFechaAsync(
        DateOnly fecha,
        IEnumerable<int>? usuarios = null,
        bool incluirParaTodos = true,
        EstadoAgenda? estado = null,
        string? q = null)
        => BuscarAsync(fecha: fecha, usuarios: usuarios, incluirParaTodos: incluirParaTodos, estado: estado, q: q);

    public Task<List<AgendaDto>> BuscarPorRangoAsync(
        DateOnly? desde,
        DateOnly? hasta,
        IEnumerable<int>? usuarios = null,
        bool incluirParaTodos = true,
        EstadoAgenda? estado = null,
        string? q = null)
        => BuscarAsync(desde: desde, hasta: hasta, usuarios: usuarios, incluirParaTodos: incluirParaTodos, estado: estado, q: q);


}
