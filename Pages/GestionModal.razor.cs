using IurixBlazor.Forms;
using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

public class GestionModalBase : ComponentBase
{
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    [Inject] protected GestionService GestionService { get; set; } = default!;
    [Inject] protected UsuarioService UsuarioService { get; set; } = default!;
    [Inject] protected TipoEstadoProcesoService EstadoService { get; set; } = default!;

    [Inject] protected JurisdiccionService JurisdiccionService { get; set; } = default!;
    [Inject] protected CircunscripcionService CircunscripcionService { get; set; } = default!;
    [Inject] protected JuzgadoService JuzgadoService { get; set; } = default!;
    [Inject] protected SecretariaService SecretariaService { get; set; } = default!;


    [Inject] protected DomicilioService DomicilioService { get; set; } = default!;
    [Inject] protected GrupoGestionService GrupoGestionService { get; set; } = default!;
    [Inject] protected TipoProcesoService TipoProcesoService { get; set; } = default!;

    [Inject] protected PresupuestoService PresupuestoService { get; set; } = default!;
    [Inject] protected PersonaService PersonaService { get; set; } = default!;


    public string ModalId { get; } = "gestionModal";

    protected int Tab { get; set; } = 0;

    protected bool Cargando { get; set; }
    protected bool SoloLectura { get; set; }

    protected CrearGestionDto? Gestion { get; set; }
    protected List<UsuarioDto> Responsables = new();
    protected List<TipoEstadoProcesoDto> EstadosJudiciales = new();
    protected List<TipoEstadoProcesoDto> EstadosExtrajudiciales = new();
    protected List<JurisdiccionDto> Jurisdicciones = new();
    protected List<CircunscripcionDto> Circunscripciones = new();
    protected List<JuzgadoDto> Juzgados = new();
    protected List<SecretariaDto> Secretarias = new();

    protected List<DomicilioDto> DomiciliosConstituidos { get; set; } = new();
    protected List<DomicilioDto> DomiciliosElectronicos { get; set; } = new();

    protected List<TipoProcesoDto> TiposProceso = new();

    protected List<PresupuestoDto> PresupuestosFiltrados = new();

    protected List<PersonaDto> Personas = new();

    protected List<GrupoGestionDto> Grupos = new();



    protected GestionFormModel Form { get; set; } = new();

    // para updates correctos:
    protected int? _procesoJudicialId;
    protected int? _procesoExtrajudicialId;
    protected DateTime? _fechaInicioExistente;




    private bool _suspendCascade;

    //protected int? _jurisdiccionSeleccionada;
    //protected int? JurisdiccionSeleccionada
    //{
    //    get => _jurisdiccionSeleccionada;
    //    set
    //    {
    //        if (_jurisdiccionSeleccionada == value) return;
    //        _jurisdiccionSeleccionada = value;
    //        if (_suspendCascade || value is null) return;
    //        _ = CargarCircunscripcionesAsync();
    //    }
    //}

    //protected int? _circunscripcionSeleccionada;
    //protected int? CircunscripcionSeleccionada
    //{
    //    get => _circunscripcionSeleccionada;
    //    set
    //    {
    //        if (_circunscripcionSeleccionada == value) return;
    //        _circunscripcionSeleccionada = value;
    //        if (_suspendCascade || value is null) return;
    //        _ = CargarJuzgadosAsync();
    //    }
    //}

    //protected int? _juzgadoSeleccionado;
    //protected int? JuzgadoSeleccionado
    //{
    //    get => _juzgadoSeleccionado;
    //    set
    //    {
    //        if (_juzgadoSeleccionado == value) return;
    //        _juzgadoSeleccionado = value;
    //        if (_suspendCascade || value is null) return;
    //        _ = CargarSecretariasAsync();
    //    }
    //}


    //protected int? JurisdiccionSeleccionada
    //{
    //    get => Form.PJ_JurisdiccionId;
    //    set
    //    {
    //        if (Form.PJ_JurisdiccionId == value) return;
    //        Form.PJ_JurisdiccionId = value;

    //        if (_suspendCascade || value is null) return;
    //        Form.PJ_CircunscripcionId = null;
    //        Form.PJ_JuzgadoId = null;
    //        Form.PJ_SecretariaId = null;
    //        Circunscripciones.Clear(); Juzgados.Clear(); Secretarias.Clear();
    //        _ = CargarCircunscripcionesAsync();
    //    }
    //}

    protected int? JurisdiccionSeleccionada
    {
        get => Form.PJ_JurisdiccionId;
        set
        {
            if (Form.PJ_JurisdiccionId == value) return;

            // Si estamos reconstruyendo (carga inicial), no disparamos cascada
            if (_suspendCascade)
            {
                Form.PJ_JurisdiccionId = value;
                return;
            }

            Form.PJ_JurisdiccionId = value;

            // Cambió jurisdicción desde la UI → reseteo hijos
            Form.PJ_CircunscripcionId = null;
            Form.PJ_JuzgadoId = null;
            Form.PJ_SecretariaId = null;
            Circunscripciones.Clear();
            Juzgados.Clear();
            Secretarias.Clear();

            if (value is null) return;

            _ = CargarCircunscripcionesAsync();
        }
    }
    //protected int? CircunscripcionSeleccionada
    //{
    //    get => Form.PJ_CircunscripcionId;
    //    set
    //    {
    //        if (Form.PJ_CircunscripcionId == value) return;
    //        Form.PJ_CircunscripcionId = value;

    //        if (_suspendCascade || value is null) return;
    //        Form.PJ_JuzgadoId = null;
    //        Form.PJ_SecretariaId = null;
    //        Juzgados.Clear(); Secretarias.Clear();
    //        _ = CargarJuzgadosAsync();
    //    }
    //}
    //protected int? JuzgadoSeleccionado
    //{
    //    get => Form.PJ_JuzgadoId;
    //    set
    //    {
    //        if (Form.PJ_JuzgadoId == value) return;
    //        Form.PJ_JuzgadoId = value;

    //        if (_suspendCascade || value is null) return;
    //        Form.PJ_SecretariaId = null;
    //        Secretarias.Clear();
    //        _ = CargarSecretariasAsync();
    //    }
    //}

    protected int? CircunscripcionSeleccionada
    {
        get => Form.PJ_CircunscripcionId;
        set
        {
            if (Form.PJ_CircunscripcionId == value) return;

            if (_suspendCascade)
            {
                Form.PJ_CircunscripcionId = value;
                return;
            }

            Form.PJ_CircunscripcionId = value;

            Form.PJ_JuzgadoId = null;
            Form.PJ_SecretariaId = null;
            Juzgados.Clear();
            Secretarias.Clear();

            if (value is null) return;

            _ = CargarJuzgadosAsync();
        }
    }

    protected int? JuzgadoSeleccionado
    {
        get => Form.PJ_JuzgadoId;
        set
        {
            if (Form.PJ_JuzgadoId == value) return;

            if (_suspendCascade)
            {
                Form.PJ_JuzgadoId = value;
                return;
            }

            Form.PJ_JuzgadoId = value;

            Form.PJ_SecretariaId = null;
            Secretarias.Clear();

            if (value is null) return;

            _ = CargarSecretariasAsync();
        }
    }



    protected bool PresupuestosDeshabilitado { get; set; } = true;

    //protected string TipoGestion
    //{
    //    get => Gestion?.TipoGestion ?? "Judicial";
    //    set
    //    {
    //        if (Gestion is null) return;
    //        Gestion.TipoGestion = value;
    //        if (value == "Judicial")
    //        {
    //            Gestion.ProcesoJudicial ??= new CrearProcesoJudicialDto();
    //            Gestion.ProcesoExtrajudicial = null;
    //        }
    //        else
    //        {
    //            Gestion.ProcesoExtrajudicial ??= new CrearProcesoExtrajudicialDto();
    //            Gestion.ProcesoJudicial = null;
    //        }
    //    }
    //}


    //protected int? _personaSeleccionada;
    //protected int? PersonaSeleccionada
    //{
    //    get => _personaSeleccionada;
    //    set
    //    {
    //        Console.WriteLine($"[DEBUG] Cambio persona: {value}");
    //        if (_personaSeleccionada != value)
    //        {
    //            _personaSeleccionada = value;
    //            _ = CargarPresupuestosPorPersonaAsync();
    //        }
    //    }
    //}


    protected int? PersonaSeleccionada
    {
        get => Form.PersonaId;
        set
        {
            if (Form.PersonaId == value) return;
            Form.PersonaId = value;
            Form.PresupuestoId = null;
            _ = CargarPresupuestosPorPersonaAsync();
        }
    }

    //protected string? TipoGestion
    //{
    //    get => Form.TipoGestion;
    //    set
    //    {
    //        if (Form.TipoGestion == value) return;
    //        Form.TipoGestion = value;

    //        if (value == "Judicial")
    //        {
    //            // limpiar extrajudicial
    //            Form.EX_Tipo = null; Form.EX_Materia = null; Form.EX_EstadoId = null;
    //            // reset cascadas judiciales
    //            Form.PJ_JurisdiccionId = null; Form.PJ_CircunscripcionId = null;
    //            Form.PJ_JuzgadoId = null; Form.PJ_SecretariaId = null;
    //            Circunscripciones.Clear(); Juzgados.Clear(); Secretarias.Clear();
    //        }
    //        else if (value == "Extrajudicial")
    //        {
    //            // limpiar judicial
    //            Form.PJ_Caratula = null; Form.PJ_NumeroExpediente = null;
    //            Form.PJ_TipoProcesoId = null; Form.PJ_DomicilioConstituidoId = null;
    //            Form.PJ_DomicilioElectronicoId = null; Form.PJ_JurisdiccionId = null;
    //            Form.PJ_CircunscripcionId = null; Form.PJ_JuzgadoId = null;
    //            Form.PJ_SecretariaId = null; Form.PJ_EstadoId = null;
    //            Circunscripciones.Clear(); Juzgados.Clear(); Secretarias.Clear();
    //        }
    //    }
    //}


    // === Tipo de gestión (usa Form) ===
    protected string? TipoGestion
    {
        get => Form.TipoGestion;
        set
        {
            if (Form.TipoGestion == value) return;
            Form.TipoGestion = value;

            if (value == "Judicial")
            {
                // limpiar extrajudicial
                Form.EX_Tipo = null;
                Form.EX_Materia = null;
                Form.EX_EstadoId = null;

                // reset dependencias judiciales
                Form.PJ_Caratula = Form.PJ_Caratula; // (no la toco)
                Form.PJ_NumeroExpediente = Form.PJ_NumeroExpediente; // (no la toco)
                Form.PJ_JurisdiccionId = null;
                Form.PJ_CircunscripcionId = null;
                Form.PJ_JuzgadoId = null;
                Form.PJ_SecretariaId = null;
                Form.PJ_NumeroCertificadoDeuda = null;
                Form.PJ_MontoDemanda = null;
                Form.PJ_FechaEmisionCertificado = null;
                Form.PJ_FechaLiquidacion = null;

                Circunscripciones.Clear();
                Juzgados.Clear();
                Secretarias.Clear();
            }
            else if (value == "Extrajudicial")
            {
                // limpiar judicial
                Form.PJ_Caratula = null;
                Form.PJ_NumeroExpediente = null;
                Form.PJ_TipoProcesoId = null;
                Form.PJ_DomicilioConstituidoId = null;
                Form.PJ_DomicilioElectronicoId = null;
                Form.PJ_JurisdiccionId = null;
                Form.PJ_CircunscripcionId = null;
                Form.PJ_JuzgadoId = null;
                Form.PJ_SecretariaId = null;
                Form.PJ_EstadoId = null;
                Form.PJ_NumeroCertificadoDeuda = null;
                Form.PJ_MontoDemanda = null;
                Form.PJ_FechaEmisionCertificado = null;
                Form.PJ_FechaLiquidacion = null;


                Circunscripciones.Clear();
                Juzgados.Clear();
                Secretarias.Clear();
            }
        }
    }




    protected int _gestionId;


    private int? _apremioTipoProcesoId;


    //protected int? ProcesoId =>
    //    Gestion?.TipoGestion == "Judicial"
    //        ? Gestion?.ProcesoJudicial?.Id
    //        : Gestion?.ProcesoExtrajudicial?.Id;



    //protected string ResponsableNombre =>
    //    Responsables.FirstOrDefault(r => r.Id == Form?.ResponsableId)?.Nombre ?? "—";



    //protected string PersonaNombre => Form?.PersonaId.HasValue == true ? "Cliente vinculado" : "—";

    protected int? ProcesoId => Form?.TipoGestion == "Judicial" ? _procesoJudicialId : _procesoExtrajudicialId;

protected string ResponsableNombre =>
    (Form?.ResponsableId is int rid ? Responsables.FirstOrDefault(r => r.Id == rid)?.Nombre : null) ?? "—";

protected string PersonaNombre => Form?.PersonaId is int ? "Cliente vinculado" : "—";



    public async Task ShowAsync(int gestionId, bool soloLectura = false)
    {
        SoloLectura = soloLectura;
        Cargando = true;
        Tab = 0;
        _gestionId = gestionId;

        // 1) Cargar listas base en paralelo
        var tResp = UsuarioService.ObtenerUsuariosAsync();
        var tEstJ = EstadoService.ObtenerPorTipoAsync("Judicial");
        var tEstX = EstadoService.ObtenerPorTipoAsync("Extrajudicial");
        var tJur = JurisdiccionService.ObtenerTodosAsync();
        var tTipProc = TipoProcesoService.ObtenerTodosAsync();
        var tDomC = DomicilioService.ObtenerPorTipoAsync(TipoDomicilio.Constituido);
        var tDomE = DomicilioService.ObtenerPorTipoAsync(TipoDomicilio.Electronico);
        var tGrupos = GrupoGestionService.ObtenerTodosAsync();
        var tPers = PersonaService.ObtenerPersonasAsync();


        //  Buscar el tipo "APREMIO" en la lista que acabamos de traer
        var tipoApremio = TiposProceso
            .FirstOrDefault(t =>
                string.Equals(t.Nombre, "APREMIO", StringComparison.OrdinalIgnoreCase));

        _apremioTipoProcesoId = tipoApremio?.Id;

        await Task.WhenAll(tResp, tEstJ, tEstX, tJur, tTipProc, tDomC, tDomE, tGrupos, tPers);

        Responsables = tResp.Result;
        EstadosJudiciales = tEstJ.Result;
        EstadosExtrajudiciales = tEstX.Result;
        Jurisdicciones = tJur.Result;
        TiposProceso = tTipProc.Result;
        DomiciliosConstituidos = tDomC.Result;
        DomiciliosElectronicos = tDomE.Result;
        Grupos = tGrupos.Result;
        Personas = tPers.Result;

        // 2) Traer la gestión
        var dto = await GestionService.ObtenerPorIdAsync(gestionId);
        if (dto is null)
        {
            Gestion = null;
            Cargando = false;
            StateHasChanged();
            await JS.InvokeVoidAsync("bsModal.show", $"#{ModalId}");
            return;
        }

        // 3) Mapear a CrearGestionDto
        //Gestion = new CrearGestionDto
        //{
        //    TipoGestion = dto.TipoGestion,
        //    ResponsableId = dto.ResponsableId,
        //    GrupoId = dto.GrupoId,
        //    PersonaId = dto.PersonaId,
        //    PresupuestoId = dto.PresupuestoId ?? 0,
        //    FechaInicio = dto.FechaInicio,

        //    ProcesoJudicial = dto.ProcesoJudicial != null ? new CrearProcesoJudicialDto
        //    {
        //        Id = dto.ProcesoJudicial.Id,
        //        Caratula = dto.ProcesoJudicial.Caratula,
        //        NumeroExpediente = dto.ProcesoJudicial.NumeroExpediente,
        //        TipoProcesoId = dto.ProcesoJudicial.TipoProcesoId,
        //        SecretariaId = dto.ProcesoJudicial.SecretariaId,
        //        DomicilioConstituidoId = dto.ProcesoJudicial.DomicilioConstituidoId,
        //        DomicilioElectronicoId = dto.ProcesoJudicial.DomicilioElectronicoId,
        //        EstadoId = dto.ProcesoJudicial.EstadoId
        //    } : null,

        //    ProcesoExtrajudicial = dto.ProcesoExtrajudicial != null ? new CrearProcesoExtrajudicialDto
        //    {
        //        Id = dto.ProcesoExtrajudicial.Id,
        //        Tipo = dto.ProcesoExtrajudicial.Tipo,
        //        Materia = dto.ProcesoExtrajudicial.Materia,
        //        EstadoId = dto.ProcesoExtrajudicial.EstadoId
        //    } : null
        //};

        //// 4) Sincronizar wrapper para que los IFs y binds funcionen
        //TipoGestion = dto.TipoGestion; // dispara creación de sub-DTO si hiciera falta

        //// 5) Reconstruir cascada judicial DESPUÉS de tener Gestion y listas
        //if (TipoGestion == "Judicial" && Gestion.ProcesoJudicial?.SecretariaId is int secId && secId > 0)
        //{
        //    var secretaria = await SecretariaService.ObtenerPorIdAsync(secId);
        //    if (secretaria != null)
        //    {
        //        var juzgado = await JuzgadoService.ObtenerPorIdAsync(secretaria.JuzgadoId);
        //        if (juzgado != null)
        //        {
        //            var circ = await CircunscripcionService.ObtenerPorIdAsync(juzgado.CircunscripcionId);
        //            if (circ != null)
        //            {
        //                _suspendCascade = true;

        //                JurisdiccionSeleccionada = circ.JurisdiccionId;
        //                Circunscripciones = await CircunscripcionService.ObtenerPorJurisdiccionAsync(circ.JurisdiccionId);
        //                CircunscripcionSeleccionada = circ.Id;

        //                Juzgados = await JuzgadoService.ObtenerPorCircunscripcionAsync(circ.Id);
        //                JuzgadoSeleccionado = juzgado.Id;

        //                Secretarias = await SecretariaService.ObtenerPorJuzgadoAsync(juzgado.Id);

        //                _suspendCascade = false;

        //                // asegurar el valor actualmente seleccionado
        //                Gestion.ProcesoJudicial.SecretariaId = secId;
        //            }
        //        }
        //    }
        //}

        //// 6) Persona / Presupuestos
        //PersonaSeleccionada = dto.PersonaId;
        //await CargarPresupuestosPorPersonaAsync(); // setea PresupuestosFiltrados + PresupuestosDeshabilitado


        // Map comunes
        Form = new GestionFormModel
        {
            TipoGestion = dto.TipoGestion,
            ResponsableId = dto.ResponsableId,
            GrupoId = dto.GrupoId,
            PersonaId = dto.PersonaId,
            PresupuestoId = dto.PresupuestoId,   // ahora int? en el VM
        };

        // Guardar info para update
        _procesoJudicialId = dto.ProcesoJudicial?.Id;
        _procesoExtrajudicialId = dto.ProcesoExtrajudicial?.Id;
        _fechaInicioExistente = dto.FechaInicio;

        // Judicial
        if (dto.TipoGestion == "Judicial" && dto.ProcesoJudicial is not null)
        {
            Form.PJ_Caratula = dto.ProcesoJudicial.Caratula;
            Form.PJ_NumeroExpediente = dto.ProcesoJudicial.NumeroExpediente;
            Form.PJ_TipoProcesoId = dto.ProcesoJudicial.TipoProcesoId;
            Form.PJ_DomicilioConstituidoId = dto.ProcesoJudicial.DomicilioConstituidoId;
            Form.PJ_DomicilioElectronicoId = dto.ProcesoJudicial.DomicilioElectronicoId;
            Form.PJ_JurisdiccionId = dto.ProcesoJudicial.JurisdiccionId;
            Form.PJ_CircunscripcionId = dto.ProcesoJudicial.CircunscripcionId;
            Form.PJ_JuzgadoId = dto.ProcesoJudicial.JuzgadoId;
            Form.PJ_SecretariaId = dto.ProcesoJudicial.SecretariaId;
            Form.PJ_EstadoId = dto.ProcesoJudicial.EstadoId;

            // reconstruir cascada partiendo de Secretaría
            //await ReconstruirJerarquiaJudicialAsync(dto.ProcesoJudicial.SecretariaId);
        }

        // Extrajudicial
        if (dto.TipoGestion == "Extrajudicial" && dto.ProcesoExtrajudicial is not null)
        {
            Form.EX_Tipo = dto.ProcesoExtrajudicial.Tipo;
            Form.EX_Materia = dto.ProcesoExtrajudicial.Materia;
            Form.EX_EstadoId = dto.ProcesoExtrajudicial.EstadoId; // int? (opcional)
        }

        // Persona / Presupuestos
        PersonaSeleccionada = Form.PersonaId;   // usa wrapper
        await CargarPresupuestosPorPersonaAsync();
        // 7) Abrir modal
        Cargando = false;
        StateHasChanged();
        await JS.InvokeVoidAsync("bsModal.show", $"#{ModalId}");
    }






    private async Task ReconstruirJerarquiaJudicialAsync(int secretariaId)
    {
        var secretaria = await SecretariaService.ObtenerPorIdAsync(secretariaId);
        if (secretaria is null) return;

        var juzgado = await JuzgadoService.ObtenerPorIdAsync(secretaria.JuzgadoId);
        if (juzgado is null) return;

        var circ = await CircunscripcionService.ObtenerPorIdAsync(juzgado.CircunscripcionId);
        if (circ is null) return;

        _suspendCascade = true;

        Form.PJ_JurisdiccionId = circ.JurisdiccionId;
        Circunscripciones = await CircunscripcionService.ObtenerPorJurisdiccionAsync(circ.JurisdiccionId);
        Form.PJ_CircunscripcionId = circ.Id;

        Juzgados = await JuzgadoService.ObtenerPorCircunscripcionAsync(circ.Id);
        Form.PJ_JuzgadoId = juzgado.Id;

        Secretarias = await SecretariaService.ObtenerPorJuzgadoAsync(juzgado.Id);
        Form.PJ_SecretariaId = secretariaId;

        _suspendCascade = false;
    }








    //protected async Task CargarPresupuestosPorPersonaAsync()
    //{
    //    Console.WriteLine($"[DEBUG] Persona seleccionada: {PersonaSeleccionada}");

    //    if (PersonaSeleccionada.HasValue)
    //    {
    //        Gestion.PersonaId = PersonaSeleccionada;

    //        PresupuestosFiltrados = await PresupuestoService.ObtenerPorPersonaAsync(PersonaSeleccionada.Value);
    //        Console.WriteLine($"[DEBUG] Presupuestos encontrados: {PresupuestosFiltrados.Count}");

    //        PresupuestosDeshabilitado = !PresupuestosFiltrados.Any();
    //        if (PresupuestosDeshabilitado)
    //            Gestion.PresupuestoId = null;
    //    }
    //    else
    //    {
    //        Gestion.PersonaId = null;
    //        PresupuestosFiltrados.Clear();
    //        PresupuestosDeshabilitado = true;
    //        Gestion.PresupuestoId = null;
    //    }

    //    StateHasChanged();
    //}

    protected async Task CargarPresupuestosPorPersonaAsync()
    {
        if (Form.PersonaId.HasValue)
        {
            PresupuestosFiltrados = await PresupuestoService.ObtenerPorPersonaAsync(Form.PersonaId.Value);
            PresupuestosDeshabilitado = PresupuestosFiltrados.Count == 0;

            if (!Form.PresupuestoId.HasValue ||
                !PresupuestosFiltrados.Any(p => p.Id == Form.PresupuestoId.Value))
            {
                Form.PresupuestoId = null;
            }
        }
        else
        {
            Form.PresupuestoId = null;
            PresupuestosFiltrados.Clear();
            PresupuestosDeshabilitado = true;
        }
        StateHasChanged();
    }







    //protected async Task CargarCircunscripcionesAsync()
    //{
    //    Circunscripciones = new();
    //    Juzgados = new();
    //    Secretarias = new();
    //    if (JurisdiccionSeleccionada is int jid && jid > 0)
    //        Circunscripciones = await CircunscripcionService.ObtenerPorJurisdiccionAsync(jid);
    //    Gestion!.ProcesoJudicial!.SecretariaId = 0;
    //    StateHasChanged();
    //}

    //protected async Task CargarJuzgadosAsync()
    //{
    //    Juzgados = new();
    //    Secretarias = new();
    //    if (CircunscripcionSeleccionada is int cid && cid > 0)
    //        Juzgados = await JuzgadoService.ObtenerPorCircunscripcionAsync(cid);
    //    Gestion!.ProcesoJudicial!.SecretariaId = 0;
    //    StateHasChanged();
    //}

    //protected async Task CargarSecretariasAsync()
    //{
    //    Secretarias = new();
    //    if (JuzgadoSeleccionado is int jid && jid > 0)
    //        Secretarias = await SecretariaService.ObtenerPorJuzgadoAsync(jid);
    //    Gestion!.ProcesoJudicial!.SecretariaId = 0;
    //    StateHasChanged();
    //}

    protected async Task CargarCircunscripcionesAsync()
    {
        if (!JurisdiccionSeleccionada.HasValue) return;
        Circunscripciones = await CircunscripcionService.ObtenerPorJurisdiccionAsync(JurisdiccionSeleccionada.Value);

        if (Circunscripciones.Count == 1)
        {
            _suspendCascade = true;
            Form.PJ_CircunscripcionId = Circunscripciones[0].Id;
            _suspendCascade = false;
            await CargarJuzgadosAsync();
        }
        StateHasChanged();
    }

    protected async Task CargarJuzgadosAsync()
    {
        if (!CircunscripcionSeleccionada.HasValue) return;
        Juzgados = await JuzgadoService.ObtenerPorCircunscripcionAsync(CircunscripcionSeleccionada.Value);

        if (Juzgados.Count == 1)
        {
            _suspendCascade = true;
            Form.PJ_JuzgadoId = Juzgados[0].Id;
            _suspendCascade = false;
            await CargarSecretariasAsync();
        }
        StateHasChanged();
    }

    protected async Task CargarSecretariasAsync()
    {
        if (!JuzgadoSeleccionado.HasValue) return;
        Secretarias = await SecretariaService.ObtenerPorJuzgadoAsync(JuzgadoSeleccionado.Value);

        if (Secretarias.Count == 1)
        {
            _suspendCascade = true;
            Form.PJ_SecretariaId = Secretarias[0].Id;
            _suspendCascade = false;
        }
        StateHasChanged();
    }














    public async Task Close()
    {
        await JS.InvokeVoidAsync("bsModal.hide", $"#{ModalId}");
    }

    [Parameter] public EventCallback OnSaved { get; set; }





    //protected async Task Guardar()
    //{
    //    if (Gestion is null || SoloLectura) return;

    //    // Normalización similar a la página:
    //    // (si usás selección de persona/presupuesto dentro del modal, setear también PersonaId/PresupuestoId)
    //    Gestion.GrupoId = Gestion.GrupoId == 0 ? null : Gestion.GrupoId;

    //    if (Gestion.FechaInicio == default)
    //        Gestion.FechaInicio = DateTime.Now;

    //    // Enviar SOLO el proceso correspondiente
    //    if (Gestion.TipoGestion == "Judicial")
    //        Gestion.ProcesoExtrajudicial = null;
    //    else if (Gestion.TipoGestion == "Extrajudicial")
    //        Gestion.ProcesoJudicial = null;

    //    // ✅ acá usamos el id guardado:
    //    if (_gestionId > 0)
    //        await GestionService.ActualizarAsync(_gestionId, Gestion);
    //    else
    //    {
    //        // El modal está pensado para edición; si querés permitir alta desde modal:
    //        // var creada = await GestionService.CrearAsync(Gestion);
    //        // _gestionId = creada.Id; // si tu API devuelve el id
    //    }

    //    if (OnSaved.HasDelegate) await OnSaved.InvokeAsync();
    //    await Close();
    //}


    protected async Task Guardar()
    {
        if (SoloLectura) return;

        var fechaInicio = _fechaInicioExistente ?? DateTime.Now;
        var dto = new CrearGestionDto
        {
            TipoGestion = Form.TipoGestion!,
            ResponsableId = Form.ResponsableId!.Value,
            PersonaId = Form.PersonaId!.Value,
            GrupoId = Form.GrupoId, // null si no hay
            PresupuestoId = PresupuestosDeshabilitado ? (int?)null : Form.PresupuestoId,
            FechaInicio = fechaInicio,
            ProcesoJudicial = Form.TipoGestion == "Judicial" ? new CrearProcesoJudicialDto
            {
                Id = _procesoJudicialId,
                Caratula = Form.PJ_Caratula!,
                NumeroExpediente = Form.PJ_NumeroExpediente!,
                TipoProcesoId = Form.PJ_TipoProcesoId!.Value,
                DomicilioConstituidoId = Form.PJ_DomicilioConstituidoId!.Value,
                DomicilioElectronicoId = Form.PJ_DomicilioElectronicoId!.Value,
                
                EstadoId = Form.PJ_EstadoId!.Value,

                  SecretariaId = Form.PJ_SecretariaId,// null si no hay
                //  NUEVOS
                JurisdiccionId = Form.PJ_JurisdiccionId,// null si no hay
                CircunscripcionId = Form.PJ_CircunscripcionId,// null si no hay
                JuzgadoId = Form.PJ_JuzgadoId
            } : null,
            ProcesoExtrajudicial = Form.TipoGestion == "Extrajudicial" ? new CrearProcesoExtrajudicialDto
            {
                Id = _procesoExtrajudicialId,
                Tipo = Form.EX_Tipo!,
                Materia = Form.EX_Materia!,
                EstadoId = Form.EX_EstadoId // int? opcional
            } : null
        };

        await GestionService.ActualizarAsync(_gestionId, dto);

        if (OnSaved.HasDelegate) await OnSaved.InvokeAsync();
        await Close();
    }

}
