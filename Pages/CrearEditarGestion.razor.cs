using IurixBlazor.Forms;
using IurixBlazor.Pages;
using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class CrearEditarGestionBase : ComponentBase
{
    [Parameter] public int? Id { get; set; }

    [Inject] protected GestionService GestionService { get; set; } = default!;
    [Inject] protected UsuarioService UsuarioService { get; set; } = default!;
    [Inject] protected PresupuestoService PresupuestoService { get; set; } = default!;
    [Inject] protected PersonaService PersonaService { get; set; } = default!;
    [Inject] protected DomicilioService DomicilioService { get; set; } = default!;
    [Inject] protected GrupoGestionService GrupoGestionService { get; set; } = default!;
    [Inject] protected TipoProcesoService TipoProcesoService { get; set; } = default!;
    [Inject] protected SecretariaService SecretariaService { get; set; } = default!;
    [Inject] protected JuzgadoService JuzgadoService { get; set; } = default!;
    [Inject] protected JurisdiccionService JurisdiccionService { get; set; } = default!;
    [Inject] protected CircunscripcionService CircunscripcionService { get; set; } = default!;
    [Inject] protected TipoEstadoProcesoService EstadoService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected CrearGestionDto Gestion { get; set; } = new();
    protected List<UsuarioDto> Responsables = new();
    //protected List<PresupuestoDto> Presupuestos = new();
    protected List<PresupuestoDto> PresupuestosFiltrados = new();

    protected List<PersonaDto> Personas = new();
    
    protected List<GrupoGestionDto> Grupos = new();
    protected List<TipoProcesoDto> TiposProceso = new();
    
    protected List<TipoEstadoProcesoDto> EstadosJudiciales = new();
    protected List<TipoEstadoProcesoDto> EstadosExtrajudiciales = new();
    protected List<DomicilioDto> DomiciliosConstituidos { get; set; } = new();
    protected List<DomicilioDto> DomiciliosElectronicos { get; set; } = new();


    protected List<JurisdiccionDto> Jurisdicciones = new();
    protected List<CircunscripcionDto> Circunscripciones = new();
    protected List<JuzgadoDto> Juzgados = new();
    protected List<SecretariaDto> Secretarias = new();





    // Bandera para evitar disparar cargas en setters durante la reconstrucción
    private bool _suspendCascade;

    protected bool MostrarModalPartes { get; set; }

    

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









    // === Persona / Presupuestos ===

    //protected int? PersonaId;
    protected int? PersonaSeleccionada
    {
        get => Form.PersonaId;
        set
        {
            if (Form.PersonaId == value) return;
            Form.PersonaId = value;

            // al cambiar persona, limpiar presupuesto seleccionado
            Form.PresupuestoId = null;

            _ = CargarPresupuestosPorPersonaAsync();
        }
    }








    protected bool PresupuestosDeshabilitado { get; set; } = true;




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
                Form.PJ_NumeroCertificadoDeuda= null;
                Form.PJ_MontoDemanda = null;
                Form.PJ_FechaEmisionCertificado = null;
                Form.PJ_FechaLiquidacion = null;


                Circunscripciones.Clear();
                Juzgados.Clear();
                Secretarias.Clear();
            }
        }
    }



    // campos privados en el componente
    protected int? _procesoJudicialId;
    protected int? _procesoExtrajudicialId;
    protected DateTime? _fechaInicioExistente; // para preservar en edición




    protected GestionFormModel Form { get; set; } = new();



    private int? _apremioTipoProcesoId;



    protected override async Task OnInitializedAsync()
    {
        // Catálogos
        Responsables = await UsuarioService.ObtenerUsuariosAsync();
        Personas = await PersonaService.ObtenerPersonasAsync();
        Grupos = await GrupoGestionService.ObtenerTodosAsync();
        TiposProceso = await TipoProcesoService.ObtenerTodosAsync();
        DomiciliosConstituidos = await DomicilioService.ObtenerPorTipoAsync(TipoDomicilio.Constituido);
        DomiciliosElectronicos = await DomicilioService.ObtenerPorTipoAsync(TipoDomicilio.Electronico);
        Jurisdicciones = await JurisdiccionService.ObtenerTodosAsync();
        EstadosJudiciales = await EstadoService.ObtenerPorTipoAsync("Judicial");
        EstadosExtrajudiciales = await EstadoService.ObtenerPorTipoAsync("Extrajudicial");

        //  Buscar el tipo "APREMIO" en la lista que acabamos de traer
        var tipoApremio = TiposProceso
            .FirstOrDefault(t =>
                string.Equals(t.Nombre, "APREMIO", StringComparison.OrdinalIgnoreCase));

        _apremioTipoProcesoId = tipoApremio?.Id;


        if (Id.HasValue)
        {
            var dto = await GestionService.ObtenerPorIdAsync(Id.Value);
            if (dto is null) return;

            _fechaInicioExistente = dto.FechaInicio;
            _procesoJudicialId = dto.ProcesoJudicial?.Id;
            _procesoExtrajudicialId = dto.ProcesoExtrajudicial?.Id;
            // 🔁 Mapear DTO -> Form (comunes)
            Form = new GestionFormModel
            {
                TipoGestion = dto.TipoGestion,
                ResponsableId = dto.ResponsableId,
                GrupoId = dto.GrupoId,
                PersonaId = dto.PersonaId,
                PresupuestoId = dto.PresupuestoId  // ahora es int? en el VM
            };

            // 🔁 Judicial
            if (dto.TipoGestion == "Judicial" && dto.ProcesoJudicial is not null)
            {
                Form.PJ_Caratula = dto.ProcesoJudicial.Caratula;
                Form.PJ_NumeroExpediente = dto.ProcesoJudicial.NumeroExpediente;
                Form.PJ_TipoProcesoId = dto.ProcesoJudicial.TipoProcesoId;
                Form.PJ_DomicilioConstituidoId = dto.ProcesoJudicial.DomicilioConstituidoId;
                Form.PJ_DomicilioElectronicoId = dto.ProcesoJudicial.DomicilioElectronicoId;
                //Form.PJ_JurisdiccionId = dto.ProcesoJudicial.JurisdiccionId;
                //Form.PJ_CircunscripcionId = dto.ProcesoJudicial.CircunscripcionId;
                //Form.PJ_JuzgadoId = dto.ProcesoJudicial.JuzgadoId;
                //Form.PJ_SecretariaId = dto.ProcesoJudicial.SecretariaId;
                Form.PJ_EstadoId = dto.ProcesoJudicial.EstadoId;
                Form.PJ_NumeroCertificadoDeuda = dto.ProcesoJudicial.NumeroCertificadoDeuda;
                Form.PJ_FechaEmisionCertificado = dto.ProcesoJudicial.FechaEmisionCertificado;
                Form.PJ_FechaLiquidacion = dto.ProcesoJudicial.FechaLiquidacion;
                Form.PJ_MontoDemanda = dto.ProcesoJudicial.MontoDemanda;

                // 👇 ACÁ calculamos la bandera solo en el front
                Form.PJ_EsApremio = _apremioTipoProcesoId.HasValue
                    && Form.PJ_TipoProcesoId == _apremioTipoProcesoId.Value;


                // 🧭 Reconstruir jerarquía (Jurisdicción → Circunscripción → Juzgado → Secretarías)
                //await ReconstruirJerarquiaJudicialAsync(dto.ProcesoJudicial.SecretariaId);

                await ReconstruirJerarquiaJudicialAsync(
                      dto.ProcesoJudicial.JurisdiccionId,
                      dto.ProcesoJudicial.CircunscripcionId,
                      dto.ProcesoJudicial.JuzgadoId,
                      dto.ProcesoJudicial.SecretariaId
                );


            }

            // 🔁 Extrajudicial
            if (dto.TipoGestion == "Extrajudicial" && dto.ProcesoExtrajudicial is not null)
            {
                Form.EX_Tipo = dto.ProcesoExtrajudicial.Tipo;
                Form.EX_Materia = dto.ProcesoExtrajudicial.Materia;
                Form.EX_EstadoId = dto.ProcesoExtrajudicial.EstadoId; // opcional
            }

            // 💸 Presupuestos por persona
            if (Form.PersonaId.HasValue)
            {
                PersonaSeleccionada = Form.PersonaId;    // si tu lógica actual usa esta wrapper
                await CargarPresupuestosPorPersonaAsync();
            }
            else
            {
                PersonaSeleccionada = null;
                PresupuestosFiltrados.Clear();
                PresupuestosDeshabilitado = true;
            }
        }
        else
        {
            // NUEVA gestión
            Form = new GestionFormModel
            {
                TipoGestion = null,
                ResponsableId = null,
                PersonaId = null,
                GrupoId = null,
                PresupuestoId = null
            };

            // limpiar combos dependientes
            Circunscripciones.Clear();
            Juzgados.Clear();
            Secretarias.Clear();

            PersonaSeleccionada = null;
            PresupuestosFiltrados.Clear();
            PresupuestosDeshabilitado = true;
        }
    }









    private async Task ReconstruirJerarquiaJudicialAsync(
    int? jurisdiccionId,
    int? circunscripcionId,
    int? juzgadoId,
    int? secretariaId)
    {
        _suspendCascade = true;

        // 1) Cargar Jurisdicciones (si no las tenés ya cargadas)
        if (Jurisdicciones is null || !Jurisdicciones.Any())
            Jurisdicciones = await JurisdiccionService.ObtenerTodosAsync();

        // 2) Setear la selección (ahora sí usando los wrappers)
        JurisdiccionSeleccionada = jurisdiccionId;

        // 3) Circunscripciones
        if (jurisdiccionId.HasValue)
            Circunscripciones = await CircunscripcionService.ObtenerPorJurisdiccionAsync(jurisdiccionId.Value);

        CircunscripcionSeleccionada = circunscripcionId;

        // 4) Juzgados
        if (circunscripcionId.HasValue)
            Juzgados = await JuzgadoService.ObtenerPorCircunscripcionAsync(circunscripcionId.Value);

        JuzgadoSeleccionado = juzgadoId;

        // 5) Secretarías
        if (juzgadoId.HasValue)
            Secretarias = await SecretariaService.ObtenerPorJuzgadoAsync(juzgadoId.Value);

        Form.PJ_SecretariaId = secretariaId;

        _suspendCascade = false;

        await InvokeAsync(StateHasChanged);
    }







    protected async Task CargarCircunscripcionesAsync()
    {
        if (!JurisdiccionSeleccionada.HasValue) return;

        // Solo cargar la lista. Los "reset" (circ/juz/secretaría) ya los hace el setter.
        Circunscripciones = await CircunscripcionService
            .ObtenerPorJurisdiccionAsync(JurisdiccionSeleccionada.Value);

        // (Opcional) autoselección si hay una sola
        if (Circunscripciones.Count == 1)
        {
            _suspendCascade = true;
            Form.PJ_CircunscripcionId = Circunscripciones[0].Id;
            _suspendCascade = false;

            // Dispara la siguiente cascada
            await CargarJuzgadosAsync();
        }

        StateHasChanged(); // no imprescindible, pero ok
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

        // (Opcional) autoselección si hay una sola
        if (Secretarias.Count == 1)
        {
            _suspendCascade = true;
            Form.PJ_SecretariaId = Secretarias[0].Id;
            _suspendCascade = false;
        }

        StateHasChanged();
    }










  



    protected async Task CargarPresupuestosPorPersonaAsync()
    {
        Console.WriteLine($"[DEBUG] Persona seleccionada: {PersonaSeleccionada}"); // <- wrapper que ya escribe en Form.PersonaId

        if (Form.PersonaId.HasValue)
        {
            //PresupuestosFiltrados = await PresupuestoService.ObtenerPorPersonaAsync(Form.PersonaId.Value);

            var todos = await PresupuestoService.ObtenerPorPersonaAsync(Form.PersonaId.Value);


            // 🔥 FILTRAR SOLO APROBADOS
            // 1) Solo aprobados (disponibles para nuevas gestiones)
            var filtrados = todos
                .Where(p => p.Estado == EstadoPresupuesto.Aprobado)
                .ToList();

            Console.WriteLine($"[DEBUG] Presupuestos encontrados: {PresupuestosFiltrados.Count}");

            // 2) Si estoy editando una gestión y ya tiene un presupuesto asignado,
            // lo agrego a la lista para que no "desaparezca" del combo
            if (Form.PresupuestoId.HasValue)
            {
                var actual = todos.FirstOrDefault(p => p.Id == Form.PresupuestoId.Value);
                if (actual != null && !filtrados.Any(p => p.Id == actual.Id))
                {
                    filtrados.Add(actual);
                }
            }

            PresupuestosFiltrados = filtrados;
            PresupuestosDeshabilitado = PresupuestosFiltrados.Count == 0;

            // Si el presupuesto seleccionado ya no está en la lista, limpiá
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






    

    protected void AbrirModalPartes()
    {
        // solo por seguridad extra
        if (!Id.HasValue) return;
        if (Form.TipoGestion == "Judicial" && !_procesoJudicialId.HasValue) return;
        if (Form.TipoGestion == "Extrajudicial" && !_procesoExtrajudicialId.HasValue) return;

        MostrarModalPartes = true;
    }

    protected void CerrarModalPartes()
    {
        MostrarModalPartes = false;
        StateHasChanged();
    }











    protected async Task Guardar()
    {
        // ⚠️ Este método se llama desde OnValidSubmit, o sea que Form ya pasó validación
        //Gestion.PersonaId = Form.PersonaId;
        
        //Gestion.GrupoId = Form.GrupoId == 0 ? null : Form.GrupoId;
        // FechaInicio: si es edición preservo; si es alta uso ahora
        var fechaInicio = (Id.HasValue && _fechaInicioExistente.HasValue)
            ? _fechaInicioExistente.Value
            : DateTime.Now;

        // Presupuesto: si está deshabilitado, no enviar nada
        var presupuestoId = PresupuestosDeshabilitado ? (int?)null : Form.PresupuestoId;

        // Mapear Form -> DTO de API
        var gestion = new CrearGestionDto
        {
            TipoGestion = Form.TipoGestion!,                 // "Judicial" | "Extrajudicial"
            ResponsableId = Form.ResponsableId!.Value,
            PersonaId = Form.PersonaId!.Value,
            GrupoId = Form.GrupoId,                      // puede ser null
            PresupuestoId = presupuestoId,                     // null si no hay
            FechaInicio = fechaInicio,

            // Envío solo el bloque que corresponde y limpio el otro
            ProcesoJudicial = Form.TipoGestion == "Judicial"
                ? new CrearProcesoJudicialDto
                {
                    Id = _procesoJudicialId,         // clave para UPDATE
                    Caratula = Form.PJ_Caratula!,
                    NumeroExpediente = Form.PJ_NumeroExpediente!,
                    TipoProcesoId = Form.PJ_TipoProcesoId!.Value,
                    DomicilioConstituidoId = Form.PJ_DomicilioConstituidoId!.Value,
                    DomicilioElectronicoId = Form.PJ_DomicilioElectronicoId!.Value,
                    
                    EstadoId = Form.PJ_EstadoId!.Value,
                    // (si tu API usa JurisdiccionId/CircunscripcionId/JuzgadoId, agregalos acá)

                    SecretariaId = Form.PJ_SecretariaId,
                    // 👇 NUEVOS
                    JurisdiccionId = Form.PJ_JurisdiccionId,
                    CircunscripcionId = Form.PJ_CircunscripcionId,
                    JuzgadoId = Form.PJ_JuzgadoId
                }
                : null,

            ProcesoExtrajudicial = Form.TipoGestion == "Extrajudicial"
                ? new CrearProcesoExtrajudicialDto
                {
                    Id = _procesoExtrajudicialId, // clave para UPDATE
                    Tipo = Form.EX_Tipo!,
                    Materia = Form.EX_Materia!,
                    EstadoId = Form.EX_EstadoId          // opcional
                }
                : null
        };

        // Llamada a API
        if (Id.HasValue)
            await GestionService.ActualizarAsync(Id.Value, gestion);
        else
            await GestionService.CrearAsync(gestion);

        Nav.NavigateTo("/gestiones");
    }






    protected async Task IrAMovimientosDesdeGestion()
    {
        if (!Id.HasValue) return;

        if (Form.TipoGestion == "Judicial" && _procesoJudicialId.HasValue)
        {
            await IrAMovimientosJudicial(Id.Value, _procesoJudicialId.Value);
            return;
        }

        if (Form.TipoGestion == "Extrajudicial" && _procesoExtrajudicialId.HasValue)
        {
            await IrAMovimientosExtrajudicial(Id.Value, _procesoExtrajudicialId.Value);
            return;
        }
    }


    protected async Task IrAMovimientos(string tipo, int gestionId, int procesoId)
    {
        //  Guardamos returnUrl (la página actual: la gestión)
        await JS.InvokeVoidAsync("localStorage.setItem", "return_url_movimientos", Nav.Uri);

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



//protected void Cancelar() => Nav.NavigateTo("/gestiones");

protected void Cancelar()
    {
        var uri = new Uri(Nav.Uri);
        var qs = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

        if (qs.TryGetValue("returnUrl", out var returnUrl) && !string.IsNullOrWhiteSpace(returnUrl))
            Nav.NavigateTo(returnUrl!);
        else
            Nav.NavigateTo("/gestiones"); // fallback
    }
}
