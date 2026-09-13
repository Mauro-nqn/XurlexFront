using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using System.Globalization;

namespace IurixBlazor.Pages;

public class AgendasBase : ComponentBase
{
    [Inject] protected AgendaService AgendaService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    [Inject] protected UsuarioService UsuarioService { get; set; } = default!;
    [Inject] protected GoogleService GoogleService { get; set; } = default!;
    protected List<AgendaDto> Agendas { get; set; } = new();
    protected string FiltroBusqueda { get; set; } = string.Empty;

    protected string FiltroUsuarios { get; set; } = string.Empty;

    //  Nuevo: filtro por estado (nullable para permitir "Todos")
    protected EstadoAgenda? FiltroEstado { get; set; } = EstadoAgenda.Pendiente;

    // Antes: DateOnly FiltroFecha = DateOnly.FromDateTime(DateTime.Now);
    protected DateOnly? FiltroFecha { get; set; } = null;   // anulable

    // Rango
    protected bool UsarRango { get; set; } = false;
    protected DateOnly? FiltroDesde { get; set; } = null;
    protected DateOnly? FiltroHasta { get; set; } = null;

    protected bool FiltroTodosUsuarios = true;
    //protected HashSet<int> FiltroUsuarioIds = new();


    protected List<UsuarioDto> ListaUsuarios = new();
    protected HashSet<int> FiltroUsuarioIds = new();

    // Debounce simple para el buscador
    protected System.Timers.Timer? _debounce;

    private bool IsBusy;

    // default: más nuevo primero
    protected string OrdenFecha { get; set; } = "desc";

    protected bool esMovil = false;


    protected override async Task OnInitializedAsync()
    {
        Agendas = await AgendaService.ObtenerTodasAsync();
        ListaUsuarios = await UsuarioService.ObtenerUsuariosAsync(); // o tu método real

        // En OnInitializedAsync, si no hay query:
        //if (string.IsNullOrEmpty(Nav.ToAbsoluteUri(Nav.Uri).Query))
        //{
        //    var saved = await JS.InvokeAsync<string?>("sessionStorage.getItem", "agendas:filters");
        //    if (!string.IsNullOrWhiteSpace(saved))
        //        Nav.NavigateTo(saved, replace: true); // reconstruye desde el backup
        //}

        //ApplyFiltersFromQuery();  // 👈 reconstruye estado desde la URL, si hay
        //await BuscarAgendaAsync();

        // Defaults primero
        SetDefaultFilters();

        // Intentar restaurar desde URL o sessionStorage (si navega, cortar)
        var navigated = await TryRestoreFromSessionOrUrlAsync();
        if (navigated) return;

        await BuscarAgendaAsync();

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



    //private void ApplyFiltersFromQuery()
    //{
    //    var uri = Nav.ToAbsoluteUri(Nav.Uri);
    //    var q = QueryHelpers.ParseQuery(uri.Query);

    //    UsarRango = q.TryGetValue("usarRango", out var vr) && bool.TryParse(vr, out var br) && br;

    //    // fecha única
    //    FiltroFecha = null;
    //    if (!UsarRango && q.TryGetValue("fecha", out var vf) && DateOnly.TryParseExact(vf!, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d1))
    //        FiltroFecha = d1;

    //    // rango
    //    FiltroDesde = null; FiltroHasta = null;
    //    if (UsarRango && q.TryGetValue("desde", out var vd) && DateOnly.TryParseExact(vd!, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d2))
    //        FiltroDesde = d2;
    //    if (UsarRango && q.TryGetValue("hasta", out var vh) && DateOnly.TryParseExact(vh!, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d3))
    //        FiltroHasta = d3;

    //    // estado
    //    FiltroEstado = null;
    //    if (q.TryGetValue("estado", out var ve) && int.TryParse(ve!, out var est))
    //        FiltroEstado = (EstadoAgenda)est;

    //    // texto
    //    FiltroBusqueda = q.TryGetValue("q", out var vq) ? vq.ToString() : string.Empty;

    //    // usuarios
    //    FiltroUsuarioIds.Clear();
    //    FiltroTodosUsuarios = q.TryGetValue("todos", out var vt) && bool.TryParse(vt, out var bt) && bt;

    //    if (q.TryGetValue("usuarios", out var vu))
    //    {
    //        foreach (var s in vu)
    //            if (int.TryParse(s, out var id)) FiltroUsuarioIds.Add(id);

    //        if (FiltroUsuarioIds.Count > 0) FiltroTodosUsuarios = false;
    //    }
    //}


    protected void SetDefaultFilters()
    {
        UsarRango = false;
        FiltroFecha = null;
        FiltroDesde = null;
        FiltroHasta = null;
        FiltroEstado = EstadoAgenda.Pendiente;   //  queda seleccionado en el InputSelect
        FiltroBusqueda = string.Empty;
        FiltroTodosUsuarios = true;
        FiltroUsuarioIds.Clear();
    }

    // Devuelve true si navegamos (y por tanto hay que cortar el flujo)
    protected async Task<bool> TryRestoreFromSessionOrUrlAsync()
    {
        var hasQuery = !string.IsNullOrEmpty(Nav.ToAbsoluteUri(Nav.Uri).Query);

        if (!hasQuery)
        {
            var saved = await JS.InvokeAsync<string?>("sessionStorage.getItem", "agendas:filters");
            if (!string.IsNullOrWhiteSpace(saved))
            {
                Nav.NavigateTo(saved, replace: true);
                return true; // se dispara nueva renderización con esa URL
            }
        }

        // Si hay query, aplicar sobre los defaults (sobrescribe solo lo presente)
        ApplyFiltersFromQuery();
        return false;
    }

    protected void ApplyFiltersFromQuery()
    {
        var uri = Nav.ToAbsoluteUri(Nav.Uri);
        var q = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

        if (q.TryGetValue("usarRango", out var vr) && bool.TryParse(vr, out var br)) UsarRango = br;

        if (q.TryGetValue("sort", out var vs))
        {
            var s = vs.ToString().ToLowerInvariant();
            OrdenFecha = (s == "asc" || s == "desc") ? s : "desc";
        }
        else
        {
            OrdenFecha = "asc";
        }

        if (!UsarRango)
        {
            if (q.TryGetValue("fecha", out var vf) &&
                DateOnly.TryParseExact(vf!, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                                       System.Globalization.DateTimeStyles.None, out var d1))
                FiltroFecha = d1;
            else
                FiltroFecha = null; // si no vino, mantenemos “sin fecha”
                                    // limpio rango por las dudas
            FiltroDesde = null; FiltroHasta = null;
        }
        else
        {
            if (q.TryGetValue("desde", out var vd) &&
                DateOnly.TryParseExact(vd!, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                                       System.Globalization.DateTimeStyles.None, out var d2))
                FiltroDesde = d2;
            if (q.TryGetValue("hasta", out var vh) &&
                DateOnly.TryParseExact(vh!, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                                       System.Globalization.DateTimeStyles.None, out var d3))
                FiltroHasta = d3;
            // ignoro fecha puntual
            FiltroFecha = null;
        }

        if (q.TryGetValue("estado", out var ve) && int.TryParse(ve!, out var est))
            FiltroEstado = (EstadoAgenda)est; // si no vino, se mantiene Pendiente

        FiltroBusqueda = q.TryGetValue("q", out var vq) ? vq.ToString() : string.Empty;

        FiltroUsuarioIds.Clear();
        // si hay “usuarios”, dejamos de estar en “Todos”
        if (q.TryGetValue("usuarios", out var vu))
        {
            foreach (var s in vu)
                if (int.TryParse(s, out var id)) FiltroUsuarioIds.Add(id);
            FiltroTodosUsuarios = FiltroUsuarioIds.Count == 0; // si no se parseó ninguno, queda true
        }
        else
        {
            // Si explícitamente vino ?todos=true/false, respetalo
            if (q.TryGetValue("todos", out var vt) && bool.TryParse(vt, out var bt))
                FiltroTodosUsuarios = bt;
            else
                FiltroTodosUsuarios = true; // default
        }
    }




    //protected IEnumerable<AgendaDto> AgendasFiltradas =>
    //    Agendas.Where(a =>
    //    // Estado: si es null, muestra todos; si no, debe coincidir
    //        (FiltroEstado == null || a.EstadoAgenda == FiltroEstado)
    //         &&(            string.IsNullOrWhiteSpace(FiltroBusqueda)
    //        || (a.PersonaNombre?.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase) ?? false)
    //        || (a.Observaciones?.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase) ?? false)
    //    )
    //    );

    protected IEnumerable<AgendaDto> AgendasFiltradas
    {
        get
        {
            var q = Agendas.AsEnumerable();

            // Estado (null = todos)
            if (FiltroEstado != null)
                q = q.Where(a => a.EstadoAgenda == FiltroEstado);

            // Texto (cliente, título, observaciones, tipo)
            var term = FiltroBusqueda?.Trim();
            if (!string.IsNullOrWhiteSpace(term))
            {
                q = q.Where(a =>
                    (a.PersonaNombre?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (a.Titulo?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (a.Observaciones?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (a.TipoAgendamientoNombre?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                );
            }

            // Orden por fecha (y desempate por Id para estabilidad)
            q = (OrdenFecha == "asc")
                ? q.OrderBy(a => a.FechaAgendada).ThenBy(a => a.Id)
                : q.OrderByDescending(a => a.FechaAgendada).ThenByDescending(a => a.Id);

            return q;
        }
    }



    protected void NuevaAgenda() => Nav.NavigateTo("/agendas/editar");

    protected void EditarAgenda(AgendaDto agenda) 
        => Nav.NavigateTo($"/agendas/editar/{agenda.Id}?return={Uri.EscapeDataString(Nav.Uri)}");


    protected void ToggleFiltroUsuario(int id, bool seleccionado)
    {
        if (seleccionado) FiltroUsuarioIds.Add(id);
        else FiltroUsuarioIds.Remove(id);
    }

    //protected async Task BuscarAgendaAsync()
    //{
    //    IEnumerable<int>? usuarios = null;
    //    var incluirParaTodos = true;

    //    if (!FiltroTodosUsuarios)
    //    {
    //        usuarios = FiltroUsuarioIds;
    //        incluirParaTodos = true; // podés exponer un toggle si quisieras excluir “para todos”
    //    }

    //    Agendas = await AgendaService.ObtenerPorFechaAsync(FiltroFecha, usuarios, incluirParaTodos);
    //}

    //protected async Task BuscarAgendaAsync()
    //{
    //    IEnumerable<int>? usuarios = null;
    //    var incluirParaTodos = true;

    //    if (!FiltroTodosUsuarios)
    //    {
    //        usuarios = FiltroUsuarioIds;
    //        incluirParaTodos = true; // podés exponer un toggle si quisieras excluir “para todos”
    //    }

    //    Agendas = await AgendaService.ObtenerPorFechaAsync(FiltroFecha, usuarios, incluirParaTodos);
    //}

    protected async Task BuscarAgendaAsync()
    {
        IEnumerable<int>? usuarios = null;
        var incluirParaTodos = true;

        if (!FiltroTodosUsuarios)
        {
            if (FiltroUsuarioIds.Count == 0) { Agendas = new(); StateHasChanged(); return; }
            usuarios = FiltroUsuarioIds;
            incluirParaTodos = true; // ponelo en false si querés excluir "para todos"
        }

        // Prioridad: si UsarRango => ignora FiltroFecha
        var fecha = UsarRango ? (DateOnly?)null : FiltroFecha;
        var desde = UsarRango ? FiltroDesde : null;
        var hasta = UsarRango ? FiltroHasta : null;

        Agendas = await AgendaService.BuscarAsync(
            fecha: fecha,
            desde: desde,
            hasta: hasta,
            usuarios: usuarios,
            incluirParaTodos: incluirParaTodos,
            estado: FiltroEstado,
            q: string.IsNullOrWhiteSpace(FiltroBusqueda) ? null : FiltroBusqueda
        );

        usuarios = FiltroUsuarioIds;


        // Guardar al final de BuscarAgendaAsync:
        await JS.InvokeVoidAsync("sessionStorage.setItem", "agendas:filters", Nav.Uri);

        UpdateUrlWithFilters();
        StateHasChanged();
    }





    protected Task OnSearchInputAsync()
    {
        if (_debounce is null)
        {
            _debounce = new System.Timers.Timer(350) { AutoReset = false };
            _debounce.Elapsed += async (_, __) => await InvokeAsync(BuscarAgendaAsync);
        }
        _debounce.Stop();
        _debounce.Start();
        return Task.CompletedTask;
    }





    protected void DebouncedBuscar(ChangeEventArgs _)
    {
        _debounce?.Stop();
        _debounce ??= new System.Timers.Timer(350); // ms
        _debounce.AutoReset = false;
        _debounce.Elapsed += async (_, __) =>
        {
            await InvokeAsync(async () => await BuscarAgendaAsync());
        };
        _debounce.Start();
    }

    protected async Task OnToggleTodosAsync()
    {
        if (FiltroTodosUsuarios)
            FiltroUsuarioIds.Clear();

        await BuscarAgendaAsync();
    }


    protected async Task OnToggleUsuarioAsync(int id, bool seleccionado)
    {
        if (seleccionado) FiltroUsuarioIds.Add(id);
        else FiltroUsuarioIds.Remove(id);

        await BuscarAgendaAsync();
    }




    //protected async Task LimpiarFiltros()
    //{
    //    FiltroFecha = DateOnly.FromDateTime(DateTime.Now);
    //    FiltroEstado = null;
    //    FiltroBusqueda = string.Empty;
    //    FiltroTodosUsuarios = true;
    //    FiltroUsuarioIds.Clear();
    //    await BuscarAgendaAsync();
    //}

    protected async Task LimpiarFiltros()
    {
        FiltroEstado = null;
        FiltroBusqueda = string.Empty;

        // fecha/rango
        UsarRango = false;
        FiltroFecha = null;
        FiltroDesde = null;
        FiltroHasta = null;

        // usuarios
        FiltroTodosUsuarios = true;
        FiltroUsuarioIds.Clear();

        await BuscarAgendaAsync();
    }


    protected async Task LimpiarFechaAsync()
    {
        FiltroFecha = null;
        await BuscarAgendaAsync();
    }

    protected async Task LimpiarRangoAsync()
    {
        FiltroDesde = null;
        FiltroHasta = null;
        await BuscarAgendaAsync();
    }

    protected async Task OnToggleRangoAsync()
    {
        if (UsarRango)
        {
            // al pasar a rango, ignoramos fecha puntual
            FiltroFecha = null;
        }
        else
        {
            // al volver a día, limpiamos rango
            FiltroDesde = null;
            FiltroHasta = null;
        }
        await BuscarAgendaAsync();
    }




    protected Task OnOrdenChangedAsync()
    {
        // si ordenás en cliente no hace falta pegarle al backend
        StateHasChanged();
        return Task.CompletedTask;
    }














    protected void UpdateUrlWithFilters()
    {
        var baseUri = Nav.ToAbsoluteUri("/agendas").ToString();

        var qs = new Dictionary<string, string?>();
        // modo fecha vs rango:
        qs["usarRango"] = UsarRango ? "true" : "false";
        qs["sort"] = OrdenFecha;

        if (!UsarRango)
        {
            if (FiltroFecha.HasValue) qs["fecha"] = FiltroFecha.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
        else
        {
            if (FiltroDesde.HasValue) qs["desde"] = FiltroDesde.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (FiltroHasta.HasValue) qs["hasta"] = FiltroHasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        // estado (enum como número)
        if (FiltroEstado.HasValue) qs["estado"] = ((int)FiltroEstado.Value).ToString(CultureInfo.InvariantCulture);

        // texto
        if (!string.IsNullOrWhiteSpace(FiltroBusqueda)) qs["q"] = FiltroBusqueda;

        // usuarios
        if (!FiltroTodosUsuarios && FiltroUsuarioIds.Count > 0)
        {
            // repetí la misma clave para cada usuario: ?usuarios=5&usuarios=7
            // QueryHelpers no soporta repetidas directo, así que armamos manual:
            var url = QueryHelpers.AddQueryString(baseUri, qs!);
            var extra = string.Join("", FiltroUsuarioIds.Select(u => $"&usuarios={u}"));
            var final = url + extra;

            Nav.NavigateTo(final, replace: true); // no recarga, solo actualiza URL
            return;
        }
        else
        {
            qs["todos"] = "true";
        }

        var final2 = QueryHelpers.AddQueryString(baseUri, qs!);
        Nav.NavigateTo(final2, replace: true);
    }









    private async Task Toast(string msg, string tipo) =>
    await JS.InvokeVoidAsync("mostrarToast", msg, tipo);




    protected async Task<int?> GetUsuarioGoogleValidoAsync(int id)
    {
        var uid = id;

        // 1) Verificá que esté vinculado (tenga email Google)
        var estado = await UsuarioService.ObtenerEstadoGoogleAsync(uid);
        var vinculado = !string.IsNullOrWhiteSpace(estado?.Email);
        if (!vinculado)
        {
            await Toast("Ese usuario no tiene cuenta Google vinculada.", "error");
            return null;
        }

        // 2) Intentá validar/refresh token (evita 409)
        var ok = await GoogleService.ValidarYRefrescarTokenAsync(uid);
        if (!ok)
        {
            await Toast("No se pudo validar el acceso a Google. Re-vinculá la cuenta.", "error");
            return null;
        }

        return uid;
    }




    protected async Task EliminarDeGoogle(int id)
    {
        var agenda = await AgendaService.ObtenerPorIdAsync(id);
        if (agenda is null)
        {
            await Toast("No se encontró la agenda.", "error");
            return;
        }

        // Si no hay usuario de Google vinculado, salimos
        if (agenda.GoogleUsuarioId is not int usuarioGoogleId)
        {
            await Toast("No hay usuario de Google vinculado.", "warning");
            return;
        }

        var ok = await JS.InvokeAsync<bool>("confirm", "¿Eliminar el evento en Google Calendar?");
        if (!ok) return;

        // Si este método espera int, ahora le pasamos un int “desenvuelto”
        var userId = await GetUsuarioGoogleValidoAsync(usuarioGoogleId);

        // Si el método devuelve int?, validamos
        if (userId is null)
        {
            await Toast("No se pudo validar la cuenta de Google.", "error");
            return;
        }

        await GoogleService.EliminarEventoAgendaAsync(id, userId.Value);
        //await Toast("🗑 Evento eliminado en Google", "success");

        StateHasChanged();
    }







    //protected async Task EliminarAgenda(AgendaDto agenda)
    //{
    //    var confirmar = await JS.InvokeAsync<bool>("mostrarConfirmacion", $"¿Eliminar evento del {agenda.FechaAgendada:d}?");

    //    if (confirmar)
    //    {
    //        if (!string.IsNullOrWhiteSpace(agenda.GoogleHtmlLink))
    //        {
    //            var confirmarGoogle = await JS.InvokeAsync<bool>("mostrarConfirmacion", $"Además, ¿eliminarlo también de Google Calendar ({agenda.FechaAgendada:d})?");


    //            if (confirmarGoogle) 
    //            {
    //                await EliminarDeGoogle(agenda.Id);
    //                //await JS.InvokeVoidAsync("mostrarToast", "🗑 Evento eliminado de Google", "success");
    //                await AgendaService.EliminarAsync(agenda.Id);
    //                await JS.InvokeVoidAsync("mostrarToast", "🗑 Evento eliminado", "success");

    //            }                              


    //        }

    //        Agendas = await AgendaService.ObtenerTodasAsync();
    //    }
    //}


    protected async Task EliminarAgenda(AgendaDto agenda)
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var confirmar = await JS.InvokeAsync<bool>(
                "mostrarConfirmacion",
                $"¿Eliminar evento del {agenda.FechaAgendada:d}?"
            );
            if (!confirmar) return;

            // Si tiene evento en Google, preguntar y, si acepta, borrar primero en Google
            if (!string.IsNullOrWhiteSpace(agenda.GoogleHtmlLink))
            {
                var confirmarGoogle = await JS.InvokeAsync<bool>(
                    "mostrarConfirmacion",
                    $"Además, ¿eliminarlo también de Google Calendar ({agenda.FechaAgendada:d})?"
                );

                if (confirmarGoogle)
                {
                    await EliminarDeGoogle(agenda.Id);
                    await JS.InvokeVoidAsync("mostrarToast", "🗑 Evento eliminado de Google", "success");
                }
            }

            // Borrado local: SOLO UNA VEZ
            await AgendaService.EliminarAsync(agenda.Id);
            await JS.InvokeVoidAsync("mostrarToast", "🗑 Evento eliminado", "success");

            // Refrescar lista (o quitarlo en memoria) y re-render
            Agendas = await AgendaService.ObtenerTodasAsync();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("mostrarToast", $"Error al eliminar: {ex.Message}", "error");
        }
        finally
        {
            IsBusy = false;
        }
    }





    protected (string Letra, string Texto, string ClaseBg) ObtenerEstadoInfo(EstadoAgenda estado)
    {
        return estado switch
        {
            EstadoAgenda.Pendiente => ("P", "Pendiente", "bg-danger"),
            EstadoAgenda.Realizado => ("R", "Realizado", "bg-success"),
            EstadoAgenda.Cancelado => ("C", "Cancelado", "bg-secondary"),
            EstadoAgenda.Reprogramado => ("Rp", "Reprogramado", "bg-warning"),
            _ => ("?", "Desconocido", "bg-dark")
        };
    }

}
