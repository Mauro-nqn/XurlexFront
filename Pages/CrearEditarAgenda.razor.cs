using IurixBlazor.Pages;
using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;
using IurixBlazor.Shared.Helpers;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using System.Globalization;
using System.Text;



namespace IurixBlazor.Pages;

public class CrearEditarAgendaBase : ComponentBase
{
    [Inject] protected AgendaService AgendaService { get; set; } = default!;
    [Inject] protected TipoAgendamientoService TipoService { get; set; } = default!;
    
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    [Inject] protected UsuarioService UsuarioService { get; set; } = default!;
    [Inject] protected PersonaService PersonaService { get; set; } = default!;
    [Inject] protected GoogleService GoogleService { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected CrearAgendaDto Agenda { get; set; } = new();

    protected ActualizarAgendaDto AgendaActualizar { get; set; } = new();
    protected List<TipoAgendamientoDto> Tipos { get; set; } = new();
    
    protected List<UsuarioDto> Usuarios { get; set; } = new();
    protected List<PersonaDto> Personas { get; set; } = new();

    

    protected bool SincronizarConGoogle { get; set; }
    protected int? UsuarioGoogleId { get; set; }
    protected CrearEventoDto EventoGoogle { get; set; } = new();
    protected bool? GoogleVinculado { get; set; }
    protected string? GoogleEmail { get; set; }


    protected AgendaDto? _original; // clon para comparar

    protected bool EsEdicion => Id.HasValue;


    [SupplyParameterFromQuery(Name = "return")]
    public string? ReturnUrl { get; set; }



    //Para deshabilitar boton guardar si no tiene fecha valida
    protected bool EsAgendaValida =>
           !string.IsNullOrWhiteSpace(Agenda.Titulo)
           && !EsFechaInvalida(Agenda.FechaAgendada)
           && (!Agenda.FechaFin.HasValue || (Agenda.FechaFin.Value >= Agenda.FechaAgendada && !EsFechaInvalida(Agenda.FechaFin.Value)));



    // Duración (minutos). Default 60.
    protected int AgendaDuracionMin { get; set; } = 60;

    // Opciones (15’ de mínimo)
    protected int[] DuracionesMin = new[] { 15, 30, 45, 60, 90, 120 };
    protected (int Min, string Label)[] DuracionesLabeled = Array.Empty<(int, string)>();

    private static string FormatMinutes(int m)
        => m < 60 ? $"{m} min" : (m % 60 == 0 ? $"{m / 60} h" : $"{m / 60} h {m % 60} min");




    protected bool AgendaNotificarApp { get; set; }
    protected int? AgendaNotificarAppUsuarioId { get; set; }
    protected bool AgendaNotificarEmail { get; set; }
    protected int? AgendaNotificarEmailUsuarioId { get; set; }
    protected string? AgendaNotificarEmailTo { get; set; }
    protected int? AgendaRecordatorioMinAntes { get; set; }


    protected (int Min, string Label)[] RecordatorioOpciones =
{
            (5,  "5 min"),
            (10, "10 min"),
            (30, "30 min"),
            (60, FormatMinutes(60)),
            (120, FormatMinutes(120)),
            (1440, "1 día"),
            (2880, "2 días")
        };



    protected DotNetObjectReference<CrearEditarAgendaBase>? _voiceRef;
    protected bool _dictadoSoportado;
    protected bool _dictando;
    protected string? _interim;

    protected string _ultimoChunk = ""; //  anti-duplicado Samsung

    protected string _baseAntesDeDictar = "";
    protected string _ultimoFullFinal = "";





    protected override async Task OnInitializedAsync()
    {
        Tipos = await TipoService.ObtenerTodosAsync();
        Usuarios = await UsuarioService.ObtenerUsuariosAsync() ?? new List<UsuarioDto>();
        
        Personas = await PersonaService.ObtenerPersonasAsync();
        Agenda.UsuarioIds ??= new List<int>();

        DuracionesLabeled = DuracionesMin.Select(m => (m, FormatMinutes(m))).ToArray();



        if (EsEdicion)
        {
            var existente = await AgendaService.ObtenerPorIdAsync(Id!.Value);
            if (existente is null)
            {
                await Toast("No se encontró el evento.", "error");
                //Nav.NavigateTo("/agendas");
                Nav.NavigateTo(string.IsNullOrWhiteSpace(ReturnUrl) ? "/agendas" : ReturnUrl);
                return;
            }

            // Mapear a modelo de edición (normalizado a Local)
            Agenda = new CrearAgendaDto
            {
                Titulo = existente.Titulo,
                //FechaAgendada = DateTime.SpecifyKind(existente.FechaAgendada, DateTimeKind.Local),
                //FechaFin = existente.FechaFin.HasValue
                //    ? DateTime.SpecifyKind(existente.FechaFin.Value, DateTimeKind.Local)
                //    : (DateTime?)null,
                FechaAgendada = HoraHelper.FromUtcToArUnspecified(existente.FechaAgendada),
                FechaFin = existente.FechaFin.HasValue ? HoraHelper.FromUtcToArUnspecified(existente.FechaFin.Value) : (DateTime?)null,
                TipoAgendamientoId = existente.TipoAgendamientoId,
                EstadoAgenda = existente.EstadoAgenda,
                Observaciones = existente.Observaciones,
                PersonaId = existente.PersonaId,
                UsuarioId = existente.UsuarioId,
                GoogleRegistrado = existente.GoogleRegistrado,
                GoogleHtmlLink = existente.GoogleHtmlLink,
                GoogleLastSyncUtc = existente.GoogleLastSyncUtc


            };

            Agenda.EsParaTodos = existente.EsParaTodos;
            Agenda.UsuarioIds = existente.UsuarioIds?.ToList() ?? new List<int>();

            Agenda.GoogleUsuarioId = existente.GoogleUsuarioId;
            UsuarioGoogleId = existente.GoogleUsuarioId;     // para reflejarlo en el selector
            GoogleEmail = existente.GoogleEmailSnapshot; // si lo devolvés
            GoogleVinculado = !string.IsNullOrWhiteSpace(GoogleEmail);


            // Duración inicial
            if (Agenda.FechaFin.HasValue)
            {
                var diff = Agenda.FechaFin.Value - Agenda.FechaAgendada;
                var mins = Math.Max(15, (int)Math.Round(diff.TotalMinutes));

                // Ajustá a la grilla de opciones (si querés “snap”)
                AgendaDuracionMin = DuracionesMin.Contains(mins)
                    ? mins
                    : DuracionesMin.OrderBy(x => Math.Abs(x - mins)).First(); // el más cercano
            }
            else
            {
                AgendaDuracionMin = 60;
                RecalcularFin(); // para dejar fin consistente
            }


            // Guardar baseline para comparación de cambios
            _original = new AgendaDto
            {

                Id = existente.Id,
                Titulo = Agenda.Titulo,
                FechaAgendada = Agenda.FechaAgendada,     // <- AR/Unspecified
                FechaFin = Agenda.FechaFin,               // <- AR/Unspecified
                TipoAgendamientoId = Agenda.TipoAgendamientoId,
                EstadoAgenda = Agenda.EstadoAgenda,
                Observaciones = Agenda.Observaciones,
                PersonaId = Agenda.PersonaId,
                UsuarioId = Agenda.UsuarioId,
                GoogleRegistrado = existente.GoogleRegistrado,
                GoogleHtmlLink = existente.GoogleHtmlLink,
                GoogleLastSyncUtc = existente.GoogleLastSyncUtc,
                GoogleEventId = existente.GoogleEventId,
                GoogleCalendarId = existente.GoogleCalendarId,
                GoogleUsuarioId = existente.GoogleUsuarioId,
            };
        }
        else
        {
            // Defaults para crear

            //Agenda.FechaAgendada = HoraHelper.TomorrowAtHourArUnspecified(12); // mañana 12:00 AR
            //Agenda.FechaFin = Agenda.FechaAgendada.AddHours(1);

            Agenda.FechaAgendada = HoraHelper.TomorrowAtHourArUnspecified(12);
            AgendaDuracionMin = 60;
            RecalcularFin();

            var evento = Tipos.FirstOrDefault(t => t.Nombre.Equals("Evento", StringComparison.OrdinalIgnoreCase));
            if (evento != null) Agenda.TipoAgendamientoId = evento.Id;
        }
    }



    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _voiceRef ??= DotNetObjectReference.Create(this);

        try
        {
            _dictadoSoportado = await JS.InvokeAsync<bool>("voiceDictation.supported");
        }
        catch
        {
            _dictadoSoportado = false;
        }

        StateHasChanged();
    }


    protected async Task ToggleDictado()
    {
        if (!_dictadoSoportado)
        {
            await JS.InvokeVoidAsync("mostrarToast", "Este navegador no soporta dictado.", "warning");
            return;
        }

        _voiceRef ??= DotNetObjectReference.Create(this);

        //if (!_dictando)
        //{
        //    _ultimoChunk = "";
        //    _interim = null;

        //    var ok = await JS.InvokeAsync<bool>("voiceDictation.start", "obsAgenda", _voiceRef, "es-AR");
        //    if (!ok)
        //        await JS.InvokeVoidAsync("mostrarToast", "No se pudo iniciar el dictado.", "error");
        //}

        if (!_dictando)
        {
            _baseAntesDeDictar = Agenda.Observaciones ?? "";
            _ultimoFullFinal = "";
            _interim = null;

            var ok = await JS.InvokeAsync<bool>("voiceDictation.start", "obsAgenda", _voiceRef, "es-AR");
            if (!ok)
                await JS.InvokeVoidAsync("mostrarToast", "No se pudo iniciar el dictado.", "error");
        }
        else
        {
            await JS.InvokeVoidAsync("voiceDictation.stop");
            _interim = null;
        }

    }


    [JSInvokable]
    public Task OnDictationChunk(string fullFinal, string interim)
    {
        _interim = string.IsNullOrWhiteSpace(interim) ? null : interim;

        string norm(string s) =>
            System.Text.RegularExpressions.Regex
                .Replace((s ?? "").Replace("\u00A0", " "), @"\s+", " ")
                .Trim();

        var ff = norm(fullFinal);

        // si no hay final, no tocamos Observaciones (solo mostramos interim)
        if (string.IsNullOrWhiteSpace(ff))
            return InvokeAsync(StateHasChanged);

        // guardamos el último final (por si querés usarlo después)
        _ultimoFullFinal = ff;

        // ✅ REEMPLAZO: base + fullFinal (sin duplicar nunca)
        var baseTxt = norm(_baseAntesDeDictar);
        if (!string.IsNullOrWhiteSpace(baseTxt))
            Agenda.Observaciones = baseTxt + " " + ff;
        else
            Agenda.Observaciones = ff;

        return InvokeAsync(StateHasChanged);
    }







    [JSInvokable]
    public Task OnDictationState(bool started)
    {
        _dictando = started;
        if (!started) _interim = null;
        return InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public Task OnDictationError(string error)
    {
        _dictando = false;
        _interim = null;

        // No spamear toasts por abort/no-speech
        if (string.Equals(error, "aborted", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(error, "no-speech", StringComparison.OrdinalIgnoreCase))
            return InvokeAsync(StateHasChanged);

        _ = JS.InvokeVoidAsync("mostrarToast", $"Dictado: {error}", "warning");
        return InvokeAsync(StateHasChanged);
    }



    protected async Task LimpiarObservaciones()
    {
        if (_dictando)
            await JS.InvokeVoidAsync("voiceDictation.stop");

        Agenda.Observaciones = null;
        _ultimoChunk = "";
        _interim = null;

        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _voiceRef?.Dispose();
    }


    private static bool EndsWithChunk(string? fullText, string chunk)
    {
        if (string.IsNullOrWhiteSpace(fullText)) return false;

        string norm(string s)
            => System.Text.RegularExpressions.Regex
                .Replace((s ?? "").Replace("\u00A0", " "), @"\s+", " ")
                .Trim()
                .ToLowerInvariant();

        var ft = norm(fullText);
        var ch = norm(chunk);

        if (string.IsNullOrWhiteSpace(ch)) return true;

        return ft.EndsWith(ch);
    }





    //Calcular Fecha Fin

    private void RecalcularFin()
    {
        if (Agenda?.FechaAgendada == default) return;

        var inicio = Agenda.FechaAgendada; // AR / Unspecified
        var dur = Math.Max(15, AgendaDuracionMin); // mínimo 15
        var fin = inicio.AddMinutes(dur);

        Agenda.FechaFin = DateTime.SpecifyKind(fin, DateTimeKind.Unspecified);
    }

    // Cuando cambia la duración
    protected Task OnDuracionChangedAsync()
    {
        RecalcularFin();
        StateHasChanged();
        return Task.CompletedTask;
    }

    // Cuando cambia la fecha/hora de inicio
    protected Task OnInicioChangedAsync()
    {
        RecalcularFin();
        StateHasChanged();
        return Task.CompletedTask;
    }






    //protected async Task ConsultarEstadoGoogle(ChangeEventArgs e)
    //{
    //    if (int.TryParse(e.Value?.ToString(), out var id))
    //    {
    //        var estado = await UsuarioService.ObtenerEstadoGoogleAsync(id);
    //        GoogleVinculado = estado != null && !string.IsNullOrWhiteSpace(estado.Email);
    //    }
    //    else
    //    {
    //        GoogleVinculado = null;
    //    }
    //}


    //protected async Task OnUsuarioGoogleChanged(int? id)
    //{
    //    UsuarioGoogleId = id;

    //    if (id is null)
    //    {
    //        GoogleVinculado = null;
    //        GoogleEmail = null;
    //        StateHasChanged();
    //        return;
    //    }

    //    var estado = await UsuarioService.ObtenerEstadoGoogleAsync(id.Value);
    //    GoogleEmail = estado?.Email;
    //    GoogleVinculado = !string.IsNullOrWhiteSpace(GoogleEmail);
    //    StateHasChanged();
    //}

    //protected async Task OnUsuarioGoogleChanged(int? id)
    //{
    //    UsuarioGoogleId = id;
    //    if (id is null) { GoogleVinculado = null; GoogleEmail = null; StateHasChanged(); return; }

    //    var estado = await UsuarioService.ObtenerEstadoGoogleAsync(id.Value);
    //    GoogleEmail = estado?.Email;
    //    GoogleVinculado = !string.IsNullOrWhiteSpace(GoogleEmail);
    //    StateHasChanged();
    //}

    protected async Task OnUsuarioGoogleChanged(int? id)
    {
        UsuarioGoogleId = id;

        if (id is null)
        {
            GoogleVinculado = null;
            GoogleEmail = null;
            StateHasChanged();
            return;
        }

        var estado = await UsuarioService.ObtenerEstadoGoogleAsync(id.Value);
        GoogleEmail = estado?.Email;

        //  Mejor criterio: refresh token presente => cuenta vinculada
        GoogleVinculado = !string.IsNullOrWhiteSpace(estado?.RefreshToken);

        // (opcional) si querés mostrar “expira en …”, podés leer estado.Expira
        StateHasChanged();
    }



    //protected int? GetUsuarioGoogleParaAccion()
    //=> Agenda.GoogleUsuarioId ?? UsuarioGoogleId;


    // 👈 Prioriza lo que el usuario eligió ahora
    private int? GetUsuarioGoogleParaAccion()
        => UsuarioGoogleId ?? Agenda.GoogleUsuarioId;

    protected async Task<int?> GetUsuarioGoogleValidoAsync()
    {
        var uid = GetUsuarioGoogleParaAccion(); // Agenda.GoogleUsuarioId ?? UsuarioGoogleId
        if (uid is null)
        {
            await Toast("Seleccioná un Usuario Google.", "warning");
            return null;
        }

        // 1) Verificá que esté vinculado (tenga email Google)
        var estado = await UsuarioService.ObtenerEstadoGoogleAsync(uid.Value);
        var vinculado = !string.IsNullOrWhiteSpace(estado?.Email);
        if (!vinculado)
        {
            await Toast("Ese usuario no tiene cuenta Google vinculada.", "error");
            return null;
        }

        // 2) Intentá validar/refresh token (evita 409)
        var ok = await GoogleService.ValidarYRefrescarTokenAsync(uid.Value);
        if (!ok)
        {
            await Toast("No se pudo validar el acceso a Google. Re-vinculá la cuenta.", "error");
            return null;
        }

        return uid;
    }




    protected void ToggleUsuarioEvento(int id, ChangeEventArgs e)
    {
        // e.Value puede venir como bool o como "on"/"true"
        var seleccionado = e.Value is bool b
            ? b
            : string.Equals(e.Value?.ToString(), "true", StringComparison.OrdinalIgnoreCase) ||
              string.Equals(e.Value?.ToString(), "on", StringComparison.OrdinalIgnoreCase);

        Agenda.UsuarioIds ??= new List<int>();
        if (seleccionado)
        {
            if (!Agenda.UsuarioIds.Contains(id)) Agenda.UsuarioIds.Add(id);
        }
        else
        {
            Agenda.UsuarioIds.Remove(id);
        }
    }






    protected bool ValidarParaGoogle(out string msg)
    {
        if (string.IsNullOrWhiteSpace(Agenda.Titulo))
        { msg = "Ingresá un título para el evento."; return false; }

        if (!Agenda.FechaFin.HasValue)
        { msg = "Definí la fecha/hora de fin."; return false; }

        if (Agenda.FechaFin.Value <= Agenda.FechaAgendada)
        { msg = "La fecha/hora de fin debe ser posterior al inicio."; return false; }

        // (opcional) si tu backend rechaza pasado:
        // if (Agenda.FechaAgendada <= DateTime.Now) { msg = "La fecha de inicio no puede ser en el pasado."; return false; }

        msg = string.Empty;
        return true;
    }

















    protected bool CambiosQueAfectanGoogle()
    {
        if (_original is null || Agenda is null) return false;

        bool distinto(string? a, string? b) => (a ?? string.Empty) != (b ?? string.Empty);

        return
            distinto(Agenda.Titulo, _original.Titulo) ||
            Agenda.FechaAgendada != _original.FechaAgendada ||
            Agenda.FechaFin != _original.FechaFin ||
            distinto(Agenda.Observaciones, _original.Observaciones);
    }




    //protected async Task Guardar()
    //{



    //    //if (Id.HasValue)
    //    //{

    //    //    // Mapear lo que el usuario editó en el form
    //    //    var dto = new ActualizarAgendaDto
    //    //    {
    //    //        FechaAgendada = Agenda.FechaAgendada,
    //    //        TipoAgendamientoId = Agenda.TipoAgendamientoId,
    //    //        EstadoAgenda = Agenda.EstadoAgenda,
    //    //        Observaciones = string.IsNullOrWhiteSpace(Agenda.Observaciones) ? null : Agenda.Observaciones,
    //    //        PersonaId = Agenda.PersonaId,
    //    //        UsuarioId = Agenda.UsuarioId,
    //    //        // MovimientoId = ... (si aplica y tenés ese dato en la vista)
    //    //    };

    //    //    await AgendaService.ActualizarAsync(Id.Value, dto);
    //    //    //await AgendaService.ActualizarAsync(Id.Value, AgendaActualizar);
    //    //}
    //    //else
    //    //{
    //    //    await AgendaService.CrearAsync(Agenda);

    //    //    if (SincronizarConGoogle && UsuarioGoogleId.HasValue && GoogleVinculado == true)
    //    //    {
    //    //        await GoogleService.CrearEventoAsync(UsuarioGoogleId.Value, EventoGoogle);
    //    //        await JS.InvokeVoidAsync("mostrarToast", "📤 Evento enviado a Google Calendar", "info");
    //    //    }
    //    //}

    //    if (Id.HasValue)
    //    {
    //        var dto = new ActualizarAgendaDto
    //        {
    //            Titulo = Agenda.Titulo,
    //            FechaAgendada = Agenda.FechaAgendada,
    //            FechaFin = Agenda.FechaFin,
    //            TipoAgendamientoId = Agenda.TipoAgendamientoId,
    //            EstadoAgenda = Agenda.EstadoAgenda,
    //            Observaciones = string.IsNullOrWhiteSpace(Agenda.Observaciones) ? null : Agenda.Observaciones,
    //            PersonaId = Agenda.PersonaId,
    //            UsuarioId = Agenda.UsuarioId
    //        };

    //        await AgendaService.ActualizarAsync(Id.Value, dto);

    //        if (SincronizarConGoogle && UsuarioGoogleId.HasValue && GoogleVinculado == true)
    //        {
    //            var sync = await GoogleService.SincronizarAgendaAsync(Id.Value, UsuarioGoogleId.Value);
    //            Agenda.GoogleRegistrado = sync.GoogleRegistrado;
    //            Agenda.GoogleHtmlLink = sync.GoogleHtmlLink;
    //            Agenda.GoogleLastSyncUtc = sync.GoogleLastSyncUtc;
    //            await JS.InvokeVoidAsync("mostrarToast", "♻️ Evento re-sincronizado en Google", "info");
    //        }
    //    }
    //    else
    //    {
    //        // Crear agenda
    //        var creada = await AgendaService.CrearAsync(Agenda); // devolver con Id
    //        if (SincronizarConGoogle && UsuarioGoogleId.HasValue && GoogleVinculado == true && creada is not null)
    //        {
    //            var sync = await GoogleService.SincronizarAgendaAsync(creada.Id, UsuarioGoogleId.Value);
    //            Agenda.GoogleRegistrado = sync.GoogleRegistrado;
    //            Agenda.GoogleHtmlLink = sync.GoogleHtmlLink;
    //            Agenda.GoogleLastSyncUtc = sync.GoogleLastSyncUtc;
    //            await JS.InvokeVoidAsync("mostrarToast", "📤 Evento creado en Google", "success");
    //        }
    //    }

    //    await JS.InvokeVoidAsync("mostrarToast", "✅ Evento guardado", "success");
    //    Nav.NavigateTo("/agendas");
    //}





    //protected async Task EliminarDeGoogle()
    //{
    //    if (!Id.HasValue || !UsuarioGoogleId.HasValue) return;
    //    var ok = await JS.InvokeAsync<bool>("confirm", "¿Eliminar el evento en Google Calendar?");
    //    if (!ok) return;
    //    await GoogleService.EliminarEventoAgendaAsync(Id.Value, UsuarioGoogleId.Value);
    //    await JS.InvokeVoidAsync("mostrarToast", "🗑 Evento eliminado en Google", "success");
    //    // opcional: refrescar detalle
    //}

    protected async Task Guardar() => await GuardarAsync(false);

    //protected async Task GuardarAsync(bool resincronizarForzado)
    //{
    //    if (Id.HasValue) // EDITAR
    //    {
    //        var dto = new ActualizarAgendaDto
    //        {
    //            Titulo = Agenda.Titulo,
    //            //FechaAgendada = Agenda.FechaAgendada,
    //            //FechaFin = Agenda.FechaFin,
    //            FechaAgendada = DateTime.SpecifyKind(Agenda.FechaAgendada, DateTimeKind.Unspecified),
    //            FechaFin = Agenda.FechaFin.HasValue ? DateTime.SpecifyKind(Agenda.FechaFin.Value, DateTimeKind.Unspecified) : (DateTime?)null,
    //            TipoAgendamientoId = Agenda.TipoAgendamientoId,
    //            EstadoAgenda = Agenda.EstadoAgenda,
    //            Observaciones = string.IsNullOrWhiteSpace(Agenda.Observaciones) ? null : Agenda.Observaciones,
    //            PersonaId = Agenda.PersonaId,
    //            UsuarioId = Agenda.UsuarioId
    //        };

    //        await AgendaService.ActualizarAsync(Id.Value, dto);

    //        // Si está sincronizado, ofrecer/forzar resincronizar
    //        var yaSincronizado = Agenda.GoogleRegistrado == true || _original?.GoogleRegistrado == true;

    //        if (yaSincronizado)
    //        {
    //            var debeResync = resincronizarForzado;

    //            if (!debeResync && CambiosQueAfectanGoogle())
    //            {
    //                debeResync = await JS.InvokeAsync<bool>("confirm",
    //                    "Este evento ya está sincronizado con Google. ¿Querés actualizarlo también en Calendar?");
    //            }

    //            if (debeResync)
    //            {
    //                var usuarioId = UsuarioGoogleId ?? Agenda.UsuarioId;
    //                if (usuarioId is null)
    //                {
    //                    await JS.InvokeVoidAsync("mostrarToast",
    //                        "Seleccioná un Usuario Google para actualizar en Calendar.", "warning");
    //                }
    //                else
    //                {
    //                    var sync = await GoogleService.SincronizarAgendaAsync(Id.Value, usuarioId.Value);
    //                    Agenda.GoogleRegistrado = sync.GoogleRegistrado;
    //                    Agenda.GoogleHtmlLink = sync.GoogleHtmlLink;
    //                    Agenda.GoogleLastSyncUtc = sync.GoogleLastSyncUtc;

    //                    UpdateBaselineFromCurrentForm();
    //                    await JS.InvokeVoidAsync("mostrarToast", "♻️ Evento actualizado en Google", "success");
    //                }
    //            }
    //        }
    //        else
    //        {
    //            // No sincronizado aún: respetar checkbox
    //            if (SincronizarConGoogle && UsuarioGoogleId.HasValue && GoogleVinculado == true)
    //            {
    //                var sync = await GoogleService.SincronizarAgendaAsync(Id.Value, UsuarioGoogleId.Value);
    //                Agenda.GoogleRegistrado = sync.GoogleRegistrado;
    //                Agenda.GoogleHtmlLink = sync.GoogleHtmlLink;
    //                Agenda.GoogleLastSyncUtc = sync.GoogleLastSyncUtc;

    //                UpdateBaselineFromCurrentForm();
    //                await JS.InvokeVoidAsync("mostrarToast", "📤 Evento creado en Google", "success");
    //            }
    //        }
    //    }
    //    else // CREAR
    //    {
    //        var creada = await AgendaService.CrearAsync(Agenda); // debe devolver Id
    //        if (SincronizarConGoogle && UsuarioGoogleId.HasValue && GoogleVinculado == true && creada is not null)
    //        {
    //            var sync = await GoogleService.SincronizarAgendaAsync(creada.Id, UsuarioGoogleId.Value);
    //            Agenda.GoogleRegistrado = sync.GoogleRegistrado;
    //            Agenda.GoogleHtmlLink = sync.GoogleHtmlLink;
    //            Agenda.GoogleLastSyncUtc = sync.GoogleLastSyncUtc;
    //            UpdateBaselineFromCurrentForm();
    //            await JS.InvokeVoidAsync("mostrarToast", "📤 Evento creado en Google", "success");
    //        }
    //    }

    //    await JS.InvokeVoidAsync("mostrarToast", "✅ Evento guardado", "success");
    //    Nav.NavigateTo("/agendas");
    //}











    private static bool EsFechaInvalida(DateTime dt)
    {
        // Evita default (01/01/0001) u otros valores raros
        return dt == default || dt.Year < 1900 || dt.Year > 9999;
    }

    private bool ValidarCamposAgenda(out string mensaje)
    {
        mensaje = string.Empty;

        // Título (opcional, si lo requerís)
        if (string.IsNullOrWhiteSpace(Agenda.Titulo))
        {
            mensaje = "Ingresá un título para el evento.";
            return false;
        }

        if (Agenda.FechaAgendada == default)
        {
            mensaje="La fecha de inicio es obligatoria.";
            return false;
        }

        if (Agenda.FechaFin == default)
        {
            mensaje = "La fecha de fin es obligatoria.";
            return false;
        }

        // Fecha de inicio obligatoria
        if (EsFechaInvalida(Agenda.FechaAgendada))
        {
            mensaje = "La fecha/hora de inicio es inválida o está vacía.";
            return false;
        }

        // Fin opcional pero, si viene, debe ser válida y >= inicio
        //if (Agenda.FechaFin.HasValue)
        //{
        //    var fin = Agenda.FechaFin.Value;
        //    if (EsFechaInvalida(fin))
        //    {
        //        mensaje = "La fecha/hora de fin es inválida.";
        //        return false;
        //    }
        //    if (fin < Agenda.FechaAgendada)
        //    {
        //        mensaje = "La fecha/hora de fin no puede ser anterior al inicio.";
        //        return false;
        //    }
        //}

        // Asegurá que el fin esté calculado
        if (!Agenda.FechaFin.HasValue) RecalcularFin();

        var fin = Agenda.FechaFin!.Value;

        if (EsFechaInvalida(fin))
        {
            mensaje = "La fecha/hora de fin es inválida.";
            return false;
        }

        if (fin < Agenda.FechaAgendada)
        {
            mensaje = "La fecha/hora de fin no puede ser anterior al inicio.";
            return false;
        }

        return true;
    }













    protected async Task MostrarToast(string mensaje, string tipo = "success")
    {
        await JS.InvokeVoidAsync("mostrarToast", mensaje, tipo);
    }



    protected async Task GuardarAsync(bool resincronizarForzado)
    {
        if (!ValidarCamposAgenda(out var error))
        {
            await JS.InvokeVoidAsync("mostrarToast", error, "warning");
            return;
        }

        //  Validación: si no es para todos, tiene que haber al menos 1 usuario
        if (!Agenda.EsParaTodos && (Agenda.UsuarioIds == null || Agenda.UsuarioIds.Count == 0))
        {
            await JS.InvokeVoidAsync("mostrarToast", "Seleccioná al menos un usuario o marcá ‘Todos’.", "warning");
            return;
        }

        if (Id.HasValue) // EDITAR
        {
            var dto = new ActualizarAgendaDto
            {
                Titulo = Agenda.Titulo,
                FechaAgendada = DateTime.SpecifyKind(Agenda.FechaAgendada, DateTimeKind.Unspecified),
                FechaFin = Agenda.FechaFin.HasValue
                    ? DateTime.SpecifyKind(Agenda.FechaFin.Value, DateTimeKind.Unspecified)
                    : (DateTime?)null,
                TipoAgendamientoId = Agenda.TipoAgendamientoId,
                EstadoAgenda = Agenda.EstadoAgenda,
                Observaciones = string.IsNullOrWhiteSpace(Agenda.Observaciones) ? null : Agenda.Observaciones,
                PersonaId = Agenda.PersonaId,
                UsuarioId = Agenda.UsuarioId,
                GoogleUsuarioId = Agenda.GoogleUsuarioId,
                GoogleEmailSnapshot = Agenda.GoogleEmailSnapshot,

                EsParaTodos = Agenda.EsParaTodos,
                UsuarioIds = Agenda.EsParaTodos
                ? null // si es para todos, no enviamos la lista
                : Agenda.UsuarioIds?.Distinct().ToList()
            };

            await AgendaService.ActualizarAsync(Id.Value, dto);

            // 🔴 Si quedó CANCELADO y estaba sincronizado → ofrecer eliminar en Google
            if (Agenda.EstadoAgenda == EstadoAgenda.Cancelado && Agenda.GoogleRegistrado == true)
            {
                //var usuarioId = UsuarioGoogleId ?? Agenda.UsuarioId;
                var usuarioId = GetUsuarioGoogleParaAccion();

                if (usuarioId is null)
                {
                    await Toast("Seleccioná un Usuario Google para eliminar en Calendar.", "warning");
                }
                else
                {
                    var desea = await JS.InvokeAsync<bool>("confirm",
                        "Este evento se marcó como Cancelado. ¿Querés eliminarlo también de Google Calendar?");
                    if (desea)
                    {
                        await EliminarDeGoogle(); // implementado más abajo
                        await JS.InvokeVoidAsync("mostrarToast", "🗑️ Evento eliminado en Google", "success");
                    }
                }
            }
            else
            {
                // ♻️ Lógica de resincronización habitual
                var yaSincronizado = Agenda.GoogleRegistrado == true || _original?.GoogleRegistrado == true;
                if (yaSincronizado)
                {
                    var debeResync = resincronizarForzado;

                    if (!debeResync && CambiosQueAfectanGoogle())
                    {
                        debeResync = await JS.InvokeAsync<bool>("confirm",
                            "Este evento ya está sincronizado con Google. ¿Querés actualizarlo también en Calendar?");
                    }

                    if (debeResync)
                    {

                        //var usuarioId = UsuarioGoogleId ?? Agenda.UsuarioId;
                        //var usuarioId = GetUsuarioGoogleParaAccion();
                        if (!ValidarParaGoogle(out var m)) { await Toast(m, "warning"); return; }

                        var userId = await GetUsuarioGoogleValidoAsync();
                        if (userId is null) return;
                        //if (usuarioId is null)
                        //{
                        //    await JS.InvokeVoidAsync("mostrarToast",
                        //        "Seleccioná un Usuario Google para actualizar en Calendar.", "warning");
                        //}
                        //else
                        //{
                        //    var sync = await GoogleService.SincronizarAgendaAsync(Id.Value, usuarioId.Value);
                        //    Agenda.GoogleRegistrado = sync.GoogleRegistrado;
                        //    Agenda.GoogleHtmlLink = sync.GoogleHtmlLink;
                        //    Agenda.GoogleLastSyncUtc = sync.GoogleLastSyncUtc;

                        //    UpdateBaselineFromCurrentForm();
                        //    await JS.InvokeVoidAsync("mostrarToast", "♻️ Evento actualizado en Google", "success");
                        //}
                        try
                        {

                            var sync = await GoogleService.SincronizarAgendaAsync(Id.Value, userId.Value);
                            Agenda.GoogleRegistrado = sync.GoogleRegistrado;
                            Agenda.GoogleHtmlLink = sync.GoogleHtmlLink;
                            Agenda.GoogleLastSyncUtc = sync.GoogleLastSyncUtc;
                            Agenda.GoogleUsuarioId = sync.GoogleUsuarioId;       // 👈 persistido en server
                            GoogleEmail = sync.GoogleEmailSnapshot;    // opcional
                            UpdateBaselineFromCurrentForm();
                            await JS.InvokeVoidAsync("mostrarToast", "♻️ Evento actualizado en Google", "success");
                        }
                        catch (InvalidOperationException ex) when (ex.Message.StartsWith("GoogleAuthConflict"))
                        {
                            await Toast("No se pudo sincronizar con Google: " + ex.Message, "error");
                            return;
                        }
                    }
                }
                else
                {
                    // Creación en Google si corresponde
                    if (SincronizarConGoogle && UsuarioGoogleId.HasValue && GoogleVinculado == true)
                    {
                        //var sync = await GoogleService.SincronizarAgendaAsync(Id.Value, UsuarioGoogleId.Value);
                        //Agenda.GoogleRegistrado = sync.GoogleRegistrado;
                        //Agenda.GoogleHtmlLink = sync.GoogleHtmlLink;
                        //Agenda.GoogleLastSyncUtc = sync.GoogleLastSyncUtc;

                        //UpdateBaselineFromCurrentForm();
                        //await JS.InvokeVoidAsync("mostrarToast", "📤 Evento creado en Google", "success");


                        if (!ValidarParaGoogle(out var m)) { await Toast(m, "warning"); return; }
                        


                        var userId = await GetUsuarioGoogleValidoAsync();
                        if (userId is null) return;

                        try
                        {
                            var sync = await GoogleService.SincronizarAgendaAsync(Id.Value, userId.Value);
                            Agenda.GoogleRegistrado = sync.GoogleRegistrado;
                            Agenda.GoogleHtmlLink = sync.GoogleHtmlLink;
                            Agenda.GoogleLastSyncUtc = sync.GoogleLastSyncUtc;
                            Agenda.GoogleUsuarioId = sync.GoogleUsuarioId;    // 👈
                            GoogleEmail = sync.GoogleEmailSnapshot;
                            UpdateBaselineFromCurrentForm();
                            await JS.InvokeVoidAsync("mostrarToast", "📤 Evento creado en Google", "success");
                        }
                        catch (InvalidOperationException ex) when (ex.Message.StartsWith("GoogleAuthConflict"))
                        {
                            await Toast("No se pudo sincronizar con Google: " + ex.Message, "error");
                            return;
                        }
                    }
                }
            }
        }
        else // CREAR
        {
            try { 
            var creada = await AgendaService.CrearAsync(Agenda);
            if (SincronizarConGoogle && UsuarioGoogleId.HasValue && GoogleVinculado == true && creada is not null)
            {
                //var sync = await GoogleService.SincronizarAgendaAsync(creada.Id, UsuarioGoogleId.Value);
                //Agenda.GoogleRegistrado = sync.GoogleRegistrado;
                //Agenda.GoogleHtmlLink = sync.GoogleHtmlLink;
                //Agenda.GoogleLastSyncUtc = sync.GoogleLastSyncUtc;
                //UpdateBaselineFromCurrentForm();
                //await JS.InvokeVoidAsync("mostrarToast", "📤 Evento creado en Google", "success");
                if (!ValidarParaGoogle(out var m)) { await Toast(m, "warning"); return; }


                var userId = await GetUsuarioGoogleValidoAsync();
                if (userId is null) return;

                try
                {
                    var sync = await GoogleService.SincronizarAgendaAsync(creada.Id, userId.Value);
                    Agenda.GoogleRegistrado = sync.GoogleRegistrado;
                    Agenda.GoogleHtmlLink = sync.GoogleHtmlLink;
                    Agenda.GoogleLastSyncUtc = sync.GoogleLastSyncUtc;
                    Agenda.GoogleUsuarioId = sync.GoogleUsuarioId;    // 👈
                    GoogleEmail = sync.GoogleEmailSnapshot;
                    UpdateBaselineFromCurrentForm();
                    await JS.InvokeVoidAsync("mostrarToast", "📤 Evento creado en Google", "success");
                }
                catch (InvalidOperationException ex) when (ex.Message.StartsWith("GoogleAuthConflict"))
                {
                    await Toast("No se pudo sincronizar con Google: " + ex.Message, "error");
                    return;
                }
            }
            }
            catch (InvalidOperationException ex)
            {
                await JS.InvokeVoidAsync("mostrarToast", ex.Message, "warning");
                return;
            }
            catch (HttpRequestException)
            {
                await JS.InvokeVoidAsync("mostrarToast", "No se pudo conectar con el servidor.", "error");
                return;
            }
        }



        await JS.InvokeVoidAsync("mostrarToast", "✅ Evento guardado", "success");
        //Nav.NavigateTo("/agendas");
        Nav.NavigateTo(string.IsNullOrWhiteSpace(ReturnUrl) ? "/agendas" : ReturnUrl);
    }






    protected async Task ResincronizarAsync()
    {


        if (!EsEdicion)
        {
            await Toast("Guardá el evento antes de resincronizar.", "warning");
            return;
        }
        //var usuarioId = UsuarioGoogleId ?? Agenda.UsuarioId;
        //var usuarioId = GetUsuarioGoogleParaAccion();
        //if (usuarioId is null)
        //{
        //    await Toast("Seleccioná un Usuario Google para actualizar en Calendar.", "warning");
        //    return;
        //}


        //await SincronizarConGoogleApiAsync(Id!.Value, usuarioId.Value, "♻️ Evento actualizado en Google");
        var userId = await GetUsuarioGoogleValidoAsync();
        if (userId is null) return;

        await SincronizarConGoogleApiAsync(Id!.Value, userId.Value, "♻️ Evento actualizado en Google");
    }

    private async Task SincronizarConGoogleApiAsync(int agendaId, int usuarioId, string toastMsg)
    {
        //try
        //{
        //    var sync = await GoogleService.SincronizarAgendaAsync(agendaId, usuarioId);
        //    Agenda.GoogleRegistrado = sync.GoogleRegistrado;
        //    Agenda.GoogleHtmlLink = sync.GoogleHtmlLink;
        //    Agenda.GoogleLastSyncUtc = sync.GoogleLastSyncUtc;

        //    // Actualizo baseline para que no vuelva a pedir resincronizar si no hay más cambios
        //    UpdateBaselineFromCurrentForm();
        //    await Toast(toastMsg, "success");
        //}
        //catch (HttpRequestException ex)
        //{
        //    await Toast($"No se pudo sincronizar con Google. {ex.Message}", "error");
        //}

        var userId = await GetUsuarioGoogleValidoAsync();
        if (userId is null) return;

        try
        {
            var sync = await GoogleService.SincronizarAgendaAsync(agendaId, userId.Value);
            Agenda.GoogleRegistrado = sync.GoogleRegistrado;
            Agenda.GoogleHtmlLink = sync.GoogleHtmlLink;
            Agenda.GoogleLastSyncUtc = sync.GoogleLastSyncUtc;
            Agenda.GoogleUsuarioId = sync.GoogleUsuarioId;    // 👈
            GoogleEmail = sync.GoogleEmailSnapshot;
            UpdateBaselineFromCurrentForm();
            await JS.InvokeVoidAsync("mostrarToast", "📤 Evento creado en Google", "success");
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("GoogleAuthConflict"))
        {
            await Toast("No se pudo sincronizar con Google: " + ex.Message, "error");
            return;
        }
    }

    private void UpdateBaselineFromCurrentForm()
    {
        // Asegurar misma representación que el form (AR/Unspecified)
        DateTime AsUnspecified(DateTime dt) => DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);

        _original ??= new AgendaDto();
        _original.Titulo = Agenda.Titulo;
        //_original.FechaAgendada = Agenda.FechaAgendada;
        //_original.FechaFin = Agenda.FechaFin;
        _original.FechaAgendada = AsUnspecified(Agenda.FechaAgendada);
        _original.FechaFin = Agenda.FechaFin.HasValue ? AsUnspecified(Agenda.FechaFin.Value) : (DateTime?)null;
        _original.EstadoAgenda = Agenda.EstadoAgenda;
        _original.TipoAgendamientoId = Agenda.TipoAgendamientoId;
        _original.Observaciones = Agenda.Observaciones;
        _original.PersonaId = Agenda.PersonaId;
        _original.UsuarioId = Agenda.UsuarioId;
        _original.GoogleRegistrado = Agenda.GoogleRegistrado ?? _original.GoogleRegistrado;
        _original.GoogleHtmlLink = Agenda.GoogleHtmlLink ?? _original.GoogleHtmlLink;
        _original.GoogleLastSyncUtc = Agenda.GoogleLastSyncUtc ?? _original.GoogleLastSyncUtc;
        _original.GoogleUsuarioId = Agenda.GoogleUsuarioId ?? _original.GoogleUsuarioId;
    }

    protected async Task EliminarDeGoogle()
    {


        //var usuarioId = UsuarioGoogleId ?? Agenda.UsuarioId;
        //var usuarioId = GetUsuarioGoogleParaAccion();
        //if (usuarioId is null)
        //{
        //    await Toast("Seleccioná un Usuario Google para eliminar en Calendar.", "warning");
        //    return;
        //}

        //var ok = await JS.InvokeAsync<bool>("confirm", "¿Eliminar el evento en Google Calendar?");
        //if (!ok) return;

        //await GoogleService.EliminarEventoAgendaAsync(Id!.Value, usuarioId.Value);
        //await Toast("🗑 Evento eliminado en Google", "success");

        //// Opcional: marcar como no sincronizado en la UI
        //Agenda.GoogleRegistrado = false;
        //Agenda.GoogleHtmlLink = null;
        //Agenda.GoogleLastSyncUtc = null;
        //UpdateBaselineFromCurrentForm();
        //StateHasChanged();

        var userId = await GetUsuarioGoogleValidoAsync();
        if (userId is null) return;

        var ok = await JS.InvokeAsync<bool>("confirm", "¿Eliminar el evento en Google Calendar?");
        if (!ok) return;

        await GoogleService.EliminarEventoAgendaAsync(Id!.Value, userId.Value);
        await Toast("🗑 Evento eliminado en Google", "success");

        Agenda.GoogleRegistrado = false;
        Agenda.GoogleHtmlLink = null;
        Agenda.GoogleLastSyncUtc = null;
        UpdateBaselineFromCurrentForm();
        StateHasChanged();
    }

    protected void Cancelar() 
        => Nav.NavigateTo(string.IsNullOrWhiteSpace(ReturnUrl) ? "/agendas" : ReturnUrl);

    private async Task Toast(string msg, string tipo) =>
        await JS.InvokeVoidAsync("mostrarToast", msg, tipo);




    
}
