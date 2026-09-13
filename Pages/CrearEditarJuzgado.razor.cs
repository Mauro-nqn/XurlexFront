using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Helpers;
using IurixBlazor.Shared.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using static IurixBlazor.Shared.Helpers.DextraEnumHelper;
using IurixBlazor.Pages;

public class CrearEditarJuzgadoBase : ComponentBase
{
    [Inject] protected JuzgadoService? Service { get; set; }
    [Inject] protected CircunscripcionService? CircunscripcionService { get; set; } = default!;
    [Inject] protected JurisdiccionService? JurisdiccionService { get; set; }
    [Inject] protected NavigationManager? Nav { get; set; }
    [Inject] protected IJSRuntime? JS { get; set; }

    [Parameter] public int? Id { get; set; }

    protected CrearJuzgadoDto CrearJuzgadoDto { get; set; } = new();
    protected JuzgadoDto? Juzgado { get; set; }

    protected List<JurisdiccionDto> Jurisdicciones { get; set; } = new();
    protected List<CircunscripcionDto> Circunscripciones { get; set; } = new();

    

    private bool _suspendCascade;

    private int? _jurisdiccionSeleccionada;
    protected int? JurisdiccionSeleccionada
    {
        get => _jurisdiccionSeleccionada;
        set
        {
            if (_jurisdiccionSeleccionada == value) return;
            _jurisdiccionSeleccionada = value;
            if (_suspendCascade) return;              // ⬅️ no dispares nada mientras restaurás
            _ = CargarCircunscripcionesAsync();       // sigue sin await en setter
        }
    }

    protected string? DextraAplicacionExacta { get; set; }
    // Detecta Neuquén por nombre, sin tildes y case-insensitive
    protected bool EsNeuquen =>
        Jurisdicciones?.FirstOrDefault(j => j.Id == JurisdiccionSeleccionada) is { } j
        && IsNeuquenByName(j.Nombre);



    // Opciones para el select de Aplicación Dextra


    //protected IEnumerable<(string val, string label)> AppsDextra =>
    //Enum.GetValues(typeof(AplicacionDextra))
    //    .Cast<AplicacionDextra>()
    //    .Select(e => (e.ToExactString(), e.ToExactString()));

    // Nombre actual de la circunscripción seleccionada (o vacío)
    // Nombre de circ actual (normalizado) según el DTO + la lista cargada
    // clave normalizada de la circ actual
    // Normaliza el nombre de la circ actual (desde la lista y el DTO)
    // 1°: tomá de la lista (cuando ya cargó). 2°: fallback al nombre que viene del back.
    protected string CircKeyActual =>
        DextraEnumHelper.NormalizeNoAccents(
            Circunscripciones.FirstOrDefault(c => c.Id == (CrearJuzgadoDto?.CircunscripcionId ?? 0))?.Nombre
            ?? Juzgado?.CircunscripcionNombre                // 👈 Fallback para el primer render ("ZAPALA CIRCUNSCRIPCION III")
        ).ToUpperInvariant();

    protected IEnumerable<AplicacionDextra> OpcionesAplicacionDextraEnum =>
        Enum.GetValues<AplicacionDextra>().Where(e => e.AplicaACirc(CircKeyActual));

    // SAFE: incluye la selección actual (por id) para que no desaparezca en primer render
    protected IEnumerable<AplicacionDextra> OpcionesAplicacionDextraEnumSafe
    {
        get
        {
            var baseList = OpcionesAplicacionDextraEnum.ToList();
            if (CrearJuzgadoDto?.AplicacionDextraId is int selId
                && Enum.IsDefined(typeof(AplicacionDextra), selId))
            {
                var selEnum = (AplicacionDextra)selId;
                if (!baseList.Contains(selEnum)) baseList.Add(selEnum);
            }
            return baseList;
        }
    }


    protected bool CargandoCirc;

    private bool _initializing;  // bandera


    //protected override async Task OnInitializedAsync()
    //{
    //    Jurisdicciones = await JurisdiccionService!.ObtenerTodosAsync();

    //    if (Id.HasValue)
    //    {
    //        Juzgado = await Service!.ObtenerPorIdAsync(Id.Value);
    //        if (Juzgado != null)
    //        {
    //            CrearJuzgadoDto = new CrearJuzgadoDto
    //            {
    //                Nombre = Juzgado.Nombre,
    //                Fuero = Juzgado.Fuero,
    //                CircunscripcionId = Juzgado.CircunscripcionId,
    //                AplicacionDextra = Juzgado.AplicacionDextra
    //            };

    //            // Obtener circunscripción y jurisdicción relacionada
    //            var circ = await CircunscripcionService!.ObtenerPorIdAsync(Juzgado.CircunscripcionId);
    //            if (circ != null)
    //            {
    //                _suspendCascade = true; // 🔒 desactiva cascada para que el setter no ejecute CargarCircunscripcionesAsync

    //                JurisdiccionSeleccionada = circ.JurisdiccionId;

    //                CargandoCirc = true;
    //                StateHasChanged();

    //                // Cargar manualmente las circunscripciones de esa jurisdicción
    //                Circunscripciones = await CircunscripcionService.ObtenerPorJurisdiccionAsync(circ.JurisdiccionId);

    //                // Restaurar la selección solo si existe en la lista
    //                if (Circunscripciones.Any(c => c.Id == Juzgado.CircunscripcionId))
    //                    CrearJuzgadoDto.CircunscripcionId = Juzgado.CircunscripcionId;
    //                else
    //                    CrearJuzgadoDto.CircunscripcionId = null; // seguridad

    //                CargandoCirc = false;
    //                _suspendCascade = false; // 🔓 reactivar cascada

    //                StateHasChanged();
    //                ValidarAplicacionDextraActual();
    //            }

    //        }
    //    }
    //}

    protected override async Task OnInitializedAsync()
{
    _initializing = true;

    Jurisdicciones = await JurisdiccionService!.ObtenerTodosAsync();

    if (Id.HasValue)
    {
        Juzgado = await Service!.ObtenerPorIdAsync(Id.Value);
        if (Juzgado != null)
        {
            CrearJuzgadoDto = new CrearJuzgadoDto
            {
                Nombre = Juzgado.Nombre,
                Fuero = Juzgado.Fuero,
                CircunscripcionId = Juzgado.CircunscripcionId,
                //AplicacionDextra = Juzgado.AplicacionDextra

                DextraAplicacionExacta = Juzgado.DextraAplicacionExacta,

                // 👇 casteo explícito a int?
                AplicacionDextraId = Juzgado.AplicacionDextra.HasValue
        ? (int?)Juzgado.AplicacionDextra.Value
        : null,

                // opcional: sincronizá también el enum
                AplicacionDextra = Juzgado.AplicacionDextra



            };

            var circ = await CircunscripcionService!.ObtenerPorIdAsync(Juzgado.CircunscripcionId);
            if (circ != null)
            {
                _suspendCascade = true;

                JurisdiccionSeleccionada = circ.JurisdiccionId;

                CargandoCirc = true;
                StateHasChanged();

                Circunscripciones = await CircunscripcionService
                    .ObtenerPorJurisdiccionAsync(circ.JurisdiccionId);

                
                // Restaurar circ del DTO si existe en la lista
                if (Circunscripciones.Any(c => c.Id == Juzgado.CircunscripcionId))
                    CrearJuzgadoDto.CircunscripcionId = Juzgado.CircunscripcionId;
                else
                    CrearJuzgadoDto.CircunscripcionId = null;

                CargandoCirc = false;
                _suspendCascade = false;
                StateHasChanged();

               
            }
        }
    }

    _initializing = false;

        // Ahora SÍ validar la app DEXTRA (ya hay circ y lista)
        ValidarAplicacionDextraActual();
    }




    // Cuando cambia circunscripción (o al editar después de reconstruir), validá selección
    // Validar la selección actual de AplicacionDextra (enum) contra las opciones de la circ
    protected void ValidarAplicacionDextraActual(bool force = false)
    {
        // Si estoy inicializando o todavía no hay opciones, no limpies.
        var opciones = OpcionesAplicacionDextraEnum.ToList();
        if (!force && (_initializing || opciones.Count == 0))
            return;

        var set = new HashSet<AplicacionDextra>(opciones);
        if (CrearJuzgadoDto.AplicacionDextra.HasValue &&
            !set.Contains(CrearJuzgadoDto.AplicacionDextra.Value))
        {
            CrearJuzgadoDto.AplicacionDextra = null;
            StateHasChanged();
        }
    }


    //protected async Task CargarCircunscripcionesAsync()
    //{
    //    CargandoCirc = true;

    //    // ⚠️ sólo limpiá si NO estás restaurando y si el id actual no pertenece a la nueva lista
    //    int? prev = CrearJuzgadoDto?.CircunscripcionId;

    //    try
    //    {
    //        var lista = JurisdiccionSeleccionada is int jid
    //            ? await CircunscripcionService!.ObtenerPorJurisdiccionAsync(jid)
    //            : new List<CircunscripcionDto>();

    //        Circunscripciones = lista;

    //        if (!_suspendCascade)
    //        {
    //            // si la selección actual no es válida para esta jurisdicción, limpiá
    //            if (!(prev.HasValue && lista.Any(c => c.Id == prev.Value)))
    //                CrearJuzgadoDto.CircunscripcionId = null;
    //        }
    //        // si _suspendCascade == true, no toques CrearJuzgadoDto.CircunscripcionId
    //    }
    //    finally
    //    {
    //        CargandoCirc = false;
    //        StateHasChanged();
    //    }
    //}














    protected async Task CargarCircunscripcionesAsync()
    {
        CargandoCirc = true;
        var prev = CrearJuzgadoDto?.CircunscripcionId;

        try
        {
            var lista = JurisdiccionSeleccionada is int jid
                ? await CircunscripcionService!.ObtenerPorJurisdiccionAsync(jid)
                : new List<CircunscripcionDto>();

            Circunscripciones = lista;

            // ❗ No limpies durante restauración
            if (!_suspendCascade && !_initializing)
            {
                if (!(prev.HasValue && lista.Any(c => c.Id == prev.Value)))
                    CrearJuzgadoDto!.CircunscripcionId = null;

                // validar app dextra SOLO fuera de init
                ValidarAplicacionDextraActual();
            }
        }
        finally
        {
            CargandoCirc = false;
            StateHasChanged();
        }
    }



    protected void SincronizarAplicacionSeleccionada()
    {
        if (CrearJuzgadoDto.AplicacionDextraId.HasValue &&
            Enum.IsDefined(typeof(AplicacionDextra), CrearJuzgadoDto.AplicacionDextraId.Value))
        {
            var e = (AplicacionDextra)CrearJuzgadoDto.AplicacionDextraId.Value;
            CrearJuzgadoDto.AplicacionDextra = e;
            CrearJuzgadoDto.DextraAplicacionExacta = e.ToExactString();
        }
        else
        {
            CrearJuzgadoDto.AplicacionDextra = null;
            CrearJuzgadoDto.DextraAplicacionExacta = null;
        }
    }


    protected async Task Guardar()
    {
        SincronizarAplicacionSeleccionada();
        if (Id.HasValue)
            await Service!.ActualizarAsync(Id.Value, CrearJuzgadoDto);
        else
            await Service!.CrearAsync(CrearJuzgadoDto);

        Nav!.NavigateTo("/juzgados");
    }

    protected async Task Eliminar()
    {
        if (Id.HasValue && Juzgado != null)
        {
            var confirmar = await JS!.InvokeAsync<bool>("confirm", $"¿Eliminar {Juzgado.Nombre}?");
            if (confirmar)
            {
                await Service!.EliminarAsync(Id.Value);
                Nav!.NavigateTo("/juzgados");
            }
        }
    }

    protected void Cancelar() => Nav!.NavigateTo("/juzgados");
}
