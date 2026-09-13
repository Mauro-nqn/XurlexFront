using IurixBlazor.Pages;
using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Helpers;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.JSInterop;


public class MovimientosPorProcesoBase : ComponentBase
{
    [Inject] protected MovimientoService MovimientoService { get; set; } = default!;
    [Inject] protected ArchivoMovimientoService ArchivoMovimientoService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    [Inject] protected GestionService GestionService { get; set; } = default!;
    [Inject] protected UsuarioService UsuarioService { get; set; } = default!;
    [Inject] protected ProcuracionService ProcuracionService { get; set; } = default!;
    [Inject] protected DextraService DextraService { get; set; } = default!;

    [Parameter] public string Tipo { get; set; } = "Judicial";
    [Parameter] public int ProcesoId { get; set; }

    protected List<MovimientoDto> Movimientos { get; set; } = new();
    protected string Titulo => Tipo == "Judicial" ? "Proceso Judicial" : "Proceso Extrajudicial";
    protected string? NombreProceso;
    protected string? TipoProceso;
    protected string? Materia;
    protected string? NumeroExpediente;
    public int? JuzgadoAplicacionDextraId { get; set; }       // numérico (14)
    public string? JuzgadoDextraAplicacionExacta { get; set; } // “JUZGADOS CUTRAL CO”


    protected bool FormularioVisible = false;
    

    protected int UsuarioId;

    protected ProcesoInfoDto? DatosProceso;

    protected bool TieneAgenda { get; set; }
    protected DateTime? AgendaFechaAgendadaUtc { get; set; }
    protected HashSet<int> _syncing = new();
    protected bool IsSyncing(int id) => _syncing.Contains(id);

    protected AdjuntosModalBase? _adjuntosModal;

    protected VerMovimientoModalBase? _verModal;

    protected string TextoBotonDextra { get; set; } = "Dextra";
    protected bool IsConsulting { get; set; } = false;



    protected bool PuedeSincronizar =>
    string.Equals(Tipo, "Judicial", StringComparison.OrdinalIgnoreCase)
    && DatosProceso is not null
    && (
        (DatosProceso.JuzgadoAplicacionDextraId ?? 0) > 0
        || !string.IsNullOrWhiteSpace(DatosProceso.JuzgadoDextraAplicacionExacta)
    );


    protected bool esMovil = false;

    protected bool esMovilReal = false;

    protected static Task Delay(int ms) => Task.Delay(ms);


    protected bool Escuchando = false;

    protected bool MicHabilitado { get; set; } = false;

    protected bool MostrarSugerenciaMic { get; set; } = false;

    private readonly HashSet<int> _leyendo = new();

    private CancellationTokenSource? _ttsCts;
    private int _readingMovId = 0;


    private DateTime _lastTap = DateTime.MinValue;

    protected bool EsIOS;


    protected override async Task OnInitializedAsync()
    {
        
        await RecargarMovimientos();

        

        // Cargar nombre del proceso si querés mostrarlo
        DatosProceso = await GestionService.ObtenerDatosProcesoAsync(Tipo, ProcesoId);

        if (DatosProceso is null)
        {
            Console.WriteLine("❌ No se encontraron datos del proceso.");
            return;
        }

        // Normalizar el tipo recibido
        var tipoNormalizado = Tipo?.Trim().ToLowerInvariant();

        switch (tipoNormalizado)
        {
            case "judicial":
                Tipo = "Judicial";
                //NombreProceso = !string.IsNullOrWhiteSpace(DatosProceso.Caratula)
                //    ? DatosProceso.Caratula
                //    : $"Expediente: {DatosProceso.NumeroExpediente}";

                NombreProceso = !string.IsNullOrWhiteSpace(DatosProceso.Caratula)
                            ? DatosProceso.Caratula
                            : ("Proceso Judicial");
             
                NumeroExpediente = DatosProceso.NumeroExpediente;
                TipoProceso = DatosProceso.TipoProceso;                // si viene null, queda null
                /*TipoProceso = "Judicial";*/                      // para mostrar en cabezal si querés
                //JuzgadoDextraAplicacionExacta = string.IsNullOrWhiteSpace(DatosProceso.JuzgadoDextraAplicacionExacta)
                //    ? null
                //    : DatosProceso.JuzgadoDextraAplicacionExacta;
                // si viene numérico
                if (DatosProceso.JuzgadoAplicacionDextraId is int appId
                    && Enum.IsDefined(typeof(AplicacionDextra), appId))
                {
                    var app = (AplicacionDextra)appId;
                    JuzgadoDextraAplicacionExacta = app.ToExactString(); // usa EnumMember.Value
                }
                else if (!string.IsNullOrWhiteSpace(DatosProceso.JuzgadoDextraAplicacionExacta))
                {
                    // si ya viene el texto exacto, usalo
                    JuzgadoDextraAplicacionExacta = DatosProceso.JuzgadoDextraAplicacionExacta;
                }
                else
                {
                    JuzgadoDextraAplicacionExacta = null;
                }


                break;

            case "extrajudicial":
                Tipo = "Extrajudicial";
                NombreProceso = $"{DatosProceso.TipoProcesoExt}";
                Materia = $"{DatosProceso.Materia}";
                break;

            default:
                System.Diagnostics.Debug.WriteLine($"❌ Tipo no válido: {Tipo}");
                return;
        }


        // Cargar UsuarioId
        // Recuperar usuario desde localStorage
        string usuarioIdStr = await JS.InvokeAsync<string>("localStorage.getItem", "usuario_id");

        if (!int.TryParse(usuarioIdStr, out int usuarioId))
        {
            usuarioId = 0; // o lanzar una excepción si es obligatorio
        }

        UsuarioId = usuarioId;
    }


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
       

        try
        {
            var width = await JS.InvokeAsync<int>("obtenerAnchoPantalla");
            esMovil = width < 768; // breakpoint Bootstrap sm/md

            esMovilReal = await JS.InvokeAsync<bool>("blazorHelpers.esMovilReal");

            EsIOS = await JS.InvokeAsync<bool>("xurlexDevice.isIOS");

            var mic = await JS.InvokeAsync<string>("localStorage.getItem", "mic_ok");
            MicHabilitado = mic == "1";

            // Solo sugerencia, NO pedir permiso acá
            MostrarSugerenciaMic = esMovilReal && !MicHabilitado;

            StateHasChanged();
        }
        catch
        {
            MicHabilitado = false;
            EsIOS = false;

            MostrarSugerenciaMic = false;

        }
    }


    protected string? TooltipAgenda(DateTime? utc)
    {
        if (utc is null) return null;

        // asegurar Kind=Utc y convertir con tu helper
        var arUnspec = HoraHelper.FromUtcToArUnspecified(
            DateTime.SpecifyKind(utc.Value, DateTimeKind.Utc)
        );

        return arUnspec.ToString("dd/MM/yyyy HH:mm");
    }


    protected async Task RecargarMovimientos()
    {
        Movimientos = await MovimientoService.ObtenerPorProcesoAsync(Tipo, ProcesoId);
    }

    protected void EditarMovimiento(int id)
    {
        Nav.NavigateTo($"/movimientos/editar/{id}");
    }

    //protected async Task SyncUno(int procesoJudicialId)
    //{
    //    System.Diagnostics.Debug.WriteLine($"Cargando movimientos syncro Dextra via:{procesoJudicialId}");

    //    _syncing.Add(procesoJudicialId);
    //    StateHasChanged();        // refresca la UI
    //    try
    //    {
    //        var r = await DextraService.SyncUnoAsync(procesoJudicialId);
    //        await JS.InvokeVoidAsync("alert",
    //            r is null ? "Sin respuesta" :
    //            $"Expediente: {r.ExpedienteUi}\nNovedades nuevas: {r.Nuevos}\nActuaciones (vista Dextra): {r.Total}");
    //        await RecargarMovimientos();
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



    //VERSION QUE FUNCIONABA
    //protected async Task SyncUno(int procesoJudicialId)
    //{
    //    // 1) CONSULTANDO: preview
    //    // 1) Modo CONSULTANDO
    //    IsConsulting = true;
    //    TextoBotonDextra = "Consultando…";
    //    StateHasChanged();

    //    // 1) Preview: cuántas actuaciones hay
    //    var pre = await DextraService.SyncUnoAsync(procesoJudicialId, preview: true);

    //    IsConsulting = false;
    //    TextoBotonDextra = "Dextra";
    //    StateHasChanged();

    //    if (pre is null)
    //    {
    //        await JS.InvokeVoidAsync("mostrarToast", "No se pudo obtener el resumen de Dextra.", "error");
    //        return;
    //    }

    //    var msg = $"El expediente tiene {pre.Total} actuaciones en DEXTRA " +
    //              $"(de las cuales {pre.Nuevos} serían nuevas). " +
    //              "La descarga puede demorar varios minutos. ¿Desea continuar?";

    //    var continuar = await JS.InvokeAsync<bool>("confirm", msg);
    //    if (!continuar) return;

    //    // 2) Mostrar algún estado de “sincronizando”
    //    // 2) Modo SINCRONIZANDO

    //    _syncing.Add(procesoJudicialId);
    //    TextoBotonDextra = "Sincronizando…";
    //    StateHasChanged();

    //    try
    //    {
    //        var r = await DextraService.SyncUnoAsync(procesoJudicialId, preview: false);

    //        await JS.InvokeVoidAsync("alert",
    //            r is null
    //                ? "Sin respuesta"
    //                : $"Expediente: {r.ExpedienteUi}\nNuevas actuaciones: {r.Nuevos}\nActuaciones totales (vista DEXTRA): {r.Total}");

    //        await RecargarMovimientos();
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


    protected async Task SyncUno(int procesoJudicialId)
    {
        // 1) Modo CONSULTANDO
        IsConsulting = true;
        TextoBotonDextra = "Consultando…";
        StateHasChanged();

        try
        {
            // Preview
            var pre = await DextraService.SyncUnoAsync(procesoJudicialId, preview: true);

            IsConsulting = false;
            TextoBotonDextra = "Dextra";
            StateHasChanged();

            if (pre is null)
            {
                await JS.InvokeVoidAsync("mostrarToast", "No se pudo obtener el resumen de DEXTRA.", "error");
                return;
            }

            var msg = $"El expediente tiene {pre.Total} actuaciones en DEXTRA " +
                      $"(de las cuales {pre.Nuevos} serían nuevas). " +
                      "La descarga puede demorar varios minutos. ¿Desea continuar?";

            var continuar = await JS.InvokeAsync<bool>("confirm", msg);
            if (!continuar)
                return;

            // 2) Modo SINCRONIZANDO
            _syncing.Add(procesoJudicialId);
            TextoBotonDextra = "Sincronizando…";
            StateHasChanged();

            var r = await DextraService.SyncUnoAsync(procesoJudicialId, preview: false);

            await JS.InvokeVoidAsync("alert",
                r is null
                    ? "Sin respuesta"
                    : $"Expediente: {r.ExpedienteUi}\nNuevas actuaciones: {r.Nuevos}\nActuaciones totales (vista DEXTRA): {r.Total}");

            await RecargarMovimientos();
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("alert", $"Error: {ex.Message}");
        }
        finally
        {
            // Volver todo al estado normal
            _syncing.Remove(procesoJudicialId);
            IsConsulting = false;
            TextoBotonDextra = "Dextra";
            StateHasChanged();
        }
    }



    //protected async Task EliminarMovimiento(int id)
    //{
    //    var confirmar = await JS.InvokeAsync<bool>("confirm", "¿Eliminar este movimiento?");
    //    if (confirmar)
    //    {
    //        await MovimientoService.EliminarAsync(id);
    //        await RecargarMovimientos();
    //    }
    //}

    protected async Task EliminarMovimiento(int id)
    {
        var confirmar = await JS.InvokeAsync<bool>("confirm", "¿Eliminar este movimiento?");
        if (!confirmar) return;

        var ok = await MovimientoService.EliminarAsync(id);
        if (ok)
        {
            await JS.InvokeVoidAsync("mostrarToast", "🗑️ Movimiento eliminado.", "success");
            await RecargarMovimientos();
            return;
        }

        // Si falló, podría ser 409. Confirmá y forzá:
        var forzar = await JS.InvokeAsync<bool>("confirm",
            "Este movimiento está agendado (y puede estar en Google). ¿Eliminar Agenda y evento de Google y continuar?");
        if (!forzar) return;

        var okForce = await MovimientoService.EliminarAsync(id, force: true);
        if (okForce)
        {
            await JS.InvokeVoidAsync("mostrarToast", "🗑️ Movimiento y Agenda eliminados.", "success");
            await RecargarMovimientos();
        }
        else
        {
            await JS.InvokeVoidAsync("mostrarToast", "❌ No se pudo eliminar. Revisá el log del servidor.", "error");
        }
    }




    protected async Task EliminarTodosMovimientos()
    {
        // 1) Confirmación fuerte
        var confirmar = await JS.InvokeAsync<bool>(
            "confirm",
            "⚠️ ¿Seguro que querés eliminar TODOS los movimientos de este expediente/proceso? Esta acción no se puede deshacer."
        );

        if (!confirmar) return;

        // 2) Intento normal (sin force)
        var ok = await MovimientoService.EliminarTodosPorProcesoAsync(Tipo, ProcesoId);
        if (ok)
        {
            await JS.InvokeVoidAsync("mostrarToast", "🗑️ Se eliminaron todos los movimientos.", "success");
            await RecargarMovimientos();
            return;
        }

        // 3) Si falló, puede ser por agendas => segunda confirmación para forzar
        var forzar = await JS.InvokeAsync<bool>(
            "confirm",
            "Algunos movimientos tienen Agenda / eventos en Google. ¿Eliminar también esas agendas y eventos y continuar?"
        );

        if (!forzar) return;

        var okForce = await MovimientoService.EliminarTodosPorProcesoAsync(Tipo, ProcesoId, force: true);
        if (okForce)
        {
            await JS.InvokeVoidAsync("mostrarToast", "🗑️ Se eliminaron todos los movimientos y sus agendas.", "success");
            await RecargarMovimientos();
        }
        else
        {
            await JS.InvokeVoidAsync("mostrarToast", "❌ No se pudieron eliminar todos los movimientos. Revisá el log del servidor.", "error");
        }
    }









    protected async Task AbrirAdjuntos(int movimientoId)
    {
        if (_adjuntosModal is not null)
            await _adjuntosModal.ShowAsync(movimientoId);
    }



    protected async Task VerAsync(MovimientoDto mov)
    {
        // Si ya tenés el nombre de proceso en la vista:
        var nombreProceso = mov.ProcesoJudicialId.HasValue || mov.ProcesoExtrajudicialId.HasValue
          ? (mov.ProcesoJudicialId.HasValue ? "Judicial" : "Extrajudicial") // o traelo de tu servicio si querés más exacto
          : null;

        if (_verModal is not null)
            await _verModal.ShowAsync(mov.Id, nombreProceso);
    }




    //protected async Task LeerMovimientoAsync(int movimientoId)
    //{
    //    var dto = await MovimientoService.ObtenerTtsAsync(movimientoId);

    //    if (string.IsNullOrWhiteSpace(dto?.Texto))
    //        return;

    //    //await JS.InvokeVoidAsync("tts.speak", dto.Texto, "es-AR");

    //    await JS.InvokeVoidAsync("tts.speakQueued", dto.Texto, "es-AR", new
    //    {
    //        rate = 1.0,
    //        pitch = 1.0,
    //        chunkLen = 220,
    //        gapMs = 140,
    //        voiceName = (string?)null // o la voz elegida
    //    });



    //}


    //protected async Task LeerMovimientoAsync(int movId)
    //{
    //    var tts = await MovimientoService.ObtenerTtsAsync(movId);

    //    await JS.InvokeVoidAsync("tts.speakQueued", tts.Texto, "es-AR", new { chunkLen = 220, gapMs = 140 });

    //    if (tts.CantidadAdjuntos > 0)
    //    {
    //        var ok = await JS.InvokeAsync<bool>("confirm", $"Este movimiento tiene {tts.CantidadAdjuntos} adjunto(s). ¿Querés que lea el adjunto?");
    //        if (!ok) return;

    //        // elegís cuál: por ahora el primero
    //        var adjuntos = await ArchivoMovimientoService.ObtenerAdjuntosAsync(movId); // ya tenés modal, podés reutilizar
    //        var primero = adjuntos.FirstOrDefault();
    //        if (primero is null) return;

    //        var aTts = await ArchivoMovimientoService.ObtenerArchivoTtsAsync(primero.Id);

    //        if (!aTts.Ok)
    //        {
    //            await JS.InvokeVoidAsync("mostrarToast", aTts.Error ?? "No se pudo leer el adjunto.", "warning");
    //            return;
    //        }

    //        await JS.InvokeVoidAsync("tts.speakQueued", aTts.Texto, "es-AR", new { chunkLen = 220, gapMs = 140 });
    //    }
    //}

    //protected async Task LeerMovimientoAsync(int movId)
    //{
    //    var ttsMov = await MovimientoService.ObtenerTtsAsync(movId);
    //    if (ttsMov is null) return;

    //    await JS.InvokeVoidAsync("tts.speakQueued", ttsMov.Texto, "es-AR",
    //        new { chunkLen = 220, gapMs = 140 });

    //    if (ttsMov.CantidadAdjuntos <= 0) return;

    //    var ok = await JS.InvokeAsync<bool>("confirm",
    //        $"Este movimiento tiene {ttsMov.CantidadAdjuntos} adjunto(s). ¿Querés que lea el adjunto?");

    //    if (!ok) return;

    //    // Traer adjuntos
    //    var adjuntos = await ArchivoMovimientoService.ObtenerAdjuntosAsync(movId);
    //    var primero = adjuntos.FirstOrDefault();
    //    if (primero is null) return;

    //    var ttsArch = await ArchivoMovimientoService.ObtenerArchivoTtsAsync(primero.Id);

    //    if (ttsArch is null || !ttsArch.Ok)
    //    {
    //        await JS.InvokeVoidAsync("mostrarToast", ttsArch?.Error ?? "No se pudo leer el adjunto.", "warning");
    //        return;
    //    }

    //    await JS.InvokeVoidAsync("tts.speakQueued", ttsArch.Texto, "es-AR",
    //        new { chunkLen = 220, gapMs = 140 });
    //}

    //protected async Task LeerMovimientoAsync(int movId)
    //{
    //    await JS.InvokeVoidAsync("tts.stop");

    //    var ttsMov = await MovimientoService.ObtenerTtsAsync(movId);
    //    if (ttsMov is null) return;

    //    // ✅ Esperar a que termine de leer el movimiento
    //    var terminoMov = await JS.InvokeAsync<bool>(
    //        "tts.speakQueued",
    //        ttsMov.Texto,
    //        "es-AR",
    //        new { chunkLen = 220, gapMs = 140 }
    //    );

    //    if (!terminoMov)
    //    {
    //        await JS.InvokeVoidAsync("mostrarToast", "No se pudo reproducir el texto del movimiento.", "warning");
    //        return;
    //    }

    //    // Si no hay adjuntos, listo
    //    if (ttsMov.CantidadAdjuntos <= 0) return;

    //    // ✅ Recién ahora preguntar
    //    var ok = await JS.InvokeAsync<bool>(
    //        "confirm",
    //        $"Este movimiento tiene {ttsMov.CantidadAdjuntos} adjunto(s). ¿Querés que lea el adjunto?"
    //    );

    //    if (!ok) return;

    //    // Traer adjuntos
    //    var adjuntos = await ArchivoMovimientoService.ObtenerAdjuntosAsync(movId);
    //    var primero = adjuntos.FirstOrDefault();
    //    if (primero is null)
    //    {
    //        await JS.InvokeVoidAsync("mostrarToast", "No se encontraron adjuntos para leer.", "info");
    //        return;
    //    }

    //    var ttsArch = await ArchivoMovimientoService.ObtenerArchivoTtsAsync(primero.Id);
    //    if (ttsArch is null || !ttsArch.Ok)
    //    {
    //        await JS.InvokeVoidAsync("mostrarToast", ttsArch?.Error ?? "No se pudo leer el adjunto.", "warning");
    //        return;
    //    }

    //    // ✅ Esperar también el adjunto (opcional pero recomendado)
    //    await JS.InvokeAsync<bool>(
    //        "tts.speakQueued",
    //        ttsArch.Texto,
    //        "es-AR",
    //        new { chunkLen = 220, gapMs = 140 }
    //    );
    //}





    //protected async Task UnlockTts()
    //{
    //    bool ok = false;
    //    try
    //    {
    //        ok = await JS.InvokeAsync<bool>("tts.unlock", "es-AR");
    //    }
    //    catch (Exception ex)
    //    {
    //        await JS.InvokeVoidAsync("alert", $"Error unlock: {ex.Message}");
    //        return;
    //    }

    //    await JS.InvokeVoidAsync("alert", ok ? "Audio habilitado ✅" : "No se pudo habilitar audio ❌");
    //}



    protected async Task UnlockAudio()
    {
        // 🔴 IMPORTANTE: esto debe disparar speak() DIRECTO
        //await JS.InvokeVoidAsync("tts.iosUnlock");
        var ok = false;
        try { ok = await JS.InvokeAsync<bool>("tts.iosUnlock"); } catch { }
        await JS.InvokeVoidAsync("alert", ok ? "Audio habilitado ✅" : "No se pudo habilitar audio ❌");

    }


    protected async Task UnlockTts()
    {
        var ok = false;
        try { ok = await JS.InvokeAsync<bool>("tts.unlock", "es-AR"); } catch { }
        await JS.InvokeVoidAsync("alert", ok ? "Audio habilitado ✅" : "No se pudo habilitar audio ❌");
    }



    protected async Task LeerMovimientoConUnlockAsync(int movId)
    {
        // anti doble disparo (touch+click)
        var now = DateTime.UtcNow;
        if ((now - _lastTap).TotalMilliseconds < 700) return;
        _lastTap = now;

        try { _ = JS.InvokeAsync<bool>("tts.unlock", "es-AR"); } catch { }
        await Task.Delay(50);

        await LeerMovimientoAsync(movId);
    }


    protected async Task TestTts()
    {
        var ok = await JS.InvokeAsync<bool>("tts.speakQueued",
            "Probando lectura en iPhone. Si escuchás esto, funciona.",
            "es-AR",
            new { chunkLen = 120, gapMs = 120, volume = 1, rate = 1, pitch = 1 });

        await JS.InvokeVoidAsync("alert", $"tts ok = {ok}");
    }




    protected async Task LeerMovimientoAsync(int movId)
    {

        // Detectar iOS
        bool isIOS = false;
        try { isIOS = await JS.InvokeAsync<bool>("deviceInfo.isIOS"); } catch { }

        if (isIOS)
        {
            var token = await JS.InvokeAsync<string>("localStorage.getItem", "token");

            // Base URL del API (ideal: guardalo en config)
            var baseUrl = Nav.BaseUri.TrimEnd('/'); // si tu API está en el mismo host
                                                    // si tu API es otro host, poné el URL real del API (https://192.168.1.100:5001 por ejemplo)

            var res = await JS.InvokeAsync<object>("tts_readMovimientoIOS", baseUrl, token, movId);
            return;
        }


     

        if (_leyendo.Contains(movId)) return;
        _leyendo.Add(movId);

        // cancelar lectura previa si existía
        try { _ttsCts?.Cancel(); } catch { }
        _ttsCts = new CancellationTokenSource();
        var ct = _ttsCts.Token;
        _readingMovId = movId;

        try
        {
            

            await JS.InvokeVoidAsync("tts.stop");
            try { await JS.InvokeVoidAsync("voiceConfirm.stop"); } catch { }

            ct.ThrowIfCancellationRequested();

            var ttsMov = await MovimientoService.ObtenerTtsAsync(movId);
            if (ttsMov is null) return;

            ct.ThrowIfCancellationRequested();

            var terminoMov = await JS.InvokeAsync<bool>(
                "tts.speakQueued",
                ttsMov.Texto,
                "es-AR",
                new { chunkLen = 220, gapMs = 140 }
            );

            // Si el usuario apretó Stop, speakQueued te puede devolver true (por tu lógica),
            // pero igual queremos cortar el flujo:
            ct.ThrowIfCancellationRequested();

            if (!terminoMov)
            {
                await JS.InvokeVoidAsync("mostrarToast", "No se pudo reproducir el movimiento.", "warning");
                return;
            }

            // 2) ✅ si tiene escrito, preguntar y leer si acepta
            if (ttsMov.TieneEscrito && ttsMov.EscritoId.HasValue)
            {
                bool okEscrito = await PreguntarLeerEscritoAsync(ttsMov.EscritoTitulo, ct);
                ct.ThrowIfCancellationRequested();

                if (okEscrito)
                {
                    var ttsEscrito = await MovimientoService.ObtenerEscritoTtsAsync(movId, ttsMov.EscritoId.Value);
                    ct.ThrowIfCancellationRequested();

                    if (ttsEscrito is null || !ttsEscrito.Ok)
                    {
                        await JS.InvokeVoidAsync("mostrarToast", ttsEscrito?.Error ?? "No se pudo leer el escrito.", "warning");
                        return;
                    }

                    await JS.InvokeAsync<bool>(
                        "tts.speakQueued",
                        ttsEscrito.Texto,
                        "es-AR",
                        new { chunkLen = 220, gapMs = 140 }
                    );

                    ct.ThrowIfCancellationRequested();
                }
            }

            var adjuntos = await ArchivoMovimientoService.ObtenerAdjuntosAsync(movId);

            ct.ThrowIfCancellationRequested();

            if (adjuntos.Count == 0) return;

            //bool ok = false;


           



            bool ok = await PreguntarLeerAdjuntoAsync(adjuntos.Count, ct);
            ct.ThrowIfCancellationRequested();

            if (!ok) return;

            var primero = adjuntos.FirstOrDefault();
            if (primero is null) return;

            var ttsArch = await ArchivoMovimientoService.ObtenerArchivoTtsAsync(primero.Id);
            if (ttsArch is null || !ttsArch.Ok)
            {
                await JS.InvokeVoidAsync("mostrarToast", ttsArch?.Error ?? "No se pudo leer el adjunto.", "warning");
                return;
            }

            ct.ThrowIfCancellationRequested();

            if (ttsArch is null || !ttsArch.Ok) return;

            await JS.InvokeAsync<bool>(
                "tts.speakQueued",
                ttsArch.Texto,
                "es-AR",
                new { chunkLen = 220, gapMs = 140 }
            );

            ct.ThrowIfCancellationRequested();
        }
        finally
        {
            _leyendo.Remove(movId);

            // limpiar estado solo si era la lectura actual
            if (_readingMovId == movId)
            {
                Escuchando = false;
                StateHasChanged();
            }
        }

    }






    private async Task<bool> PreguntarLeerEscritoAsync(string? titulo, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var etiqueta = string.IsNullOrWhiteSpace(titulo) ? "un escrito" : $"un escrito titulado {titulo}";
        var preguntaUi = $"Este movimiento tiene {etiqueta}. ¿Querés que lo lea?";
        var preguntaVoz = $"Este movimiento tiene un escrito. ¿Querés que lo lea? Decí sí o no.";

        bool vozSoportada = false;
        try { vozSoportada = await JS.InvokeAsync<bool>("voiceConfirm.supported"); }
        catch { vozSoportada = false; }

        if (!vozSoportada)
            return await JS.InvokeAsync<bool>("ui.confirm", preguntaUi);

        await JS.InvokeAsync<bool>("tts.speakQueued", preguntaVoz, "es-AR",
            new { chunkLen = 220, gapMs = 120, rate = 1.0 });

        ct.ThrowIfCancellationRequested();
        await Task.Delay(200, ct);

        var micOk = false;
        try { micOk = await JS.InvokeAsync<bool>("mic.ensurePermission"); } catch { micOk = false; }
        ct.ThrowIfCancellationRequested();

        if (!micOk)
            return await JS.InvokeAsync<bool>("ui.confirm", preguntaUi);

        try { await JS.InvokeAsync<bool>("audioUtils.beep"); } catch { }

        Escuchando = true;
        StateHasChanged();

        try
        {
            await Task.Delay(250, ct);

            bool? respuestaVoz = null;
            try { respuestaVoz = await JS.InvokeAsync<bool?>("voiceConfirm.askYesNo", "es-AR", 7000); }
            catch { respuestaVoz = null; }

            ct.ThrowIfCancellationRequested();

            if (respuestaVoz is null)
            {
                try { await JS.InvokeVoidAsync("voiceConfirm.stop"); } catch { }
                await Task.Delay(150, ct);
                return await JS.InvokeAsync<bool>("ui.confirm", preguntaUi);
            }

            return respuestaVoz.Value;
        }
        finally
        {
            Escuchando = false;
            StateHasChanged();
        }
    }

































    private async Task<bool> PreguntarLeerAdjuntoAsync(int cantidadAdjuntos, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        bool vozSoportada = false;
        try { vozSoportada = await JS.InvokeAsync<bool>("voiceConfirm.supported"); }
        catch { vozSoportada = false; }

        if (!vozSoportada)
        {
            ct.ThrowIfCancellationRequested();
            return await JS.InvokeAsync<bool>("ui.confirm",
                $"Este movimiento tiene {cantidadAdjuntos} archivo(s) adjunto(s). ¿Querés que lea el adjunto?");
        }

        // Pregunta hablada
        await JS.InvokeAsync<bool>(
            "tts.speakQueued",
            $"¿Querés que lea el adjunto? Decí sí o no.",
            "es-AR",
            new { chunkLen = 220, gapMs = 120, rate = 1.0 }
        );

        ct.ThrowIfCancellationRequested();

        await Task.Delay(200, ct);

        // warm-up mic
        var micOk = false;
        try { micOk = await JS.InvokeAsync<bool>("mic.ensurePermission"); } catch { micOk = false; }

        ct.ThrowIfCancellationRequested();

        if (!micOk)
        {
            return await JS.InvokeAsync<bool>("ui.confirm",
                $"Este movimiento tiene {cantidadAdjuntos} archivo(s) adjunto(s). ¿Querés que lea el adjunto?");
        }

        // beep + escuchar
        try { await JS.InvokeAsync<bool>("audioUtils.beep"); } catch { }

        Escuchando = true;
        StateHasChanged();

        try
        {
            await Task.Delay(250, ct);

            bool? respuestaVoz = null;
            try
            {
                respuestaVoz = await JS.InvokeAsync<bool?>("voiceConfirm.askYesNo", "es-AR", 7000);
            }
            catch { respuestaVoz = null; }

            ct.ThrowIfCancellationRequested();

            if (respuestaVoz is null)
            {
                try { await JS.InvokeVoidAsync("voiceConfirm.stop"); } catch { }
                await Task.Delay(150, ct);

                return await JS.InvokeAsync<bool>("ui.confirm",
                    $"Este movimiento tiene {cantidadAdjuntos} archivo(s) adjunto(s). ¿Querés que lea el adjunto?");
            }

            return respuestaVoz.Value;
        }
        finally
        {
            Escuchando = false;
            StateHasChanged();
        }
    }


    //protected async Task HabilitarMicAsync()
    //{
    //    // Esto pide permiso (warm-up) y lo corta
    //    bool ok = false;
    //    try
    //    {
    //        ok = await JS.InvokeAsync<bool>("mic.ensurePermission");
    //    }
    //    catch
    //    {
    //        ok = false;
    //    }



    //    MicHabilitado = ok;

    //    if (!ok)
    //        await JS.InvokeVoidAsync("alert", "No se pudo habilitar el micrófono. Revisá permisos del navegador.");
    //    else
    //        await JS.InvokeVoidAsync("alert", "Micrófono habilitado ✅");
    //}


    //protected async Task HabilitarMicAsync()
    //{
    //    bool ok;
    //    try
    //    {
    //        ok = await JS.InvokeAsync<bool>("mic.ensurePermission");
    //    }
    //    catch
    //    {
    //        ok = false;
    //    }

    //    MicHabilitado = ok;

    //    //if (ok)
    //    //{
    //    //    MostrarSugerenciaMic = false;
    //    //    await JS.InvokeVoidAsync("localStorage.setItem", "mic_ok", "1");
    //    //    StateHasChanged();

    //    //    await JS.InvokeVoidAsync("alert", "Micrófono habilitado ✅");
    //    //}

    //    if (ok)
    //    {
    //        MicHabilitado = true;
    //        MostrarSugerenciaMic = false;
    //        await JS.InvokeVoidAsync("localStorage.setItem", "mic_ok", "1");
    //        StateHasChanged();

    //        await Task.Delay(150);
    //        await JS.InvokeVoidAsync("alert", "Micrófono habilitado ✅");
    //    }

    //    else
    //    {
    //        await JS.InvokeVoidAsync("alert", "No se pudo habilitar el micrófono. Revisá permisos del navegador.");
    //    }
    //}



    protected async Task HabilitarMicAsync()
    {
        bool ok = false;
        try
        {
            // ✅ permiso + localStorage en el MISMO call JS
            ok = await JS.InvokeAsync<bool>("mic.enableAndPersist");
        }
        catch { ok = false; }

        MicHabilitado = ok || await JS.InvokeAsync<bool>("mic.getStored");
        MostrarSugerenciaMic = esMovilReal && !MicHabilitado;

        if (MicHabilitado)
            await JS.InvokeVoidAsync("mostrarToast", "Micrófono habilitado ✅", "success");
        else
            await JS.InvokeVoidAsync("mostrarToast", "No se pudo habilitar el micrófono", "warning");

        await InvokeAsync(StateHasChanged);
    }



    //protected async Task StopTts()
    //{
    //    await JS.InvokeVoidAsync("tts.stop");
    //}

    protected async Task StopTts()
    {
        try
        {
            _ttsCts?.Cancel();
        }
        catch { }

        await JS.InvokeVoidAsync("tts.stop");
        try { await JS.InvokeVoidAsync("voiceConfirm.stop"); } catch { }

        Escuchando = false;
        StateHasChanged();
    }


    protected async Task PauseTts()
    {
        await JS.InvokeVoidAsync("tts.pause");
    }

    protected async Task ResumeTts()
    {
        await JS.InvokeVoidAsync("tts.resume");
    }

    protected async Task ReleerMovimientoAsync(int movId)
    {
        await JS.InvokeVoidAsync("tts.stop");
        await Task.Delay(120);
        await LeerMovimientoAsync(movId);
    }





    public async ValueTask DisposeAsync()
    {
        await JS.InvokeVoidAsync("tts.stop");


    }



    //protected void VolverAGestiones()
    //{
    //    Nav.NavigateTo("/gestiones");
    //}

    protected async Task VolverAGestiones()
    {
        string? returnUrl = null;

        try
        {
            returnUrl = await JS.InvokeAsync<string>("localStorage.getItem", "return_url_movimientos");
        }
        catch { }

        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            // opcional: limpiar para que no quede “pegado”
            await JS.InvokeVoidAsync("localStorage.removeItem", "return_url_movimientos");
            Nav.NavigateTo(returnUrl);
            return;
        }

        // fallback
        Nav.NavigateTo("/gestiones");
    }

}
