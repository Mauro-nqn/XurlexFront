using IurixBlazor.Pages;
using IurixBlazor.Services;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;
using System.Net;
using System.Net.Http;
using static Org.BouncyCastle.Math.EC.ECCurve;
using static System.Net.WebRequestMethods;

public class CuentaCorrienteService
{
    private readonly HttpClient _httpClient;

    public enum CargoPresupuestoResult { Creado, YaExistia }

    public CuentaCorrienteService(IHttpClientFactory factory, ConfigService config)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _httpClient = factory.CreateClient("Api");
    }

    // --- Cuenta Corriente ---
    //public Task<CuentaCorrienteDto?> ObtenerPorPersonaAsync(int personaId)
    //    => _httpClient.GetFromJsonAsync<CuentaCorrienteDto>($"api/CuentasCorrientes/persona/{personaId}");

    public async Task<CuentaCorrienteDto?> ObtenerPorPersonaAsync(int personaId)
    {
        var resp = await _httpClient.GetAsync($"api/CuentasCorrientes/persona/{personaId}");
        if (resp.StatusCode == HttpStatusCode.NotFound)
            return null; // <- sin cuenta, devolvemos null

        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<CuentaCorrienteDto>();
    }

    //public async Task<CuentaCorrienteDto?> CrearAsync(int personaId, DateTime? fechaApertura = null)
    //{
    //    var dto = new CrearCuentaCorrienteDto { PersonaId = personaId, FechaApertura = fechaApertura ?? DateTime.Now };
    //    var r = await _httpClient.PostAsJsonAsync("api/CuentasCorrientes", dto);
    //    r.EnsureSuccessStatusCode();
    //    return await r.Content.ReadFromJsonAsync<CuentaCorrienteDto>();
    //}

    // CuentaCorrienteService.cs
    public async Task<CuentaCorrienteDto?> CrearAsync(
        int personaId,
        DateTime? fechaApertura = null,
        decimal saldoApertura = 0m,
        bool activa = true,
        decimal? limiteCredito = null)
    {
        var dto = new CrearCuentaCorrienteDto
        {
            PersonaId = personaId,
            FechaApertura = (fechaApertura ?? DateTime.Now).ToUniversalTime(),
            SaldoApertura = saldoApertura,
            Activa = activa,
            LimiteCredito = limiteCredito
        };
        var r = await _httpClient.PostAsJsonAsync("api/CuentasCorrientes", dto);
        r.EnsureSuccessStatusCode();
        return await r.Content.ReadFromJsonAsync<CuentaCorrienteDto>();
    }

    // --- Movimientos ---
    public Task<List<MovimientoCuentaDto>?> ObtenerMovimientosPorCuentaAsync(int cuentaId)
        => _httpClient.GetFromJsonAsync<List<MovimientoCuentaDto>>($"api/MovimientosCuenta/cuenta/{cuentaId}");



    public async Task<MovimientoCuentaDto?> CrearMovimientoAsync(CrearMovimientoCuentaDto dto)
    {
        var r = await _httpClient.PostAsJsonAsync("api/MovimientosCuenta", dto);
        r.EnsureSuccessStatusCode();
        return await r.Content.ReadFromJsonAsync<MovimientoCuentaDto>();
    }


    public async Task EliminarMovimientoAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/MovimientosCuenta/{id}");
        response.EnsureSuccessStatusCode();
    }

    // Utilidad: calcula débitos abiertos consultando imputaciones
    // IurixBlazor/Services/CuentaCorrienteService.cs
    public async Task<List<DebitoAbiertoDto>> ObtenerDebitosAbiertosAsync(int cuentaId)
    {
        var movs = await ObtenerMovimientosPorCuentaAsync(cuentaId) ?? new();
        var debitos = movs.Where(m => m.Tipo == TipoMovimientoCuenta.Debito).ToList();

        var list = new List<DebitoAbiertoDto>();
        foreach (var d in debitos)
        {
            var imps = await _httpClient.GetFromJsonAsync<List<ImputacionDto>>($"api/Imputaciones/cargo/{d.Id}") ?? new();
            var imputado = imps.Sum(i => i.Importe ?? 0m);
            var saldo = (d.Importe ?? 0m) - imputado;
            if (saldo > 0)
                list.Add(new DebitoAbiertoDto { Debito = d, Saldo = saldo });
        }

        return list
            .OrderByDescending(x => x.Debito.FacturaId.HasValue)
            .ThenBy(x => x.Debito.Fecha)
            .ThenBy(x => x.Debito.Id)   // 👈 Id es int, sin .Value
            .ToList();
    }


    // --- Imputaciones ---
    public async Task<ImputacionDto?> CrearImputacionAsync(CrearImputacionDto dto)
    {
        var r = await _httpClient.PostAsJsonAsync("api/Imputaciones", dto);
        r.EnsureSuccessStatusCode();
        return await r.Content.ReadFromJsonAsync<ImputacionDto>();
    }

    // --- Atajo: Cargo por Presupuesto al aprobar ---
    //public async Task CrearCargoPresupuestoAsync(int personaId, int presupuestoId, decimal importe)
    //{
    //    // Garantizar que exista la cuenta
    //    var cta = await ObtenerPorPersonaAsync(personaId) ?? await CrearAsync(personaId);

    //    // 2) crea el movimiento (débito) por presupuesto
    //    var mov = new CrearMovimientoCuentaDto
    //    {
    //        CuentaCorrienteId = cta!.Id,
    //        Fecha = DateTime.Now,
    //        Tipo = TipoMovimientoCuenta.Debito,
    //        Importe = importe,
    //        Detalle = $"Presupuesto #{presupuestoId}",
    //        OrigenTipo = OrigenMovimientoCuenta.Presupuesto,
    //        OrigenId = presupuestoId,
    //        PresupuestoId = presupuestoId
    //    };
    //    await CrearMovimientoAsync(mov);
    //}


    //public async Task CrearCargoPresupuestoAsync(int personaId, int presupuestoId, decimal importe)
    //{
    //    var payload = new { PersonaId = personaId, PresupuestoId = presupuestoId, Importe = importe };
    //    var r = await _httpClient.PostAsJsonAsync("api/CuentasCorrientes/cargar-presupuesto", payload);
    //    if (r.StatusCode == System.Net.HttpStatusCode.Conflict) return; // ya existe = OK
    //    r.EnsureSuccessStatusCode();
    //}


    //public async Task RevertirCargoPresupuestoAsync(int personaId, int presupuestoId)
    //{
    //    var payload = new { PersonaId = personaId, PresupuestoId = presupuestoId };
    //    var r = await _httpClient.PostAsJsonAsync("api/CuentasCorrientes/revertir-presupuesto", payload);
    //    if (r.StatusCode == System.Net.HttpStatusCode.Conflict)
    //        throw new InvalidOperationException(await r.Content.ReadAsStringAsync());
    //    r.EnsureSuccessStatusCode();
    //}



    // CuentaCorrienteService.cs
    public async Task<bool> ExisteCargoPresupuestoAsync(int personaId, int presupuestoId)
    {
        var r = await _httpClient.GetAsync($"api/CuentasCorrientes/persona/{personaId}/tiene-cargo-presupuesto/{presupuestoId}");
        r.EnsureSuccessStatusCode();
        var flag = await r.Content.ReadFromJsonAsync<bool>();
        return flag;
    }



    public async Task<CargoPresupuestoResult> CrearCargoPresupuestoAsync(int personaId, int presupuestoId, decimal importe)
    {
        var payload = new { PersonaId = personaId, PresupuestoId = presupuestoId, Importe = importe };
        var r = await _httpClient.PostAsJsonAsync("api/CuentasCorrientes/cargar-presupuesto", payload);

        if (r.StatusCode == System.Net.HttpStatusCode.Conflict)
            return CargoPresupuestoResult.YaExistia;

        r.EnsureSuccessStatusCode();
        return CargoPresupuestoResult.Creado;
    }




    public async Task RevertirCargoPresupuestoAsync(int personaId, int presupuestoId)
    {
        var payload = new { PersonaId = personaId, PresupuestoId = presupuestoId };
        var r = await _httpClient.PostAsJsonAsync("api/CuentasCorrientes/revertir-presupuesto", payload);

        if (r.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var msg = await r.Content.ReadAsStringAsync(); // texto del back
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(msg)
                ? "No se puede eliminar: el cargo ya tiene imputaciones."
                : msg);
        }

        r.EnsureSuccessStatusCode();
    }


    //public Task ReversarCargoPresupuestoAsync(int personaId, int presupuestoId)
    //=> _httpClient.PostAsJsonAsync("api/CuentasCorrientes/reversar-presupuesto",
    //   new { PersonaId = personaId, PresupuestoId = presupuestoId }).ContinueWith(t => {
    //       var r = t.Result; r.EnsureSuccessStatusCode();
    //   });




    // CuentaCorrienteService.cs
    public async Task<CuentaCorrienteDto?> PatchAsync(int cuentaId, bool? activa, decimal? limiteCredito)
    {
        var dto = new { Activa = activa, LimiteCredito = limiteCredito };
        var r = await _httpClient.PatchAsJsonAsync($"api/CuentasCorrientes/{cuentaId}", dto);
        r.EnsureSuccessStatusCode();
        return await r.Content.ReadFromJsonAsync<CuentaCorrienteDto>();
    }


    public async Task AjustarPresupuestoAsync(int personaId, int presupuestoId, decimal nuevoTotal)
    {
        var payload = new { PersonaId = personaId, PresupuestoId = presupuestoId, NuevoTotal = nuevoTotal };
        var r = await _httpClient.PostAsJsonAsync("api/CuentasCorrientes/ajustar-presupuesto", payload);
        if (r.StatusCode == System.Net.HttpStatusCode.Conflict)
            throw new HttpRequestException(await r.Content.ReadAsStringAsync(), null, System.Net.HttpStatusCode.Conflict);
        r.EnsureSuccessStatusCode();
    }
}

