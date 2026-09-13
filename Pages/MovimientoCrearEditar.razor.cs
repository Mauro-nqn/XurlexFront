using IurixBlazor.Pages;
using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;
using IurixBlazor.Shared.Helpers;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Org.BouncyCastle.Utilities;
using System;
using System.Globalization;
using System.Text;

public class MovimientoCrearEditarBase : ComponentBase
{
    [Inject] protected MovimientoService MovimientoService { get; set; } = default!;
    [Inject] protected GestionService GestionService { get; set; } = default!;
    [Inject] protected EscritoService EscritoService { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected UsuarioService UsuarioService { get; set; } = default!;

    [Inject] protected EditorInfoService EditorInfoService { get; set; } = default!;
    [Inject] protected MovimientoTemporalService MovimientoTemporalService { get; set; } = default!;

    [Inject] protected ArchivoMovimientoService ArchivoService { get; set; } = default!;
    [Inject] protected GoogleService GoogleService { get; set; } = default!;


    [Inject] protected TipoAgendamientoService TipoService { get; set; } = default!;

    [Inject] protected AgendaService AgendaService { get; set; } = default!;
    [Inject] protected PersonaService PersonaService { get; set; } = default!;

    [Inject] protected EscritoMovimientoService EscritoMovimientoService { get; set; } = default!;


    [Inject] protected TipoProcesoService TipoProcesoService { get; set; } = default!;

    [Inject] public ProcesoJudicialService ProcesoJudicialService { get; set; } = default!;



    [Parameter] public string? Tipo { get; set; }  // Solo en modo crear
    [Parameter] public int? ProcesoId { get; set; }
    [Parameter] public int? Id { get; set; }       // Solo en modo editar

    [Parameter] public int GestionId { get; set; }          // la gestión a la que pertenece el movimiento
    [Parameter] public string TipoProceso { get; set; } = "Judicial"; // "Judicial" o "Extrajudicial"

    protected CrearMovimientoDto Modelo { get; set; } = new();
    
    protected string? NombreProceso;

    protected string? NumeroExpediente;
    protected bool EsEdicion => Id.HasValue;

    protected bool ModalEditorVisible = false;

    protected int? MovimientoId { get; set; }

    //protected bool editorInicializado = false;


    //protected string? FiltroEscritoModal;
    //protected List<EscritoDto> EscritosModal = new();
    //protected int? EscritoSeleccionadoIdModal;









    private bool contenidoEditorCargado = false;

    protected string EscritoPersonalizadoResumen { get; set; } = string.Empty;
    protected string EscritoTitulo { get; set; } = string.Empty;


    protected IBrowserFile[] ArchivosSeleccionados = Array.Empty<IBrowserFile>();
    protected List<ArchivoTemporalDto> ArchivosTemporales = new();


    // 🔹 NUEVO: ids de archivos ya existentes a eliminar en BD
    protected List<int> ArchivosMarcadosParaEliminar { get; set; } = new();



    protected bool MostrarModalArchivo = false;
    protected ArchivoTemporalDto? ArchivoSeleccionadoParaVer;

    protected List<UsuarioDto> Usuarios { get; set; } = new();
    
    protected List<PersonaDto> Personas { get; set; } = new();

    protected bool PersonasCargadas { get; set; } = false;

    protected int? AgendaPersonaId { get; set; }
    protected bool AgendaPersonaBloqueada { get; set; } // true = tomada de gestión o agenda
    protected string? AgendaPersonaNombreFijo { get; set; } // para mostrar cuando está bloqueada

    protected bool Agendar { get; set; }
    protected int? AgendaTipoAgendamientoId { get; set; }
    //protected DateTime AgendaInicioLocalDate { get; set; } = DateTime.Today;
    //protected string AgendaInicioLocalTimeText { get; set; } = "09:00"; // "HH:mm"

    // Hora local de AR, sin zona (Unspecified)
    //protected DateTime AgendaFechaHoraLocal { get; set; } =
    //    DateTime.SpecifyKind(DateTime.Today.AddHours(9), DateTimeKind.Unspecified);


    protected DateTime AgendaFechaHoraLocal { get; set; }

    protected DateTime AgendaFechaHoraFin { get; set; }
    protected int AgendaDuracionMin { get; set; } = 60;
    protected int? AgendaExistenteId { get; set; }
    protected string? AgendaObservaciones { get; set; }

    protected bool AgendaNotificarApp { get; set; }
    protected int? AgendaNotificarAppUsuarioId { get; set; }
    protected bool AgendaNotificarEmail { get; set; }
    protected int? AgendaNotificarEmailUsuarioId { get; set; }
    protected string? AgendaNotificarEmailTo { get; set; }
    protected int? AgendaRecordatorioMinAntes { get; set; }

    // Catálogos
    protected List<TipoAgendamientoDto> TiposAgendamiento { get; set; } = new();


    // selects
    protected int[] DuracionesMin = new[] { 15, 30, 45, 60, 90, 120 };
    protected (int Min, string Label)[] DuracionesLabeled = Array.Empty<(int, string)>();



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


    protected EstadoAgenda? FiltroEstado { get; set; } = EstadoAgenda.Pendiente;

    //protected int AgendaEstadoId { get; set; }

    protected int? AgendaUsuarioId { get; set; }


    

    // VISIBILIDAD
    protected bool AgendarEsParaTodos { get; set; } = true;
    protected HashSet<int> AgendarUsuarioIds { get; set; } = new();

    // GOOGLE (opcional)
    protected bool AgendarSincronizarGoogle { get; set; } = false;
    protected int? AgendarUsuarioGoogleId { get; set; }
    protected string? AgendarGoogleEmail { get; set; }
    protected bool? AgendarGoogleVinculado { get; set; } // null: sin seleccionar



    protected bool AgendaGoogleRegistrado { get; set; } = false;
    protected string? AgendaGoogleHtmlLink { get; set; }
    protected DateTime? AgendaGoogleLastSyncUtc { get; set; }



    protected bool IsSaving;


    protected bool AgendarOriginal { get; set; } // estado al cargar


    // ===== Baseline para detectar cambios que afectan Google =====
    protected AgendaSnapshot? _agendaBaseline;

    protected record AgendaSnapshot(
    string Titulo,
    DateTime InicioLocal,
    DateTime? FinLocal,
    string? AgendaObservaciones,
    int DuracionMin
);





    protected List<TipoProcesoDto> TiposProceso { get; set; } = new();
    private int? _apremioTipoProcesoId;
    protected bool EsApremioMovimiento { get; set; }



    protected override async Task OnInitializedAsync()
    {
        TiposAgendamiento = await TipoService.ObtenerTodosAsync();
        //Usuarios = await UsuarioService.ObtenerUsuariosAsync() ;
        Usuarios = await UsuarioService.ObtenerUsuariosAsync() ?? new List<UsuarioDto>();
        Personas = await PersonaService.ObtenerPersonasAsync();

        


        await ObtenerUsuarioId();

        //Pasamos al select de hora del evento fecha actual y 1 hr adelante
        var tz = HoraHelper.GetArgentinaTz();

        //var tz = HoraHelper.GetArgentinaTz();
        var nowAr = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz).DateTime;

        // opcional: truncar segundos para el input "datetime-local"
        nowAr = new DateTime(nowAr.Year, nowAr.Month, nowAr.Day, nowAr.Hour, nowAr.Minute, 0);

        AgendaFechaHoraLocal = HoraHelper.TomorrowAtHourArUnspecified(12);


        AgendaFechaHoraFin = DateTime.SpecifyKind(AgendaFechaHoraLocal.AddHours(1), DateTimeKind.Unspecified);




        DuracionesLabeled = DuracionesMin
            .Select(m => (m, FormatMinutes(m)))
            .ToArray();

        var evento = TiposAgendamiento.FirstOrDefault(t => t.Nombre.Equals("Evento", StringComparison.OrdinalIgnoreCase));
        if (evento != null) AgendaTipoAgendamientoId = evento.Id;

        if (EsEdicion)
        {
            var movimiento = await MovimientoService.ObtenerPorIdAsync(Id!.Value);
            if (movimiento is null)
            {
                Nav.NavigateTo("/gestiones");
                return;
            }

            MovimientoId = movimiento.Id;

            Modelo = new CrearMovimientoDto
            {
                //Id = movimiento.Id,
                Titulo = movimiento.Titulo,
                Detalle = movimiento.Detalle,
                Fecha = HoraHelper.FromUtcToArUnspecified(movimiento.Fecha),
                ProcesoJudicialId = movimiento.ProcesoJudicialId,
                ProcesoExtrajudicialId = movimiento.ProcesoExtrajudicialId,
                EscritoPersonalizadoHtml = movimiento.EscritoPersonalizadoHtml,
                EscritoPersonalizadoTitulo = movimiento.EscritoPersonalizadoTitulo,
                //UsuarioId = movimiento.UsuarioId
            };
            // Si tiene archivos cargados, agregarlos a la lista visual
            // Cargar archivos desde DB
            if (movimiento.Archivos != null && movimiento.Archivos.Any())
            {
                ArchivosTemporales = movimiento.Archivos
                    .Select(a => new ArchivoTemporalDto
                    {
                        Id = a.Id,
                        MovimientoId = a.MovimientoId,
                        Nombre = a.Nombre,
                        TipoMime = a.TipoMime,
                        Extension = a.Extension,
                        FechaSubida = a.FechaSubida,
                        //UsuarioId = a.UsuarioId
                    })
                    .ToList();
            }



            ProcesoId = movimiento.ProcesoJudicialId ?? movimiento.ProcesoExtrajudicialId;
            Tipo = movimiento.ProcesoJudicialId.HasValue ? "Judicial" : "Extrajudicial";


            ////Tomamos el Tipo Proceso Id Apremio
            //if (Tipo == "Judicial")
            //{
            //    var ProcesoJudicialId = ProcesoId;
            //    TiposProceso = await TipoProcesoService.ObtenerTodosAsync();

            //    // 👇 Buscar el tipo "APREMIO" en la lista que acabamos de traer
            //    var tipoApremio = TiposProceso
            //        .FirstOrDefault(t =>
            //            string.Equals(t.Nombre, "APREMIO", StringComparison.OrdinalIgnoreCase));

            //    _apremioTipoProcesoId = tipoApremio?.Id;


            //    await CalcularApremioAsync();
            //}

            if (!string.IsNullOrWhiteSpace(Modelo?.EscritoPersonalizadoHtml))
            {
                EscritoPersonalizadoResumen = ObtenerResumen(Modelo.EscritoPersonalizadoHtml);
            }
            if (!string.IsNullOrWhiteSpace(Modelo?.EscritoPersonalizadoTitulo))
            {
                EscritoTitulo = Modelo.EscritoPersonalizadoTitulo;
            }


            // 2) Si hay agenda vinculada, prellenar bloque 📅
            var agenda = await AgendaService.ObtenerPorMovimientoAsync(movimiento.Id);


            // Traer persona desde la gestión por movimiento (endpoint que ya tenés)
            var gestion = await GestionService.ObtenerPersonaPorMovimientoAsync(movimiento.Id);

            // Resolver persona con prioridad: Agenda → Gestión → Select manual
            if (agenda is not null && agenda.PersonaId.HasValue)
            {
                AgendaPersonaId = agenda.PersonaId;
                AgendaPersonaNombreFijo = agenda.PersonaNombre; // si tu DTO la trae; si no, podés dejar null/"" y mostrar solo el select al “Cambiar”
                AgendaPersonaBloqueada = true;
            }
            else if (gestion?.PersonaId is int pid)
            {
                AgendaPersonaId = pid;
                AgendaPersonaNombreFijo = gestion.PersonaNombre;
                AgendaPersonaBloqueada = true;
            }
            else
            {
                AgendaPersonaId = null;
                AgendaPersonaBloqueada = false;
                Personas = await PersonaService.ObtenerPersonasAsync();
                PersonasCargadas = true;


            }


            if (agenda is not null)
            {
                AgendaExistenteId = agenda.Id; // ← clave para decidir update vs create

                Agendar = true; // prender el switch

                AgendaPersonaId = agenda.PersonaId; 

                AgendaTipoAgendamientoId = agenda.TipoAgendamientoId;

                AgendaUsuarioId = agenda.UsuarioId;

                FiltroEstado = agenda.EstadoAgenda;

                //AgendaFechaHoraFin = agenda.FechaFin;

                AgendaObservaciones = agenda.Observaciones;


                // convertir UTC -> AR local
                //var tz = HoraHelper.GetArgentinaTz();
                var inicioUtc = DateTime.SpecifyKind(agenda.FechaAgendada, DateTimeKind.Utc);
                var inicioLocal = TimeZoneInfo.ConvertTimeFromUtc(inicioUtc, tz);

                // MUY IMPORTANTE: guardarlo como Unspecified
                AgendaFechaHoraLocal = DateTime.SpecifyKind(inicioLocal, DateTimeKind.Unspecified);

                // Duración
                if (agenda.FechaFin.HasValue)
                {
                    var finLocal = TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.SpecifyKind(agenda.FechaFin.Value, DateTimeKind.Utc), tz);
                    AgendaDuracionMin = (int)Math.Max(15, (finLocal - inicioLocal).TotalMinutes);
                    AgendaFechaHoraFin = finLocal;
                }
                else
                {
                    //AgendaFechaHoraLocal = HoraHelper.TomorrowAtHourArUnspecified(12);
                    AgendaDuracionMin = 60;
                    RecalcularFin();
                    //AgendaDuracionMin = 60;

                }



                //AgendaObservaciones = agenda.Observaciones;

                // Si ya agregaste campos de notificación al modelo:
                AgendaNotificarApp = agenda.NotificarApp;
                AgendaNotificarAppUsuarioId = agenda.NotificarAppUsuarioId;
                AgendaNotificarEmail = agenda.NotificarEmail;
                AgendaNotificarEmailUsuarioId = agenda.NotificarEmailUsuarioId;
                AgendaNotificarEmailTo = agenda.NotificarEmailTo;
                AgendaRecordatorioMinAntes = agenda.RecordatorioMinAntes;


                // VISIBILIDAD
                AgendarEsParaTodos = agenda.EsParaTodos;
                AgendarUsuarioIds.Clear();
                if (!AgendarEsParaTodos && agenda.UsuarioIds is not null)
                    foreach (var id in agenda.UsuarioIds) AgendarUsuarioIds.Add(id);

                // GOOGLE (si querés precargar lo último usado)
                AgendarUsuarioGoogleId = agenda.GoogleUsuarioId;
                AgendarGoogleEmail = agenda.GoogleEmailSnapshot;
                AgendarGoogleVinculado = !string.IsNullOrWhiteSpace(AgendarGoogleEmail);

                // Estado Google actual
                AgendaGoogleRegistrado = agenda.GoogleRegistrado;
                AgendaGoogleHtmlLink = agenda.GoogleHtmlLink;
                AgendaGoogleLastSyncUtc = agenda.GoogleLastSyncUtc?.UtcDateTime;

               

                // NOTIFICACIONES (solo email / recordatorio)
                AgendaNotificarEmail = agenda.NotificarEmail;
                AgendaNotificarEmailUsuarioId = agenda.NotificarEmailUsuarioId;
                AgendaNotificarEmailTo = agenda.NotificarEmailTo;
                AgendaRecordatorioMinAntes = agenda.RecordatorioMinAntes;
            }
            AgendarOriginal = Agendar;

            //AgendaFechaHoraLocal = HoraHelper.TomorrowAtHourArUnspecified(12);
            //AgendaDuracionMin = 60;
            //RecalcularFin();
            UpdateBaselineFromCurrent();


        }
        else
        {
            Modelo = new CrearMovimientoDto
            {
                //Fecha = DateTime.Today,
                Fecha = HoraHelper.FromUtcToArUnspecified(DateTime.UtcNow),
                ProcesoJudicialId = Tipo == "Judicial" ? ProcesoId : null,
                ProcesoExtrajudicialId = Tipo == "Extrajudicial" ? ProcesoId : null,
                UsuarioId = await ObtenerUsuarioId()


            };
            // sin agenda todavía
            AgendaGoogleRegistrado = false;
            AgendaGoogleHtmlLink = null;
            AgendaGoogleLastSyncUtc = null;




            //AgendaFechaHoraLocal = HoraHelper.TomorrowAtHourArUnspecified(12);

            //AgendaDuracionMin = 60;
            //RecalcularFin();
            //RecalcularFin();




        }



        await CargarNombreProceso();


        await CalcularApremioAsync();


        var clave = Id?.ToString() ?? "temporal";
        var info = EditorInfoService.Obtener(clave);

        if (info is not null)
        {
            Modelo!.EscritoPersonalizadoHtml = info.ContenidoHtml!;
            Modelo.EscritoPersonalizadoTitulo = info.Titulo!;
            EscritoPersonalizadoResumen = ObtenerResumen(info.ContenidoHtml ?? "");
        }


        


        // Agregar archivos temporales nuevos (sin ID, deben tener contenido)
        var temp = MovimientoTemporalService.Obtener();
       
        if (temp is not null)
        {
            Modelo!.Titulo = temp.Titulo ?? string.Empty;
            Modelo.Detalle = temp.Detalle ?? string.Empty;
            Modelo.Fecha = temp.Fecha ?? DateTime.Today;            
        }
        if (temp?.Archivos?.Any() == true)
        {
            foreach (var archivo in temp.Archivos)
            {
                if (archivo.Contenido != null && archivo.Contenido.Length > 0)
                {
                    // Evitar duplicados por nombre
                    if (!ArchivosTemporales.Any(a => a.Nombre == archivo.Nombre && a.Id == 0))
                    {
                        ArchivosTemporales.Add(archivo);
                    }
                }
            }
        }

        // 🔹 Si estoy en edición, ya tengo los archivos “reales” del movimiento,
        //    así que no quiero seguir arrastrando temporales eternamente.
        if (EsEdicion)
        {
            MovimientoTemporalService.Limpiar();
        }
    }


















    //Cargamos datos que mueven cambiar para detectar cambio y resincronizar
    //google calendar
    protected AgendaSnapshot BuildAgendaSnapshotActual()
    {
        var titulo = string.IsNullOrWhiteSpace(Modelo.Titulo) ? "Movimiento agendado" : Modelo.Titulo.Trim();
        var obs = string.IsNullOrWhiteSpace(Modelo.Detalle) ? "Movimiento agendado" : Modelo.Detalle.Trim();

        return new AgendaSnapshot(
            titulo,
            DateTime.SpecifyKind(AgendaFechaHoraLocal, DateTimeKind.Unspecified),
            DateTime.SpecifyKind(AgendaFechaHoraFin, DateTimeKind.Unspecified),
            obs,
            AgendaDuracionMin > 0 ? AgendaDuracionMin : 60
        );
    }

    protected void UpdateBaselineFromCurrent()
    {
        _agendaBaseline = BuildAgendaSnapshotActual();
    }

    // (opcional) tolerancia de segundos si querés ser más laxo al comparar
    protected static bool DiffersDate(DateTime a, DateTime b)
        => a != b; // o: Math.Abs((a - b).TotalSeconds) > 30

    private static bool DiffersStr(string? a, string? b)
        => (a ?? "").Trim() != (b ?? "").Trim();

    protected bool CambiosQueAfectanGoogleMov()
    {
        if (_agendaBaseline is null) return false;
        var cur = BuildAgendaSnapshotActual();

        var inicioChange = DiffersDate(cur.InicioLocal, _agendaBaseline.InicioLocal);
        var finChange = (cur.FinLocal.HasValue != _agendaBaseline.FinLocal.HasValue) ||
                           (cur.FinLocal.HasValue && _agendaBaseline.FinLocal.HasValue &&
                            DiffersDate(cur.FinLocal.Value, _agendaBaseline.FinLocal.Value));
        var tituloChange = DiffersStr(cur.Titulo, _agendaBaseline.Titulo);
        var obsChange = DiffersStr(cur.AgendaObservaciones, _agendaBaseline.AgendaObservaciones);
        var durChange = cur.DuracionMin != _agendaBaseline.DuracionMin;

        return inicioChange || finChange || tituloChange || obsChange || durChange;
    }

    // (opcional) para mostrar en el toast qué cambió
    protected string[] ListaCambiosGoogleMov()
    {
        if (_agendaBaseline is null) return Array.Empty<string>();
        var cur = BuildAgendaSnapshotActual();
        var cambios = new List<string>();
        if (DiffersStr(cur.Titulo, _agendaBaseline.Titulo)) cambios.Add("título");
        if (DiffersDate(cur.InicioLocal, _agendaBaseline.InicioLocal)) cambios.Add("inicio");
        if ((cur.FinLocal.HasValue != _agendaBaseline.FinLocal.HasValue) ||
            (cur.FinLocal.HasValue && _agendaBaseline.FinLocal.HasValue &&
             DiffersDate(cur.FinLocal.Value, _agendaBaseline.FinLocal.Value))) cambios.Add("fin");
        if (cur.DuracionMin != _agendaBaseline.DuracionMin) cambios.Add("duración");
        if (DiffersStr(cur.AgendaObservaciones, _agendaBaseline.AgendaObservaciones)) cambios.Add("observaciones");
        return cambios.ToArray();
    }











    //Calcular fecha fin     //Calcular Fecha Fin

    private void RecalcularFin()
    {
        if (AgendaFechaHoraLocal == default) return;

        var inicio = AgendaFechaHoraLocal; // AR / Unspecified
        var dur = Math.Max(15, AgendaDuracionMin); // mínimo 15
        var fin = inicio.AddMinutes(dur);

        AgendaFechaHoraFin = DateTime.SpecifyKind(fin, DateTimeKind.Unspecified);
        AgendaDuracionMin = dur;
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







    //private async Task CalcularApremioAsync()
    //{
    //    EsApremioMovimiento = false;
    //    _apremioTipoProcesoId = null;

    //    // Solo tiene sentido para procesos judiciales con ProcesoId
    //    if (!string.Equals(Tipo, "Judicial", StringComparison.OrdinalIgnoreCase))
    //        return;

    //    if (!ProcesoId.HasValue)
    //        return;

    //    // 1) Traer catálogo de tipos de proceso
    //    TiposProceso = await TipoProcesoService.ObtenerTodosAsync();

    //    // 2) Encontrar el tipo "APREMIO"
    //    var tipoApremio = TiposProceso
    //        .FirstOrDefault(t =>
    //            string.Equals(t.Nombre, "APREMIO", StringComparison.OrdinalIgnoreCase));

    //    _apremioTipoProcesoId = tipoApremio?.Id;

    //    if (!_apremioTipoProcesoId.HasValue)
    //        return; // No existe tipo APREMIO → no marcamos nada

    //    // 3) Traer el Proceso Judicial para obtener su TipoProcesoId
    //    var proceso = await ProcesoJudicialService.ObtenerPorIdAsync(ProcesoId.Value);
    //    if (proceso is null)
    //        return;

    //    EsApremioMovimiento = proceso.TipoProcesoId == _apremioTipoProcesoId.Value;

    //    await JS.InvokeVoidAsync("console.log", $"{EsApremioMovimiento}");

    //    System.Diagnostics.Debug.WriteLine(
    //        $"[Movimientos] ProcesoId={ProcesoId}, TipoProcesoIdProceso={proceso.TipoProcesoId}, ApremioId={_apremioTipoProcesoId}, EsApremio={EsApremioMovimiento}");
    //}

    private async Task CalcularApremioAsync()
    {
        EsApremioMovimiento = false;

        System.Diagnostics.Debug.WriteLine($"[CalcularApremio] Tipo='{Tipo}', ProcesoId={ProcesoId}");

        if (!ProcesoId.HasValue)
        {
            System.Diagnostics.Debug.WriteLine("[CalcularApremio] ProcesoId no tiene valor.");
            return;
        }

        // 1) Traer catálogo de tipos de proceso
        TiposProceso = await TipoProcesoService.ObtenerTodosAsync();
        System.Diagnostics.Debug.WriteLine($"[CalcularApremio] TiposProceso count = {TiposProceso.Count}");

        // 2) Traer el Proceso Judicial para obtener su TipoProcesoId
        var proceso = await ProcesoJudicialService.ObtenerPorIdAsync(ProcesoId.Value);
        if (proceso is null)
        {
            System.Diagnostics.Debug.WriteLine($"[CalcularApremio] No se encontró ProcesoJudicial con Id={ProcesoId.Value}");
            return;
        }

        // 3) Buscar el tipo de proceso del expediente en el catálogo
        var tipoProcesoDelExpte = TiposProceso.FirstOrDefault(t => t.Id == proceso.TipoProcesoId);

        if (tipoProcesoDelExpte is null)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[CalcularApremio] No se encontró TipoProceso con Id={proceso.TipoProcesoId} en el catálogo.");
            return;
        }

        var nombreTipo = (tipoProcesoDelExpte.Nombre ?? string.Empty).Trim();

        // 4) Regla: es Apremio si el NOMBRE contiene "APREMIO" (ignora mayúsculas/minúsculas)
        EsApremioMovimiento =
            nombreTipo.IndexOf("apremio", StringComparison.OrdinalIgnoreCase) >= 0;

        System.Diagnostics.Debug.WriteLine(
            $"[CalcularApremio] ProcesoId={ProcesoId}, TipoProcesoIdProceso={proceso.TipoProcesoId} ('{nombreTipo}'), EsApremioMovimiento={EsApremioMovimiento}");

        await JS.InvokeVoidAsync("console.log",
            $"[CalcularApremio] ProcesoId={ProcesoId}, TipoProcesoIdProceso={proceso.TipoProcesoId} ('{nombreTipo}'), EsApremioMovimiento={EsApremioMovimiento}");
    }





















    protected async Task DesbloquearPersonaAsync()
    {
        AgendaPersonaBloqueada = false;
        if (!PersonasCargadas)
        {
            Personas = await PersonaService.ObtenerPersonasAsync();
            PersonasCargadas = true;
        }
        StateHasChanged();
    }






    //nuevos campos y visual de agenda

    protected void ToggleUsuarioAgendar(int id, ChangeEventArgs e)
    {
        var on = e.Value is bool b ? b
            : string.Equals(e.Value?.ToString(), "true", StringComparison.OrdinalIgnoreCase)
              || string.Equals(e.Value?.ToString(), "on", StringComparison.OrdinalIgnoreCase);

        if (on) AgendarUsuarioIds.Add(id); else AgendarUsuarioIds.Remove(id);
    }

    protected async Task OnUsuarioGoogleChangedAsync(ChangeEventArgs e)
    {
        AgendarUsuarioGoogleId = e.Value is int v ? v
            : (int.TryParse(e.Value?.ToString(), out var p) ? p : (int?)null);

        if (AgendarUsuarioGoogleId is null)
        {
            AgendarGoogleVinculado = null;
            AgendarGoogleEmail = null;
            StateHasChanged(); return;
        }

        var estado = await UsuarioService.ObtenerEstadoGoogleAsync(AgendarUsuarioGoogleId.Value);
        AgendarGoogleEmail = estado?.Email;
        AgendarGoogleVinculado = !string.IsNullOrWhiteSpace(estado?.RefreshToken);
        StateHasChanged();
    }

    private int? GetUsuarioGoogleParaAccionMov()
        => AgendarUsuarioGoogleId; // en movimientos priorizamos lo elegido aquí

    protected async Task<int?> GetUsuarioGoogleValidoMovAsync()
    {
        var uid = GetUsuarioGoogleParaAccionMov();
        if (uid is null) { await JS.InvokeVoidAsync("mostrarToast", "Seleccioná un Usuario Google.", "warning"); return null; }

        var estado = await UsuarioService.ObtenerEstadoGoogleAsync(uid.Value);
        if (string.IsNullOrWhiteSpace(estado?.RefreshToken))
        {
            await JS.InvokeVoidAsync("mostrarToast", "Ese usuario no tiene cuenta Google vinculada.", "error");
            return null;
        }

        var ok = await GoogleService.ValidarYRefrescarTokenAsync(uid.Value);
        if (!ok)
        {
            await JS.InvokeVoidAsync("mostrarToast", "No se pudo validar el acceso a Google. Re-vinculá la cuenta.", "error");
            return null;
        }
        return uid;
    }





    protected async Task VerificarUsuarioGoogleMovAsync()
    {
        if (AgendarUsuarioGoogleId is null)
        {
            AgendarGoogleVinculado = null;
            AgendarGoogleEmail = null;
            StateHasChanged();
            return;
        }

        var estado = await UsuarioService.ObtenerEstadoGoogleAsync(AgendarUsuarioGoogleId.Value);
        AgendarGoogleEmail = estado?.Email;
        AgendarGoogleVinculado = !string.IsNullOrWhiteSpace(estado?.RefreshToken);
        StateHasChanged();
    }



    protected async Task ResincronizarAgendaMovAsync()
    {
        if (!AgendaExistenteId.HasValue)
        {
            await JS.InvokeVoidAsync("mostrarToast", "Guardá la agenda antes de resincronizar.", "warning");
            return;
        }

        var uid = await GetUsuarioGoogleValidoMovAsync();
        if (uid is null) return;

        try
        {
            var sync = await GoogleService.SincronizarAgendaAsync(AgendaExistenteId.Value, uid.Value);

            // Refrescá estado UI con lo que devuelve el server
            AgendaGoogleRegistrado = sync.GoogleRegistrado;
            AgendaGoogleHtmlLink = sync.GoogleHtmlLink;
            AgendaGoogleLastSyncUtc = sync.GoogleLastSyncUtc?.UtcDateTime;

            // Persistí selección (por si el server devuelve snapshot/id)
            AgendarUsuarioGoogleId = sync.GoogleUsuarioId ?? AgendarUsuarioGoogleId;
            AgendarGoogleEmail = string.IsNullOrWhiteSpace(sync.GoogleEmailSnapshot) ? AgendarGoogleEmail : sync.GoogleEmailSnapshot;

            await JS.InvokeVoidAsync("mostrarToast", "♻️ Evento actualizado en Google", "success");
            StateHasChanged();
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("GoogleAuthConflict"))
        {
            await JS.InvokeVoidAsync("mostrarToast", "No se pudo sincronizar con Google: " + ex.Message, "error");
        }
    }




    protected async Task EliminarAgendaMovGoogleAsync()
    {
        if (!AgendaExistenteId.HasValue)
        {
            await JS.InvokeVoidAsync("mostrarToast", "No hay agenda guardada.", "warning");
            return;
        }

        var uid = await GetUsuarioGoogleValidoMovAsync();
        if (uid is null) return;

        var ok = await JS.InvokeAsync<bool>("confirm", "¿Eliminar el evento en Google Calendar?");
        if (!ok) return;

        await GoogleService.EliminarEventoAgendaAsync(AgendaExistenteId.Value, uid.Value);

        await JS.InvokeVoidAsync("mostrarToast", "🗑 Evento eliminado en Google", "success");

        // Actualizá estado local
        AgendaGoogleRegistrado = false;
        AgendaGoogleHtmlLink = null;
        AgendaGoogleLastSyncUtc = null;
        StateHasChanged();
    }





































    private int? GetUsuarioGoogleParaAccion()
    => AgendarUsuarioGoogleId;







    protected async Task EliminarAgendaActual()
    {
        if (!AgendaExistenteId.HasValue) return;

        if (AgendaGoogleRegistrado == true)
        {
            var usuarioId = GetUsuarioGoogleParaAccion();

            if (usuarioId is null)
            {
                await Toast("Seleccioná un Usuario Google para eliminar en Calendar.", "warning");
            }
            else
            {
                var desea = await JS.InvokeAsync<bool>("confirm",
                    "Vas a eliminar el movimiento de la Agenda ¿Querés eliminarlo?");
                if (desea)
                {
                    await EliminarAgendaMovGoogleAsync(); // implementado más abajo
                    //await JS.InvokeVoidAsync("mostrarToast", "🗑️ Evento eliminado en Google", "success");
                }
            }
        }

        
        var ok = await AgendaService.EliminarAsync(AgendaExistenteId.Value /*, deleteGoogle:true, usuarioId: Modelo.UsuarioId */);
        if (ok)
        {
            AgendaExistenteId = null;
            Agendar = false;
            // Actualizá estado local
            //AgendaGoogleRegistrado = false;
            //AgendaGoogleHtmlLink = null;
            //AgendaGoogleLastSyncUtc = null;
            await JS.InvokeVoidAsync("mostrarToast", "🗑️ Agenda eliminada", "success");
            StateHasChanged();

        }
        else
        {
            await JS.InvokeVoidAsync("mostrarToast", "❌ No se pudo eliminar la agenda", "error");
        }
    }









    private static string FormatMinutes(int minutes)
    {
        var h = minutes / 60;
        var min = minutes % 60;

        if (h > 0 && min > 0) return $"{h} h {min} min";
        if (h > 0) return h == 1 ? "1 h" : $"{h} h";
        return $"{minutes} min";
    }



    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!contenidoEditorCargado)
        {
            contenidoEditorCargado = true;

            var contenido = await JS.InvokeAsync<string>("localStorage.getItem", "contenidoEditorMovimiento");
            if (!string.IsNullOrWhiteSpace(contenido))
            {
                Modelo.EscritoPersonalizadoHtml = contenido;


                var titulo = await JS.InvokeAsync<string>("localStorage.getItem", "tituloEditorMovimiento");
                if (!string.IsNullOrWhiteSpace(titulo))
                {
                    Modelo.EscritoPersonalizadoTitulo = titulo;
                    //await JS.InvokeVoidAsync("localStorage.removeItem", "tituloEditorMovimiento");
                }
                //else
                //{
                //    Modelo.EscritoPersonalizadoTitulo = ObtenerResumen(contenido);
                //}
                // Guardamos el resumen también (nuevo campo)
                EscritoPersonalizadoResumen = ObtenerResumen(contenido);            

                await JS.InvokeVoidAsync("localStorage.removeItem", "contenidoEditorMovimiento");

                // 👇 Si querés ver qué llega
                System.Diagnostics.Debug.WriteLine("✅ Contenido recibido del editor:");
                System.Diagnostics.Debug.WriteLine(Modelo.EscritoPersonalizadoHtml);
                System.Diagnostics.Debug.WriteLine("✅ Título:");
                System.Diagnostics.Debug.WriteLine(Modelo.EscritoPersonalizadoTitulo);


                StateHasChanged();
            }
           
        }
    }


    private static string ObtenerResumen(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        // 1. Decodificar entidades HTML (como &nbsp;)
        var decoded = System.Net.WebUtility.HtmlDecode(html);

        // 2. Eliminar etiquetas HTML
        var plainText = System.Text.RegularExpressions.Regex.Replace(decoded, "<.*?>", string.Empty);

        // 3. Reemplazar saltos de línea y espacios múltiples
        plainText = plainText.Replace("\n", " ")
                             .Replace("\r", " ")
                             .Trim();

        // 4. Colapsar espacios múltiples
        plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"\s+", " ");

        return plainText.Length <= 80 ? plainText : plainText.Substring(0, 80) + "...";
    }






    //protected async Task AbrirEditorMovimiento(int ? id)
    //{

    //    // 🧠 Guardar los datos actuales del movimiento
    //    MovimientoTemporalService.Guardar(new MovimientoTemporalDto
    //    {
    //        Titulo = Modelo.Titulo,
    //        Detalle = Modelo.Detalle,
    //        Fecha = Modelo.Fecha,
    //        Archivos = ArchivosTemporales
    //    });

    //    //if (id == null)
    //    //    return;
    //    // Guardar ruta de retorno y otros datos si querés
    //    await JS.InvokeVoidAsync("localStorage.setItem", "volverADespuesDeEditor", Nav.Uri);
    //    await JS.InvokeVoidAsync("localStorage.setItem", "datosProcesoEditorMovimiento", NombreProceso);



    //    // Navegar con ID
    //    // Navegar al editor pasando el ID por query

    //    if (id.HasValue)
    //    {
    //        Nav.NavigateTo($"/editor-movimiento?id={id.Value}");
    //    }
    //    else
    //    {
    //        Nav.NavigateTo("/editor-movimiento");
    //    }
    //}

    protected async Task AbrirEditorMovimiento(int? id)
    {
        // 🧠 Guardar los datos actuales del movimiento
        MovimientoTemporalService.Guardar(new MovimientoTemporalDto
        {
            Titulo = Modelo.Titulo,
            Detalle = Modelo.Detalle,
            Fecha = Modelo.Fecha,
            Archivos = ArchivosTemporales
        });

        // 🧭 Guardar ruta de retorno y datos del proceso/gestión
        await JS.InvokeVoidAsync("localStorage.setItem", "volverADespuesDeEditor", Nav.Uri);
        await JS.InvokeVoidAsync("localStorage.setItem", "datosProcesoEditorMovimiento", NombreProceso);

        // 👉 Claves que va a usar el editor + modal IA - Si lo hacemos pisamos los valores que ya pasaron
        //await JS.InvokeVoidAsync("localStorage.setItem", "gestion_actual", GestionId.ToString());
        //await JS.InvokeVoidAsync("localStorage.setItem", "proceso_actual", ProcesoId.ToString());
        //await JS.InvokeVoidAsync("localStorage.setItem", "tipo_proceso", TipoProceso);

        // 👇 NUEVO: guardamos si este movimiento es de un proceso APREMIO
        await JS.InvokeVoidAsync(
            "localStorage.setItem",
            "es_apremio",
            EsApremioMovimiento ? "true" : "false"
        );

        await JS.InvokeVoidAsync("console.log", $"{EsApremioMovimiento}");

        System.Diagnostics.Debug.WriteLine($"[Movimientos] GestionId = {GestionId}, ProcesoId = {ProcesoId}, TipoProceso = {TipoProceso}");


        // Navegar al editor pasando el ID por query si lo hay
        if (id.HasValue)
            Nav.NavigateTo($"/editor-movimiento?id={id.Value}");
        else
            Nav.NavigateTo("/editor-movimiento");
    }






    protected async Task EliminarEscritoAsync()
    {
        // Confirmación simple (podés cambiar a tu toast/modal)
        var ok = await JS.InvokeAsync<bool>("confirm", "¿Quitar el escrito de este movimiento?");
        if (!ok) return;

        try
        {
            // 1) Limpiar estado local
            Modelo.EscritoPersonalizadoHtml = null;
            Modelo.EscritoPersonalizadoTitulo = null;
            EscritoPersonalizadoResumen = string.Empty;
            EscritoTitulo = string.Empty;



            // 2) Limpiar cachés del editor
            var clave = Id?.ToString() ?? "temporal";
            EditorInfoService.Limpiar(clave);
            await JS.InvokeVoidAsync("localStorage.removeItem", "contenidoEditorMovimiento");
            await JS.InvokeVoidAsync("localStorage.removeItem", "tituloEditorMovimiento");

            // 3) Si es edición, persistir en backend
            if (EsEdicion && MovimientoId.HasValue)
            {
                // Recuperar el string del localStorage
                var userIdStr = await JS!.InvokeAsync<string>("localStorage.getItem", "usuario_id");

                // Convertirlo a int y asignarlo al DTO
                if (int.TryParse(userIdStr, out var userId))
                {
                    Modelo.UsuarioId = userId;
                }
                else
                {
                    await JS!.InvokeVoidAsync("mostrarToast", "❌ Usuario inválido en localStorage.", "error");
                    return;
                }
                // Opción A: endpoint específico para quitar escrito
                //var okApi = await MovimientoService.QuitarEscritoAsync(MovimientoId.Value);

                //Opción B(alternativa): reutilizar tu Actualizar, enviando esos campos en null
                await EscritoMovimientoService.EliminarPorMovimientoAsync(MovimientoId.Value, Modelo.UsuarioId);
                //{
                //    EscritoPersonalizadoHtml = null,
                //    EscritoPersonalizadoTitulo = null,
                //    UsuarioId = Modelo.UsuarioId 
                //});

                //if (!okApi)
                //{
                //    await JS.InvokeVoidAsync("mostrarToast", "❌ No se pudo quitar el escrito.", "error");
                //    return;
                //}
            }

            await JS.InvokeVoidAsync("mostrarToast", "🗑️ Escrito quitado", "success");
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("mostrarToast", $"❌ Error al quitar el escrito: {ex.Message}", "error");
        }
    }








    protected async Task<int> ObtenerUsuarioId()
    {
        var usuarioStr = await JS.InvokeAsync<string>("localStorage.getItem", "usuario_id");
         return int.TryParse(usuarioStr, out var id) ? id : 0;

    }


    protected async Task CargarNombreProceso()
    {
        if (!ProcesoId.HasValue || string.IsNullOrEmpty(Tipo)) return;

        var datos = await GestionService.ObtenerDatosProcesoAsync(Tipo, ProcesoId.Value);

        if (datos != null)
        {
            //NombreProceso = Tipo == "Judicial"
            //    ? (!string.IsNullOrWhiteSpace(datos.Caratula) ? datos.Caratula : $"Expediente: {datos.NumeroExpediente}")
            //    : $"{datos.Tipo} - {datos.Materia}";            

            NombreProceso = Tipo == "Judicial"
                ? BuildJudicialNombreProceso(datos.Caratula, datos.NumeroExpediente)
                : string.Join(" — ", new[] { datos.TipoProceso ?? datos.Tipo, datos.Materia }
                    .Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!.Trim()));



        }
    }



    static string BuildJudicialNombreProceso(string? caratula, string? numero)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(caratula)) parts.Add(caratula.Trim());
        if (!string.IsNullOrWhiteSpace(numero)) parts.Add($"Exp: {numero.Trim()}");
        return string.Join(" — ", parts);
    }


    //protected async Task OnArchivoSeleccionado(InputFileChangeEventArgs e)
    //{
    //    ArchivosSeleccionados = e.GetMultipleFiles().ToArray();
    //}

    //protected async Task OnArchivoSeleccionado(InputFileChangeEventArgs e)
    //{
    //    ArchivosSeleccionados = e.GetMultipleFiles().ToArray();
    //    ArchivosTemporales.Clear();

    //    foreach (var archivo in ArchivosSeleccionados)
    //    {
    //        using var ms = new MemoryStream();
    //        await archivo.OpenReadStream().CopyToAsync(ms);

    //        ArchivosTemporales.Add(new ArchivoTemporalDto
    //        {
    //            Nombre = archivo.Name,
    //            TipoMime = archivo.ContentType,
    //            Extension = Path.GetExtension(archivo.Name),
    //            Contenido = ms.ToArray()
    //        });
    //    }

    //    MovimientoTemporalService.Guardar(new MovimientoTemporalDto
    //    {
    //        Titulo = Modelo.Titulo,
    //        Detalle = Modelo.Detalle,
    //        Fecha = Modelo.Fecha,
    //        Archivos = ArchivosTemporales
    //    });
    //}

    //protected async Task OnArchivoSeleccionado(InputFileChangeEventArgs e)
    //{
    //    foreach (var archivo in e.GetMultipleFiles())
    //    {
    //        using var ms = new MemoryStream();
    //        await archivo.OpenReadStream().CopyToAsync(ms);

    //        // Eliminar archivo anterior con mismo nombre si existe
    //        ArchivosTemporales.RemoveAll(a => a.Nombre == archivo.Name && a.Id == 0);

    //        ArchivosTemporales.Add(new ArchivoTemporalDto
    //        {
    //            Nombre = archivo.Name,
    //            TipoMime = archivo.ContentType,
    //            Extension = Path.GetExtension(archivo.Name),
    //            Contenido = ms.ToArray()
    //        });
    //    }

    //    // Guardar en sesión temporal
    //    MovimientoTemporalService.Guardar(new MovimientoTemporalDto
    //    {
    //        Titulo = Modelo.Titulo,
    //        Detalle = Modelo.Detalle,
    //        Fecha = Modelo.Fecha,
    //        Archivos = ArchivosTemporales
    //    });
    //}

    protected async Task OnArchivoSeleccionado(InputFileChangeEventArgs e)
    {
        foreach (var archivo in e.GetMultipleFiles())
        {
            using var ms = new MemoryStream();
            await archivo.OpenReadStream().CopyToAsync(ms);

            // ¿Ya hay un archivo con ese nombre?
            var existente = ArchivosTemporales
                .FirstOrDefault(a => a.Nombre == archivo.Name);

            if (existente is not null)
            {
                // Si venía de BD, lo marcamos para eliminar
                if (existente.Id.HasValue && existente.Id.Value > 0)
                {
                    ArchivosMarcadosParaEliminar.Add(existente.Id.Value);
                }

                ArchivosTemporales.Remove(existente);
            }

            ArchivosTemporales.Add(new ArchivoTemporalDto
            {
                Id = 0, // nuevo
                Nombre = archivo.Name,
                TipoMime = archivo.ContentType,
                Extension = Path.GetExtension(archivo.Name),
                Contenido = ms.ToArray()
            });
        }

        // Guardar en sesión temporal
        MovimientoTemporalService.Guardar(new MovimientoTemporalDto
        {
            Titulo = Modelo.Titulo,
            Detalle = Modelo.Detalle,
            Fecha = Modelo.Fecha,
            Archivos = ArchivosTemporales
        });
    }










    protected async Task Guardar()
    {
        if (IsSaving) return;
        IsSaving = true;
        try
        {
            // 1) UsuarioId desde localStorage
            var userIdStr = await JS!.InvokeAsync<string>("localStorage.getItem", "usuario_id");
            if (!int.TryParse(userIdStr, out var userId))
            {
                await JS.InvokeVoidAsync("mostrarToast", "❌ Usuario inválido en localStorage.", "error");
                return;
            }
            Modelo.UsuarioId = userId;

            // 2) Contenido editor (si aplica)
            var clave = Id?.ToString() ?? "temporal";
            var info = EditorInfoService.Obtener(clave);
            if (info is not null)
            {
                Modelo.EscritoPersonalizadoHtml = info.ContenidoHtml ?? "";
                Modelo.EscritoPersonalizadoTitulo = info.Titulo ?? "";
            }

            //// 3) Preparar archivos NUEVOS
            //Modelo.Archivos = new List<CrearArchivoMovimientoDto>();
            //foreach (var a in ArchivosTemporales)
            //{
            //    if (a.Id == 0 && a.Contenido is { Length: > 0 })
            //    {
            //        Modelo.Archivos.Add(new CrearArchivoMovimientoDto
            //        {
            //            Nombre = a.Nombre,
            //            TipoMime = a.TipoMime,
            //            Extension = a.Extension,
            //            Contenido = a.Contenido,
            //            UsuarioId = Modelo.UsuarioId
            //        });
            //    }
            //}

            // 3) Preparar archivos NUEVOS
            Modelo.Archivos = new List<CrearArchivoMovimientoDto>();
            foreach (var a in ArchivosTemporales)
            {
                if (a.Id == 0 && a.Contenido is { Length: > 0 })
                {
                    Modelo.Archivos.Add(new CrearArchivoMovimientoDto
                    {
                        Nombre = a.Nombre,
                        TipoMime = a.TipoMime,
                        Extension = a.Extension,
                        Contenido = a.Contenido,
                        UsuarioId = Modelo.UsuarioId
                    });
                }
            }

            // 🔹 3.b) IDs de archivos existentes a eliminar
            Modelo.ArchivosEliminarIds = ArchivosMarcadosParaEliminar
                .Distinct()
                .ToList();

            // 4) Detectar cambios relevantes para Google
            var hayCambiosAfectanGoogle = CambiosQueAfectanGoogleMov();

            // 5) Guardar movimiento (primero SIEMPRE)
            //if (EsEdicion && Id.HasValue)
            //{
            //    await MovimientoService.ActualizarAsync(Id.Value, Modelo);
            //    await JS.InvokeVoidAsync("mostrarToast", "✅ Movimiento actualizado.", "success");
            //    MovimientoId = Id.Value;
            //}
            //else
            //{
            //    var creado = await MovimientoService.CrearYDevolverAsync(Modelo);
            //    if (creado is null)
            //    {
            //        await JS.InvokeVoidAsync("mostrarToast", "❌ Error al crear movimiento.", "error");
            //        return;
            //    }
            //    await JS.InvokeVoidAsync("mostrarToast", "✅ Movimiento creado.", "success");
            //    MovimientoId = creado.Id;
            //}

            // 5) Guardar movimiento (primero SIEMPRE)
            if (EsEdicion && Id.HasValue)
            {
                await MovimientoService.ActualizarAsync(Id.Value, Modelo);
                await JS.InvokeVoidAsync("mostrarToast", "✅ Movimiento actualizado.", "success");
                MovimientoId = Id.Value;
            }
            else
            {
                var creado = await MovimientoService.CrearYDevolverAsync(Modelo);
                if (creado is null)
                {
                    await JS.InvokeVoidAsync("mostrarToast", "❌ Error al crear movimiento.", "error");
                    return;
                }
                await JS.InvokeVoidAsync("mostrarToast", "✅ Movimiento creado.", "success");
                MovimientoId = creado.Id;
            }

            // 🔹 IMPORTANTE: al guardar OK, limpiar temporales para que no reaparezcan
            MovimientoTemporalService.Limpiar();
            ArchivosMarcadosParaEliminar.Clear();


            // 6) Crear/actualizar Agenda SOLO UNA VEZ (después de tener MovimientoId)
            try
            {
                await CrearAgendaSiCorrespondeAsync(MovimientoId!.Value, hayCambiosAfectanGoogle);
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("mostrarToast", $"⚠️ Movimiento OK, pero falló agenda:\n{ex.Message}", "error");
                // No cortar el flujo de navegación
            }

            // 7) Navegar
            if (Tipo != null && ProcesoId.HasValue)
                Nav.NavigateTo($"/movimientos/{Tipo}/{ProcesoId.Value}");
            else
                Nav.NavigateTo("/gestiones");
        }
        catch (HttpRequestException ex)
        {
            await JS.InvokeVoidAsync("mostrarToast", $"❌ Error HTTP:\n{ex.Message}", "error");
        }
        finally
        {
            IsSaving = false;
        }
    }









    private string BuildObs(string? detalle, string? nombreProceso, string? numeroExpediente)
    {
        var d = string.IsNullOrWhiteSpace(detalle) ? "Movimiento agendado" : detalle.Trim();
        var np = string.IsNullOrWhiteSpace(nombreProceso) ? null : nombreProceso.Trim();
        var ne = string.IsNullOrWhiteSpace(numeroExpediente) ? null : numeroExpediente.Trim();

        var sb = new System.Text.StringBuilder(d);
        if (np != null || ne != null)
        {
            sb.AppendLine();
            if (np != null) sb.AppendLine($"Proceso: {np}");
            if (ne != null) sb.AppendLine($"Expediente: {ne}");
        }
        return sb.ToString();
    }








    //private async Task CrearAgendaSiCorrespondeAsync(int movimientoId)
    //{
    //    // si apagan el switch y existe agenda, podrías eliminarla aquí
    //    if (!Agendar)
    //    {
    //        if (AgendaExistenteId.HasValue)
    //        {
    //            var okDel = await AgendaService.EliminarAsync(AgendaExistenteId.Value); /*, deleteGoogle: true, usuarioId: Modelo.UsuarioId */
    //            if (okDel)
    //            {
    //                AgendaExistenteId = null;
    //                await JS.InvokeVoidAsync("mostrarToast", "🗑️ Agenda eliminada del movimiento", "success");
    //            }
    //            else
    //            {
    //                await JS.InvokeVoidAsync("mostrarToast", "❌ No se pudo eliminar la agenda", "error");
    //            }
    //        }
    //        return;
    //    }

    //    var inicioLocal = DateTime.SpecifyKind(AgendaFechaHoraLocal, DateTimeKind.Unspecified);
    //    var usuarioId = Modelo.UsuarioId != 0 ? Modelo.UsuarioId : await ObtenerUsuarioId();


    //    if (AgendaExistenteId.HasValue)
    //    {
    //        // 🔄 ACTUALIZAR
    //        var dtoUpd = new ActualizarAgendaDesdeMovimientoDto
    //        {
    //            Titulo = string.IsNullOrWhiteSpace(Modelo.Titulo) ? "Movimiento agendado" : Modelo.Titulo,
    //            FechaInicioLocal = inicioLocal,
    //            DuracionMin = (AgendaDuracionMin > 0 ? AgendaDuracionMin : 60),
    //            TipoAgendamientoId = AgendaTipoAgendamientoId,
    //            Observaciones = AgendaObservaciones,

    //            NotificarApp = AgendaNotificarApp,
    //            NotificarAppUsuarioId = AgendaNotificarAppUsuarioId,
    //            NotificarEmail = AgendaNotificarEmail,
    //            NotificarEmailUsuarioId = AgendaNotificarEmailUsuarioId,
    //            NotificarEmailTo = string.IsNullOrWhiteSpace(AgendaNotificarEmailTo) ? null : AgendaNotificarEmailTo,
    //            RecordatorioMinAntes = AgendaRecordatorioMinAntes,

    //            PersonaId = AgendaPersonaId,
    //            UsuarioId = usuarioId
    //        };

    //        var updated = await AgendaService.ActualizarDesdeMovimientoAsync(AgendaExistenteId.Value, dtoUpd);
    //        if (updated is null)
    //        {
    //            await JS.InvokeVoidAsync("mostrarToast", "❌ No se pudo actualizar la agenda del movimiento.", "error");
    //        }
    //        else
    //        {
    //            await JS.InvokeVoidAsync("mostrarToast", "📅 Agenda actualizada", "success");
    //        }
    //    }
    //    else
    //    {
    //        // ➕ CREAR
    //        var dtoNew = new CrearAgendaDesdeMovimientoDto
    //        {
    //            MovimientoId = movimientoId,
    //            Titulo = string.IsNullOrWhiteSpace(Modelo.Titulo) ? "Movimiento agendado" : Modelo.Titulo,
    //            FechaInicioLocal = inicioLocal,
    //            DuracionMin = (AgendaDuracionMin > 0 ? AgendaDuracionMin : 60),
    //            TipoAgendamientoId = AgendaTipoAgendamientoId,
    //            Observaciones = AgendaObservaciones,

    //            NotificarApp = AgendaNotificarApp,
    //            NotificarAppUsuarioId = AgendaNotificarAppUsuarioId,
    //            NotificarEmail = AgendaNotificarEmail,
    //            NotificarEmailUsuarioId = AgendaNotificarEmailUsuarioId,
    //            NotificarEmailTo = string.IsNullOrWhiteSpace(AgendaNotificarEmailTo) ? null : AgendaNotificarEmailTo,
    //            RecordatorioMinAntes = AgendaRecordatorioMinAntes,

    //            PersonaId = AgendaPersonaId,
    //            UsuarioId = usuarioId
    //        };

    //        var created = await AgendaService.CrearDesdeMovimientoAsync(dtoNew);
    //        if (created is null)
    //        {
    //            await JS.InvokeVoidAsync("mostrarToast", "❌ No se pudo crear la agenda del movimiento.", "error");
    //        }
    //        else
    //        {
    //            AgendaExistenteId = created.Id; // <<-- guardarlo para futuras ediciones
    //            await JS.InvokeVoidAsync("mostrarToast", "📅 Agenda creada para este movimiento", "success");
    //        }
    //    }
    //}







    private async Task CrearAgendaSiCorrespondeAsync(int movimientoId, bool reSyncIfChanged = false)
    {
        var inicioLocal = DateTime.SpecifyKind(AgendaFechaHoraLocal, DateTimeKind.Unspecified);
        var finLocal = DateTime.SpecifyKind(AgendaFechaHoraFin, DateTimeKind.Unspecified);
        var usuarioId = Modelo.UsuarioId != 0 ? Modelo.UsuarioId : await ObtenerUsuarioId();
        var obs = BuildObs(Modelo.Detalle, NombreProceso, NumeroExpediente);
        // 4 casos:
        // A) de ON→OFF: borrar
        if (!Agendar && AgendarOriginal && AgendaExistenteId.HasValue)
        {
            var okDel = await AgendaService.EliminarAsync(AgendaExistenteId.Value);
            if (okDel)
            {
                AgendaExistenteId = null;

                await JS.InvokeVoidAsync("mostrarToast", "🗑️ Agenda eliminada del movimiento", "success");
            }
            else
            {
                await JS.InvokeVoidAsync("mostrarToast", "❌ No se pudo eliminar la agenda", "error");
            }
            return;
        }

        // B) de OFF→OFF: no hacer nada
        if (!Agendar && !AgendarOriginal) return;

        


        // C) ON→ON: actualizar
        if (Agendar && AgendarOriginal && AgendaExistenteId.HasValue)
        {
            var dtoUpd = new ActualizarAgendaDesdeMovimientoDto
            {
                Titulo = string.IsNullOrWhiteSpace(Modelo.Titulo) ? "Movimiento agendado" : Modelo.Titulo,
                FechaInicioLocal = DateTime.SpecifyKind(AgendaFechaHoraLocal, DateTimeKind.Unspecified),
                FechaFin = DateTime.SpecifyKind(AgendaFechaHoraFin, DateTimeKind.Unspecified),
                DuracionMin = (AgendaDuracionMin > 0 ? AgendaDuracionMin : 60),
                TipoAgendamientoId = AgendaTipoAgendamientoId,
                //Observaciones = Observaciones = obs, string.IsNullOrWhiteSpace(Modelo.Detalle) ? "Movimiento agendado" : Modelo.Detalle,

                Observaciones = obs,

                NotificarApp = AgendaNotificarApp,
                NotificarAppUsuarioId = AgendaNotificarAppUsuarioId,
                NotificarEmail = AgendaNotificarEmail,
                NotificarEmailUsuarioId = AgendaNotificarEmailUsuarioId,
                NotificarEmailTo = string.IsNullOrWhiteSpace(AgendaNotificarEmailTo) ? null : AgendaNotificarEmailTo,
                RecordatorioMinAntes = AgendaRecordatorioMinAntes,

                PersonaId = AgendaPersonaId,
                UsuarioId = AgendaUsuarioId,/*(Modelo.UsuarioId != 0 ? Modelo.UsuarioId : await ObtenerUsuarioId()),*/

                // Visibilidad
                EsParaTodos = AgendarEsParaTodos,
                UsuarioIds = AgendarEsParaTodos ? null : AgendarUsuarioIds.Distinct().ToList(),

                // Google (persistir selección)
                GoogleUsuarioId = AgendarUsuarioGoogleId,
                GoogleEmailSnapshot = AgendarGoogleEmail
            };

            // detección de cambios antes del update (vs baseline)
            var hayCambiosAfectanGoogle = CambiosQueAfectanGoogleMov();
            var cambiosTxt = string.Join(", ", ListaCambiosGoogleMov());

            var updated = await AgendaService.ActualizarDesdeMovimientoAsync(AgendaExistenteId.Value, dtoUpd);


            if (updated is null)
            {
                await JS.InvokeVoidAsync("mostrarToast", "❌ No se pudo actualizar la agenda del movimiento.", "error");
            }
            else if (hayCambiosAfectanGoogle)
            {
                await JS.InvokeVoidAsync("mostrarToast", "📅 Agenda actualizada", "success");
            }
            //await JS.InvokeVoidAsync("mostrarToast",
            //    updated is null ? "❌ No se pudo actualizar la agenda del movimiento."
            //                    : "📅 Agenda actualizada",
            //    updated is null ? "error" : "success");

            if (updated is null)
                return;

            // refrescar estado Google local con lo que devolvió el server
            AgendaGoogleRegistrado = updated.GoogleRegistrado;
            AgendaGoogleHtmlLink = updated.GoogleHtmlLink;
            AgendaGoogleLastSyncUtc = updated.GoogleLastSyncUtc?.UtcDateTime;

            AgendarUsuarioGoogleId = updated.GoogleUsuarioId ?? AgendarUsuarioGoogleId;
            AgendarGoogleEmail = updated.GoogleEmailSnapshot ?? AgendarGoogleEmail;

            // ¿ya estaba sincronizado?
            var yaSincronizado = updated.GoogleRegistrado || AgendaGoogleRegistrado;

            // Si estaba sincronizado y hubo cambios que afectan Google ► avisar + resincronizar
            if (yaSincronizado && hayCambiosAfectanGoogle)
            {
                // Si NO está marcada la opción de auto-sincro, pedimos confirmación
                var debeResync = AgendarSincronizarGoogle;
                if (!debeResync)
                {
                    var msg = string.IsNullOrWhiteSpace(cambiosTxt)
                        ? "Este evento ya está sincronizado con Google y tuvo cambios. ¿Querés actualizarlo también en Calendar?"
                        : $"Este evento ya está sincronizado con Google y cambió ({cambiosTxt}). ¿Actualizar también en Calendar?";
                    debeResync = await JS.InvokeAsync<bool>("confirm", msg);
                }

                if (debeResync)
                {
                    var uid = await GetUsuarioGoogleValidoMovAsync();
                    if (uid is not null)
                    {
                        try
                        {
                            await JS.InvokeVoidAsync("mostrarToast", "🔁 Resincronizando con Google…", "info");

                            var sync = await GoogleService.SincronizarAgendaAsync(AgendaExistenteId.Value, uid.Value);

                            // actualizar estado post-sync
                            AgendaGoogleRegistrado = sync.GoogleRegistrado;
                            AgendaGoogleHtmlLink = sync.GoogleHtmlLink;
                            AgendaGoogleLastSyncUtc = sync.GoogleLastSyncUtc?.UtcDateTime;
                            AgendarUsuarioGoogleId = sync.GoogleUsuarioId ?? AgendarUsuarioGoogleId;
                            AgendarGoogleEmail = string.IsNullOrWhiteSpace(sync.GoogleEmailSnapshot) ? AgendarGoogleEmail : sync.GoogleEmailSnapshot;

                            await JS.InvokeVoidAsync("mostrarToast", "♻️ Evento actualizado en Google", "success");
                        }
                        catch (InvalidOperationException ex) when (ex.Message.StartsWith("GoogleAuthConflict"))
                        {
                            await JS.InvokeVoidAsync("mostrarToast", "No se pudo sincronizar con Google: " + ex.Message, "error");
                        }
                    }
                    else
                    {
                        await JS.InvokeVoidAsync("mostrarToast", "Guardado. Falta seleccionar usuario de Google para resincronizar.", "warning");
                    }
                }
                else
                {
                    await JS.InvokeVoidAsync("mostrarToast", "✅ Agenda guardada. Google queda desactualizado hasta que resincronices.", "warning");
                }
            }
            else if (AgendarSincronizarGoogle && !yaSincronizado)
            {
                // Caso: no estaba sincronizado aún y el usuario marcó "sincronizar"
                var uid = await GetUsuarioGoogleValidoMovAsync();
                if (uid is not null)
                {
                    try
                    {
                        var sync = await GoogleService.SincronizarAgendaAsync(AgendaExistenteId.Value, uid.Value);

                        AgendaGoogleRegistrado = sync.GoogleRegistrado;
                        AgendaGoogleHtmlLink = sync.GoogleHtmlLink;
                        AgendaGoogleLastSyncUtc = sync.GoogleLastSyncUtc?.UtcDateTime;
                        AgendarUsuarioGoogleId = sync.GoogleUsuarioId ?? AgendarUsuarioGoogleId;
                        AgendarGoogleEmail = string.IsNullOrWhiteSpace(sync.GoogleEmailSnapshot) ? AgendarGoogleEmail : sync.GoogleEmailSnapshot;

                        await JS.InvokeVoidAsync("mostrarToast", "📤 Evento creado/actualizado en Google", "success");
                    }
                    catch (InvalidOperationException ex) when (ex.Message.StartsWith("GoogleAuthConflict"))
                    {
                        await JS.InvokeVoidAsync("mostrarToast", "No se pudo sincronizar con Google: " + ex.Message, "error");
                    }
                }
            }

            // ✅ baseline al final (estado actual queda como “original”)
            UpdateBaselineFromCurrent();
            return;
        }

        // D) OFF→ON: crear
        if (Agendar && !AgendarOriginal)
        {
            var dtoNew = new CrearAgendaDesdeMovimientoDto
            {
                MovimientoId = movimientoId,
                Titulo = string.IsNullOrWhiteSpace(Modelo.Titulo) ? "Movimiento agendado" : Modelo.Titulo,
                FechaInicioLocal = inicioLocal,
                FechaFin = finLocal,
                DuracionMin = (AgendaDuracionMin > 0 ? AgendaDuracionMin : 60),
                TipoAgendamientoId = AgendaTipoAgendamientoId,
                //Observaciones = AgendaObservaciones,
                Observaciones = obs,/*string.IsNullOrWhiteSpace(Modelo.Detalle) ? "Movimiento agendado" : Modelo.Detalle,*/
                NotificarApp = AgendaNotificarApp,
                NotificarAppUsuarioId = AgendaNotificarAppUsuarioId,
                NotificarEmail = AgendaNotificarEmail,
                NotificarEmailUsuarioId = AgendaNotificarEmailUsuarioId,
                NotificarEmailTo = string.IsNullOrWhiteSpace(AgendaNotificarEmailTo) ? null : AgendaNotificarEmailTo,
                RecordatorioMinAntes = AgendaRecordatorioMinAntes,
                PersonaId = AgendaPersonaId,
                UsuarioId = AgendaUsuarioId,/*usuarioId,*/

                // ✅ Nueva visibilidad
                EsParaTodos = AgendarEsParaTodos,
                UsuarioIds = AgendarEsParaTodos ? null : AgendarUsuarioIds.Distinct().ToList(),

                GoogleUsuarioId = AgendarUsuarioGoogleId,
                GoogleEmailSnapshot = AgendarGoogleEmail
            };

            var created = await AgendaService.CrearDesdeMovimientoAsync(dtoNew);
            if (created is null)
            {
                await JS.InvokeVoidAsync("mostrarToast", "❌ No se pudo crear la agenda del movimiento.", "error");
            }
            else
            {
                AgendaExistenteId = created.Id;
                // refrescar estado google
                AgendaGoogleRegistrado = created.GoogleRegistrado;
                AgendaGoogleHtmlLink = created.GoogleHtmlLink;
                AgendaGoogleLastSyncUtc = created.GoogleLastSyncUtc?.UtcDateTime;

                AgendarUsuarioGoogleId = created.GoogleUsuarioId ?? AgendarUsuarioGoogleId;
                AgendarGoogleEmail = created.GoogleEmailSnapshot ?? AgendarGoogleEmail;
                await JS.InvokeVoidAsync("mostrarToast", "📅 Agenda creada para este movimiento", "success");

                // 📤 Google opcional
                if (AgendarSincronizarGoogle)
                {
                    var uid = await GetUsuarioGoogleValidoMovAsync();
                    if (uid is not null)
                    {
                        try
                        {
                            var sync = await GoogleService.SincronizarAgendaAsync(created.Id, uid.Value);
                            await JS.InvokeVoidAsync("mostrarToast", "📤 Evento creado en Google", "success");
                        }
                        catch (InvalidOperationException ex) when (ex.Message.StartsWith("GoogleAuthConflict"))
                        {
                            await JS.InvokeVoidAsync("mostrarToast", "No se pudo sincronizar con Google: " + ex.Message, "error");
                        }
                    }
                }
                return;




            }
        }
    }









    //protected void EliminarArchivo(ArchivoTemporalDto archivo)
    //{
    //    ArchivosTemporales.Remove(archivo);
    //    MovimientoTemporalService.Guardar(new MovimientoTemporalDto
    //    {
    //        Titulo = Modelo.Titulo,
    //        Detalle = Modelo.Detalle,
    //        Fecha = Modelo.Fecha,
    //        Archivos = ArchivosTemporales
    //    });
    //}

    protected void EliminarArchivo(ArchivoTemporalDto archivo)
    {
        ArchivosTemporales.Remove(archivo);

        // 🔹 Si el archivo existe en BD, lo marcamos para eliminar
        if (archivo.Id.HasValue && archivo.Id.Value > 0)
        {
            ArchivosMarcadosParaEliminar.Add(archivo.Id.Value);
        }

        MovimientoTemporalService.Guardar(new MovimientoTemporalDto
        {
            Titulo = Modelo.Titulo,
            Detalle = Modelo.Detalle,
            Fecha = Modelo.Fecha,
            Archivos = ArchivosTemporales
        });
    }



    //protected string ObtenerUrlBase64(ArchivoTemporalDto archivo)
    //{
    //    var base64 = Convert.ToBase64String(archivo.Contenido);
    //    return $"data:{archivo.TipoMime};base64,{base64}";
    //}

    protected async Task<string> ObtenerUrlBase64Async(ArchivoTemporalDto archivo)
    {
        if (!archivo.ContenidoCargado && archivo.Id.HasValue)
        {
            var contenido = await ArchivoService.DescargarContenidoAsync(archivo.Id.Value);
            if (contenido is not null)
            {
                archivo.Contenido = contenido;
            }
        }

        if (archivo.ContenidoCargado)
        {
            var base64 = Convert.ToBase64String(archivo.Contenido);
            return $"data:{archivo.TipoMime};base64,{base64}";
        }

        return string.Empty;
    }

    //protected string ObtenerUrlDescarga(ArchivoTemporalDto archivo)
    //{
    //    return archivo.Id.HasValue
    //        ? $"api/archivomovimiento/{archivo.Id}/contenido"
    //        : $"data:{archivo.TipoMime};base64,{Convert.ToBase64String(archivo.Contenido)}";
    //}



    protected string ObtenerUrlDescarga(ArchivoTemporalDto archivo)
    {
        return archivo.Id.HasValue
            ? $"api/archivomovimiento/{archivo.Id}/contenido"
            : $"data:{archivo.TipoMime};base64,{Convert.ToBase64String(archivo.Contenido)}";
    }

    protected async Task VerArchivo(ArchivoTemporalDto archivo)
    {
        if (!archivo.ContenidoCargado && archivo.Id.HasValue)
        {
            var contenido = await ArchivoService.DescargarContenidoAsync(archivo.Id.Value);
            if (contenido is not null)
            {
                archivo.Contenido = contenido;
            }
        }

        if (archivo.ContenidoCargado)
        {
            archivo.DataUrlBase64 = $"data:{archivo.TipoMime};base64,{Convert.ToBase64String(archivo.Contenido)}";
            ArchivoSeleccionadoParaVer = archivo;
            MostrarModalArchivo = true;
        }
    }










    protected void Volver()
    {
        var clave = Id?.ToString() ?? "temporal";
        var info = EditorInfoService.Obtener(clave);

        EditorInfoService.Limpiar(clave);

        MovimientoTemporalService.Limpiar();

        if (Tipo != null && ProcesoId.HasValue)
            Nav.NavigateTo($"/movimientos/{Tipo}/{ProcesoId.Value}");
        else
            Nav.NavigateTo("/gestiones");

      

    }




    private async Task Toast(string msg, string tipo) =>
    await JS.InvokeVoidAsync("mostrarToast", msg, tipo);



}
