using IurixBlazor.Pages;
using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

public class GestionBase : ComponentBase
{
    [Inject] protected GestionService GestionService { get; set; } = default!;
    [Inject] protected UsuarioService UsuarioService { get; set; } = default!;
    [Inject] protected PersonaService PersonaService { get; set; } = default!;
    [Inject] protected ProcuracionService ProcuracionService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected List<GestionDto>? Gestiones;


    protected int? FiltroResponsableId { get; set; }
    protected string? FiltroTipo { get; set; }
    protected string? FiltroNombreProceso { get; set; }
    protected int? FiltroPersonaId { get; set; }

    protected List<PersonaDto> Personas { get; set; } = new();
    protected List<UsuarioDto> Responsables { get; set; } = new();


    //Marca cuando syncronizamos 1 proceso, ver para varios

    //// Marcamos qué proceso está sincronizando
    //protected HashSet<int> _syncing = new();

    //protected bool IsSyncing(int id) => _syncing.Contains(id);


    protected GestionModal? _gestionModal;






    protected bool esMovil = false;





    protected override async Task OnInitializedAsync()
    {
        //borramos la pagina de rotorno del editor a los movimientos del proceso
        await JS.InvokeVoidAsync("localStorage.removeItem", "volverADespuesDeEditor");

        //Gestiones = await GestionService.ObtenerTodasAsync();
        // Carga inicial sin filtros (trae todas)
        Gestiones = await GestionService.FiltrarGestionesAsync(null, null, null, null);

        // Cargar combos de filtros
        Responsables = await UsuarioService.ObtenerUsuariosAsync();
        Personas = await PersonaService.ObtenerPersonasAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var width = await JS.InvokeAsync<int>("obtenerAnchoPantalla");
            esMovil = width < 768; // breakpoint Bootstrap sm/md

            StateHasChanged();
        }
    }



    protected void NuevaGestion() => Nav.NavigateTo("/gestiones/crear-editar");

    protected void EditarGestion(int id) => Nav.NavigateTo($"/gestiones/crear-editar/{id}");


    //protected async Task BuscarGestiones()
    //{
    //    Gestiones = await GestionService.FiltrarGestionesAsync(
    //        FiltroResponsableId,
    //        FiltroTipo,
    //        FiltroNombreProceso,
    //        FiltroPersonaId
    //    );
    //}

    protected async Task BuscarGestiones()
    {
        var texto = string.IsNullOrWhiteSpace(FiltroNombreProceso)
            ? null
            : FiltroNombreProceso.Trim();

        Gestiones = await GestionService.FiltrarGestionesAsync(
            FiltroResponsableId,
            FiltroTipo,
            texto,
            FiltroPersonaId
        );
    }

    protected async Task HandleEnter(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" || e.Code == "NumpadEnter")
        {
            await BuscarGestiones();
        }
    }



    //Pasamos la funcion a Movimientos por proceso

    //protected async Task SyncUno(int procesoJudicialId)
    //{
    //    _syncing.Add(procesoJudicialId);
    //    StateHasChanged();        // refresca la UI
    //    try
    //    {
    //        var r = await ProcuracionService.SyncUnoAsync(procesoJudicialId);
    //        await JS.InvokeVoidAsync("alert",
    //            r is null ? "Sin respuesta" :
    //            $"Expediente: {r.ExpedienteUi}\nNovedades nuevas: {r.Nuevos}\nActuaciones (vista Dextra): {r.Total}");
    //        await BuscarGestiones();
    //    }
    //    catch (Exception ex)
    //    {
    //        await JS.InvokeVoidAsync("alert", $"Error: {ex.Message}");
    //    }
    //    finally
    //    {
    //        _syncing.Remove(procesoJudicialId);
    //        StateHasChanged();
    //    }
    //}



    //Ver esta funcion acomodarla para trabajar por listas de exptes
    //Sincronizar todos los exptes puede demorar mucho o romper la funcion
    //Deberiamos ver como hacer listados de exptes , varios id que los vaya pasando 
    //de a uno
    protected async Task SyncTodos()
    {
        if (!await JS.InvokeAsync<bool>("confirm", "¿Sincronizar TODOS los procesos judiciales?")) return;

        try
        {
            var r = await ProcuracionService.SyncTodosAsync();
            if (r is null) { await JS.InvokeVoidAsync("alert", "Sin respuesta"); return; }

            var errores = r.Errores?.Count > 0 ? $"\nErrores: {r.Errores.Count}" : "";
            await JS.InvokeVoidAsync("alert",
                $"Procesados: {r.Procesados}\nCon novedades: {r.ConNovedades}\nSin novedades: {r.SinNovedades}{errores}");

            await BuscarGestiones();
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("alert", $"Error: {ex.Message}");
        }
    }




    protected async Task EliminarGestion(int id)
    {
        var confirmar = await JS.InvokeAsync<bool>("confirm", "¿Eliminar esta gestión?");
        if (confirmar)
        {
            await GestionService.EliminarAsync(id);
            Gestiones = await GestionService.ObtenerTodasAsync();
        }
    }






    protected async Task IrAMovimientos(string tipo, int gestionId, int procesoId)
    {
        // Guardamos info básica para después
        await JS.InvokeVoidAsync("localStorage.setItem", "gestion_actual", gestionId.ToString());
        await JS.InvokeVoidAsync("localStorage.setItem", "proceso_actual", procesoId.ToString());
        await JS.InvokeVoidAsync("localStorage.setItem", "tipo_proceso", tipo);

        var url = tipo == "Judicial"
            ? $"/movimientos/judicial/{procesoId}"
            : $"/movimientos/extrajudicial/{procesoId}";

        Nav.NavigateTo(url);
    }

    //protected void IrAMovimientosJudicial(int id) => IrAMovimientos("Judicial", id);
    //protected void IrAMovimientosExtrajudicial(int id) => IrAMovimientos("Extrajudicial", id);

    // Helpers que solo delegan, con misma firma (gestionId, procesoId)
    protected Task IrAMovimientosJudicial(int gestionId, int procesoId)
        => IrAMovimientos("Judicial", gestionId, procesoId);

    protected Task IrAMovimientosExtrajudicial(int gestionId, int procesoId)
        => IrAMovimientos("Extrajudicial", gestionId, procesoId);
}
