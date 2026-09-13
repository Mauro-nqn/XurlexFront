using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;

public class MovimientosEmbedBase : ComponentBase
{


    [Inject] protected MovimientoService MovimientoService { get; set; } = default!;
    [Inject] protected GestionService GestionService { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected DextraService DextraService { get; set; } = default!;

    [Parameter] public string Tipo { get; set; } = "Judicial";
    [Parameter] public int ProcesoId { get; set; }

    [Parameter] public EventCallback OnSaved { get; set; }   // <-- requerido
    [Parameter] public EventCallback OnClosed { get; set; }  // <-- si lo pasás en markup, también


    [Parameter] public string ParentModalSelector { get; set; } = "#gestionModal";

    protected AdjuntosModalBase? _adjuntosModal;
    protected VerMovimientoModalBase? _movModal;

    protected List<MovimientoDto> Movimientos { get; set; } = new();
    protected bool Cargando { get; set; }
    protected ProcesoInfoDto? DatosProceso { get; set; }

    protected HashSet<int> _syncing = new();
    protected bool IsSyncing(int id) => _syncing.Contains(id);

    protected bool HasParent => !string.IsNullOrWhiteSpace(ParentModalSelector);

    protected bool PuedeSincronizar =>
        string.Equals(Tipo, "Judicial", StringComparison.OrdinalIgnoreCase)
        && DatosProceso is not null
        && ((DatosProceso.JuzgadoAplicacionDextraId ?? 0) > 0
            || !string.IsNullOrWhiteSpace(DatosProceso.JuzgadoDextraAplicacionExacta));

    protected override async Task OnParametersSetAsync()
    {
        Cargando = true;
        try
        {
            Movimientos = await MovimientoService.ObtenerPorProcesoAsync(Tipo, ProcesoId);
            DatosProceso = await GestionService.ObtenerDatosProcesoAsync(Tipo, ProcesoId);
        }
        finally { Cargando = false; }
    }

    protected void IrAListaCompleta()
        => Nav.NavigateTo(Tipo == "Judicial"
            ? $"/movimientos/judicial/{ProcesoId}"
            : $"/movimientos/extrajudicial/{ProcesoId}");


    protected async Task SyncUno(int procesoJudicialId)
    {
        _syncing.Add(procesoJudicialId);
        StateHasChanged();
        try
        {
            var r = await DextraService.SyncUnoAsync(procesoJudicialId);
            await JS.InvokeVoidAsync("alert",
                r is null ? "Sin respuesta"
                          : $"Expediente: {r.ExpedienteUi}\nNuevos: {r.Nuevos}\nActuaciones: {r.Total}");
            Movimientos = await MovimientoService.ObtenerPorProcesoAsync(Tipo, ProcesoId);
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("alert", $"Error: {ex.Message}");
        }
        finally
        {
            _syncing.Remove(procesoJudicialId);
            StateHasChanged();
        }
    }

    // ganchos básicos (podés integrarlos luego)
    protected async Task VerAdjuntos(int movId)
    {
        if (HasParent) await JS.InvokeVoidAsync("bsModal.hide", ParentModalSelector);
        await _adjuntosModal!.ShowAsync(movId);
        if (HasParent) await JS.InvokeVoidAsync("bsModal.show", ParentModalSelector);
    }
    protected async Task VerMovimiento(int id)
    {
        if (HasParent) await JS.InvokeVoidAsync("bsModal.hide", ParentModalSelector);
        await _movModal!.ShowAsync(id);
        if (HasParent) await JS.InvokeVoidAsync("bsModal.show", ParentModalSelector);
    }

    protected Task ReabrirPadre() =>
    JS.InvokeVoidAsync("bsModal.show", ParentModalSelector).AsTask();


    protected async Task RecargarDespuesDeEditar()
    {
        Movimientos = await MovimientoService.ObtenerPorProcesoAsync(Tipo, ProcesoId);
        StateHasChanged();
    }

    // Helpers adjuntos (ajustá al shape de tu DTO)
    protected static bool HayAdjuntos(MovimientoDto m) =>
        m.CantidadArchivos > 0;
    protected async Task EliminarMovimiento(int id)
    {
        if (await JS.InvokeAsync<bool>("confirm", "¿Eliminar este movimiento?"))
        {
            await MovimientoService.EliminarAsync(id);
            Movimientos = await MovimientoService.ObtenerPorProcesoAsync(Tipo, ProcesoId);
            StateHasChanged();
        }
    }
}
