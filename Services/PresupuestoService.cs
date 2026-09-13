using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using static System.Net.WebRequestMethods;


namespace IurixBlazor.Services; 
public class PresupuestoService
{
    private readonly HttpClient _httpClient;

    public PresupuestoService(IHttpClientFactory factory, ConfigService config)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _httpClient = factory.CreateClient("Api");
    }

    public async Task<List<PresupuestoDto>> ObtenerTodosAsync()
        => await _httpClient.GetFromJsonAsync<List<PresupuestoDto>>("api/Presupuestos") ?? new();

    public async Task<PresupuestoDto?> ObtenerPorIdAsync(int id)
        => await _httpClient.GetFromJsonAsync<PresupuestoDto>($"api/Presupuestos/{id}");

    public async Task<List<PresupuestoDto>> ObtenerPorPersonaAsync(int personaId)
    {
        var response = await _httpClient.GetAsync($"api/Presupuestos/por-persona/{personaId}");
        if (!response.IsSuccessStatusCode) return new List<PresupuestoDto>();
        return await response.Content.ReadFromJsonAsync<List<PresupuestoDto>>() ?? new List<PresupuestoDto>();
    }



    public async Task<List<PresupuestoDto>> BuscarAsync(DateTime? desde, DateTime? hasta, int? personaId, EstadoPresupuesto? estado)
    {
        var qs = new List<string>();
        if (desde.HasValue) qs.Add($"desde={desde.Value:yyyy-MM-dd}");
        if (hasta.HasValue) qs.Add($"hasta={hasta.Value:yyyy-MM-dd}");
        if (personaId.HasValue && personaId.Value > 0) qs.Add($"personaId={personaId.Value}");
        if (estado.HasValue) qs.Add($"estado={(int)estado.Value}");

        var url = "api/Presupuestos/buscar" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");
        var res = await _httpClient.GetFromJsonAsync<List<PresupuestoDto>>(url);
        return res ?? new List<PresupuestoDto>();
    }








    public async Task<PresupuestoDto> CrearAsync(PresupuestoDto dto)
    {
        var crearDto = new CrearPresupuestoDto
        {
            Nombre = dto.Nombre,
            PersonaId = dto.PersonaId,
            Neto = dto.Neto,
            PlazoValidezDias = dto.PlazoValidezDias,
            Observaciones = dto.Observaciones,
            MarcarEnviado = dto.MarcarEnviado,
            Iva = dto.Iva,
            Total = dto.Total,
            Estado = dto.Estado,
            Fecha = dto.Fecha.Kind == DateTimeKind.Utc ? dto.Fecha : dto.Fecha.ToUniversalTime(),
            DistribucionHonorarioId = dto.DistribucionHonorarioId,
            Detalles = dto.Detalles.Select(d => new CrearPresupuestoDetalleDto
            {
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                IvaAlicuotaId = d.IvaAlicuotaId,  //  Incluimos IVA
                AjusteVariableId = d.AjusteVariableId,
                VariableCoef = d.VariableCoef,
                VariableValorUsado = d.VariableValorUsado

            }).ToList()
        };

        var json = JsonSerializer.Serialize(crearDto, new JsonSerializerOptions { WriteIndented = true });
        System.Diagnostics.Debug.WriteLine("📤 JSON enviado a API/Presupuestos:\n" + json);

        var resp = await _httpClient.PostAsJsonAsync("api/Presupuestos", crearDto);

        if (!resp.IsSuccessStatusCode)
        {
            var errorContent = await resp.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"❌ Error al crear presupuesto: {resp.StatusCode}\n{errorContent}");
        }

        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<PresupuestoDto>();
    }



    // Preview (aplicar = false)
    public async Task<RevalorizarRespuestaDto?> PrevisualizarRevalorizarAsync(int presupuestoId)
    {
        var url = $"api/Presupuestos/{presupuestoId}/revalorizar?aplicar=false";
        var res = await _httpClient.PostAsync(url, content: null);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<RevalorizarRespuestaDto>();
    }

    // Aplicar (aplicar = true)
    public async Task<RevalorizarRespuestaDto?> RevalorizarAsync(int presupuestoId, bool aplicar = true)
    {
        var url = $"api/Presupuestos/{presupuestoId}/revalorizar?aplicar={(aplicar ? "true" : "false")}";
        var res = await _httpClient.PostAsync(url, content: null);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<RevalorizarRespuestaDto>();
    }





    public async Task ActualizarAsync(int id, PresupuestoDto dto)
    {
        var patchDto = new PresupuestoPatchDto
        {
            Nombre = dto.Nombre,
            PersonaId = dto.PersonaId,
            Neto = dto.Neto,
            PlazoValidezDias = dto.PlazoValidezDias,
            Observaciones = dto.Observaciones,
            MarcarEnviado = dto.MarcarEnviado,
            Iva = dto.Iva,
            Total = dto.Total,
            Estado = dto.Estado,
            Fecha = dto.Fecha.ToUniversalTime(),
            DistribucionHonorarioId = dto.DistribucionHonorarioId,
            Detalles = dto.Detalles.Select(d => new PresupuestoDetallePatchDto
            {
                Id = d.Id > 0 ? d.Id : null,
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                IvaAlicuotaId = d.IvaAlicuotaId,  //  Incluimos IVA
                AjusteVariableId = d.AjusteVariableId,
                VariableCoef = d.VariableCoef,
                VariableValorUsado = d.VariableValorUsado

            }).ToList()
        };

        var json = JsonSerializer.Serialize(patchDto, new JsonSerializerOptions { WriteIndented = true });
        Debug.WriteLine("📤 JSON enviado a API/Presupuestos (PATCH):\n" + json);

        var resp = await _httpClient.PatchAsJsonAsync($"api/Presupuestos/{id}", patchDto);
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadAsStringAsync();
            Debug.WriteLine($"❌ Error al actualizar presupuesto: {resp.StatusCode}\n{error}");
        }

        resp.EnsureSuccessStatusCode();
    }





   

    public async Task ActualizarEstadoAsync(int id, EstadoPresupuesto nuevo)
    {
        var payload = new { estado = nuevo };
        var resp = await _httpClient.PatchAsJsonAsync($"api/presupuestos/{id}/estado", payload);

        if (resp.IsSuccessStatusCode || resp.StatusCode == System.Net.HttpStatusCode.NoContent)
            return;

        var msg = await resp.Content.ReadAsStringAsync();
        throw new HttpRequestException(
            string.IsNullOrWhiteSpace(msg) ? "No se pudo actualizar el estado." : msg,
            null,
            resp.StatusCode);
    }


    // listado para el modal
    public async Task<List<PresupuestoResumenDto>> BuscarParaFacturarAsync(int personaId)
        => await _httpClient.GetFromJsonAsync<List<PresupuestoResumenDto>>(
            $"api/presupuestos/persona/{personaId}/para-facturar") ?? new();



    public async Task<(decimal Total, decimal YaFacturado, decimal Pendiente)> ObtenerPendienteAsync(int presupuestoId)
    {
        var dto = await _httpClient.GetFromJsonAsync<PendientePresupuestoDto>($"api/presupuestos/{presupuestoId}/pendiente-facturar");
        return dto is null ? (0m, 0m, 0m) : (dto.Total, dto.YaFacturado, dto.Pendiente);
    }



    public Task<List<PresupuestoDetallePendienteDto>> ObtenerPendientesPorDetalleAsync(int presupuestoId)
    => _httpClient.GetFromJsonAsync<List<PresupuestoDetallePendienteDto>>(
           $"api/presupuestos/{presupuestoId}/pendientes-detalle")!;


    //public async Task<bool> MarcarEnviadoAsync(int presupuestoId, string proveedor = "gmail")
    //{
    //    var payload = new { ok = true, proveedor };
    //    var resp = await _httpClient.PatchAsJsonAsync($"api/presupuestos/{presupuestoId}/marcar-enviado", payload);
    //    return resp.IsSuccessStatusCode;
    //}

    public async Task MarcarEnviadoAsync(int id, MarcarEnviadoDto dto)
    {
        var req = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/presupuestos/{id}/marcar-enviado")
        {
            Content = JsonContent.Create(dto) // { Ok = true, MessageId = ... }
        };

        var resp = await _httpClient.SendAsync(req);
        resp.EnsureSuccessStatusCode();
    }



    // Blazor (front) - PresupuestoService.cs
    public async Task<byte[]> ObtenerPdfAsync(int id)
    {
        var r = await _httpClient.GetAsync($"api/Presupuestos/{id}/pdf");
        r.EnsureSuccessStatusCode();
        return await r.Content.ReadAsByteArrayAsync();
    }



    public async Task<byte[]?> ObtenerPdfPreviewAsync(int id, string? introHtml, string? outroHtml)
    {
        var dto = new { IntroHtml = introHtml, OutroHtml = outroHtml };
        var resp = await _httpClient.PostAsJsonAsync($"api/presupuestos/{id}/pdf-preview", dto);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadAsByteArrayAsync(); // viene application/pdf
    }





    public async Task EliminarAsync(int id)
    {
        var resp = await _httpClient.DeleteAsync($"api/Presupuestos/{id}");
        resp.EnsureSuccessStatusCode();
    }

    //  DETALLES
    public async Task<List<PresupuestoDetalleDto>> ObtenerDetallesAsync(int presupuestoId)
        => await _httpClient.GetFromJsonAsync<List<PresupuestoDetalleDto>>($"api/PresupuestoDetalles/porPresupuesto/{presupuestoId}") ?? new();

    public async Task CrearDetalleAsync(PresupuestoDetalleDto dto)
    {
        var resp = await _httpClient.PostAsJsonAsync("api/PresupuestoDetalles", dto);
        resp.EnsureSuccessStatusCode();
    }





    public async Task<byte[]> ObtenerPdf2Async(int presupuestoId)
    {
        var resp = await _httpClient.GetAsync($"api/Presupuestos/{presupuestoId}/pdf2");
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsByteArrayAsync();
    }




    public async Task EliminarDetalleAsync(int id)
    {
        var resp = await _httpClient.DeleteAsync($"api/PresupuestoDetalles/{id}");
        resp.EnsureSuccessStatusCode();
    }
}

