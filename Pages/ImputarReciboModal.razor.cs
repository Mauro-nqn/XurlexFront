using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static CtaCteClienteBase;

public class ImputarReciboModalBase : ComponentBase
{
    [Inject] protected CuentaCorrienteService CtaSrv { get; set; } = default!;
    [Inject] protected ReciboService ReciboSrv { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    // Estado interno
    protected bool Visible { get; set; }
    protected int MovimientoCreditoId { get; set; }
    protected decimal SaldoDisponible { get; set; }
    protected List<DebitoAbiertoRow> DebitosAbiertosModal { get; set; } = new();
    protected decimal TotalAImputar { get; set; }
    protected string? ValidacionError { get; set; }

    protected int _reciboId;
    protected int _personaId;

    [Parameter] public EventCallback OnImputado { get; set; }

    public async Task AbrirAsync(int reciboId)
    {
        // Limpiamos estado anterior
        Visible = false;
        _reciboId = reciboId;
        _personaId = 0;
        MovimientoCreditoId = 0;
        SaldoDisponible = 0m;
        DebitosAbiertosModal.Clear();
        TotalAImputar = 0m;
        ValidacionError = null;

        // 1) Traer vista del recibo
        ReciboVistaDto? vista;
        try
        {
            vista = await ReciboSrv.ObtenerVistaAsync(reciboId);
        }
        catch (HttpRequestException ex)
        {
            await Toast($"No se pudo cargar el recibo. {ex.Message}", "error");
            return;
        }

        if (vista is null)
        {
            await Toast("No se encontró la vista del recibo.", "error");
            return;
        }

        if (!vista.Recibo.PersonaId.HasValue)
        {
            await Toast("El recibo no tiene asociado un cliente válido.", "warning");
            return;
        }

        _personaId = vista.Recibo.PersonaId.Value;
        SaldoDisponible = vista.SaldoResultante;

        // 2) Cargar datos de imputación (movimiento + débitos abiertos)
        var ok = await CargarDatosImputacionAsync();
        if (!ok) return;

        // Si todo salió bien, ahora sí mostramos el modal
        Visible = true;
        StateHasChanged();
    }

    protected async Task<bool> CargarDatosImputacionAsync()
    {
        // 2.1) obtener MovimientoCreditoId del recibo
        try
        {
            MovimientoCreditoId = await ReciboSrv.ObtenerMovimientoCreditoIdAsync(_reciboId);
        }
        catch (HttpRequestException ex)
        {
            await Toast($"No se pudo obtener el movimiento del recibo. {ex.Message}", "error");
            return false;
        }

        // 2.2) obtener cuenta y débitos abiertos
        try
        {
            var cuenta = await CtaSrv.ObtenerPorPersonaAsync(_personaId);

            if (cuenta is null)
            {
                await Toast("El cliente no tiene cuenta corriente.", "warning");
                return false;
            }

            var debs = await CtaSrv.ObtenerDebitosAbiertosAsync(cuenta.Id);
            DebitosAbiertosModal = debs.Select(d => new DebitoAbiertoRow
            {
                Debito = d.Debito,
                Saldo = d.Saldo,
                Imputar = 0m
            }).ToList();
        }
        catch (HttpRequestException ex)
        {
            await Toast($"No se pudieron cargar débitos abiertos. {ex.Message}", "error");
            return false;
        }

        ValidacionError = null;
        TotalAImputar = 0m;
        return true;
    }

    protected bool PuedeConfirmarImputacion =>
        DebitosAbiertosModal.Count > 0 &&
        TotalAImputar > 0 &&
        TotalAImputar <= SaldoDisponible &&
        string.IsNullOrEmpty(ValidacionError);

    protected async Task Toast(string msg, string type = "info")
        => await JS.InvokeVoidAsync("mostrarToast", msg, type);

    protected void OnImputarChanged(DebitoAbiertoRow row, ChangeEventArgs e)
    {
        if (e?.Value is null) return;

        if (!decimal.TryParse(e.Value.ToString(), out var val)) val = 0m;

        if (val < 0) val = 0m;
        if (val > row.Saldo) val = row.Saldo;

        row.Imputar = val;

        TotalAImputar = DebitosAbiertosModal.Sum(x => x.Imputar);

        if (TotalAImputar > SaldoDisponible)
            ValidacionError = $"La suma a imputar ({TotalAImputar:C}) supera el saldo disponible del recibo ({SaldoDisponible:C}).";
        else
            ValidacionError = null;

        StateHasChanged();
    }

    protected async Task ConfirmarImputacionesRecibo()
    {
        if (!PuedeConfirmarImputacion || MovimientoCreditoId == 0) return;

        try
        {
            foreach (var x in DebitosAbiertosModal.Where(r => r.Imputar > 0))
            {
                await CtaSrv.CrearImputacionAsync(new CrearImputacionDto
                {
                    MovimientoCreditoId = MovimientoCreditoId,
                    MovimientoCargoId = x.Debito.Id,
                    Importe = x.Imputar,
                    OrigenTipo = "Recibo",
                    OrigenId = _reciboId
                });
            }

            await Toast("Imputación realizada.", "success");
            CerrarImputar();

            if (OnImputado.HasDelegate)
                await OnImputado.InvokeAsync();
        }
        catch (HttpRequestException ex)
        {
            await Toast($"No se pudo imputar. {ex.Message}", "error");
        }
    }

    protected void CerrarImputar()
    {
        Visible = false;
        DebitosAbiertosModal.Clear();
        TotalAImputar = 0m;
        MovimientoCreditoId = 0;
        ValidacionError = null;

        StateHasChanged();
    }
}

