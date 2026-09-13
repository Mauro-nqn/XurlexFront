using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Linq; // Necesario para usar .Any()

public class CrearEditarSecretariaBase : ComponentBase
{
    protected CrearSecretariaDto Secretaria = new(); //  Cambiado al DTO correcto
    protected List<JurisdiccionDto> Jurisdicciones = new();
    protected List<CircunscripcionDto> Circunscripciones = new();
    protected List<JuzgadoDto> Juzgados = new();

    //protected int? JurisdiccionSeleccionada;


    private bool _initializing; // << guard


    protected int? _jurisdiccionSeleccionada;
    protected int? JurisdiccionSeleccionada
    {
        get => _jurisdiccionSeleccionada;
        set
        {
            if (_jurisdiccionSeleccionada != value)
            {
                _jurisdiccionSeleccionada = value;
                if (!_initializing)
                    _ = CargarCircunscripcionesAsync();
            }
        }
    }

    //protected int? CircunscripcionSeleccionada;
    protected int? _circunscripcionSeleccionada;
    protected int? CircunscripcionSeleccionada
    {
        get => _circunscripcionSeleccionada;
        set
        {
            if (_circunscripcionSeleccionada != value)
            {
                _circunscripcionSeleccionada = value;
                if (!_initializing)
                    _ = CargarJuzgadosAsync();
            }
        }
    }


    [Inject] protected JurisdiccionService JurisdiccionService { get; set; } = default!;
    [Inject] protected CircunscripcionService CircunscripcionService { get; set; } = default!;
    [Inject] protected JuzgadoService JuzgadoService { get; set; } = default!;
    [Inject] protected SecretariaService SecretariaService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    //protected override async Task OnInitializedAsync()
    //{
    //    // Cargar todas las jurisdicciones
    //    Jurisdicciones = await JurisdiccionService.ObtenerTodosAsync();

    //    if (Id.HasValue) // 🖊 Modo Edición
    //    {
    //        var existente = await SecretariaService.ObtenerPorIdAsync(Id.Value);
    //        if (existente != null)
    //        {
    //            Secretaria = new CrearSecretariaDto
    //            {
    //                Nombre = existente.Nombre,
    //                Juez = existente.Juez,
    //                Secretario = existente.Secretario,
    //                JuzgadoId = existente.JuzgadoId
    //            };

    //            // ✅ Cargar datos relacionados (Circunscripción y Jurisdicción)
    //            var juzgado = await JuzgadoService.ObtenerPorIdAsync(existente.JuzgadoId);
    //            if (juzgado != null)
    //            {
    //                CircunscripcionSeleccionada = juzgado.CircunscripcionId;
    //                var circ = await CircunscripcionService.ObtenerPorIdAsync(juzgado.CircunscripcionId);

    //                if (circ != null)
    //                {
    //                    JurisdiccionSeleccionada = circ.JurisdiccionId;

    //                    // Cargar listas filtradas
    //                    Circunscripciones = await CircunscripcionService.ObtenerPorJurisdiccionAsync(JurisdiccionSeleccionada.Value);
    //                    Juzgados = await JuzgadoService.ObtenerPorCircunscripcionAsync(CircunscripcionSeleccionada.Value);
    //                }
    //            }
    //        }
    //    }
    //    else
    //    {
    //        Secretaria = new CrearSecretariaDto(); // Crear vacía
    //    }
    //}
    protected override async Task OnInitializedAsync()
    {
        _initializing = true;

        Jurisdicciones = await JurisdiccionService.ObtenerTodosAsync();

        if (Id.HasValue)
        {
            var existente = await SecretariaService.ObtenerPorIdAsync(Id.Value);
            if (existente is not null)
            {
                Secretaria = new CrearSecretariaDto
                {
                    Nombre = existente.Nombre,
                    Juez = existente.Juez,
                    Secretario = existente.Secretario,
                    JuzgadoId = existente.JuzgadoId
                };

                var juzgado = await JuzgadoService.ObtenerPorIdAsync(existente.JuzgadoId);
                if (juzgado is not null)
                {
                    // Seteo directo a backing field para no disparar cargas durante init:
                    _circunscripcionSeleccionada = juzgado.CircunscripcionId;

                    var circ = await CircunscripcionService.ObtenerPorIdAsync(juzgado.CircunscripcionId);
                    if (circ is not null)
                    {
                        // ojo: si JurisdiccionId es nullable en el DTO, validarlo
                        if (circ.JurisdiccionId is int jId && jId > 0)
                            _jurisdiccionSeleccionada = jId;
                        else
                            _jurisdiccionSeleccionada = null; // evita .Value
                    }
                }

                // Cargar listas solo si hay IDs válidos
                if (JurisdiccionSeleccionada is int jid && jid > 0)
                    Circunscripciones = await CircunscripcionService.ObtenerPorJurisdiccionAsync(jid);

                if (CircunscripcionSeleccionada is int cid && cid > 0)
                    Juzgados = await JuzgadoService.ObtenerPorCircunscripcionAsync(cid);
            }
        }
        else
        {
            Secretaria = new CrearSecretariaDto();
        }

        _initializing = false;
    }







    protected async Task CargarCircunscripcionesAsync()
    {
        if (JurisdiccionSeleccionada.HasValue)
        {
            Circunscripciones = await CircunscripcionService.ObtenerPorJurisdiccionAsync(JurisdiccionSeleccionada.Value);
            CircunscripcionSeleccionada = null;
            Juzgados.Clear();
            Secretaria.JuzgadoId = 0;

            Console.WriteLine($"Circunscripciones cargadas: {Circunscripciones.Count}");
            StateHasChanged();
        }
    }





  

    protected async Task CargarJuzgadosAsync()
    {
        if (CircunscripcionSeleccionada.HasValue)
        {
            Juzgados = await JuzgadoService.ObtenerPorCircunscripcionAsync(CircunscripcionSeleccionada.Value);
            Secretaria.JuzgadoId = 0; // Resetear selección previa
            Console.WriteLine($"Juzgados cargados: {Juzgados.Count}");
            StateHasChanged(); // Forzar actualización visual
        }
        else
        {
            Juzgados.Clear();
            Secretaria.JuzgadoId = 0;
        }
    }


    protected async Task Guardar()
    {
        if (Id.HasValue)
            await SecretariaService.ActualizarAsync(Id.Value, Secretaria);
        else
            await SecretariaService.CrearAsync(Secretaria);

        await JS.InvokeVoidAsync("alert", Id.HasValue ? "Secretaría actualizada" : "Secretaría creada");
        Nav.NavigateTo("/secretarias");
    }



    protected void Cancelar()
    {
        Nav.NavigateTo("/secretarias");
    }

    protected async Task Eliminar()
    {
        if (Id.HasValue)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "¿Está seguro de eliminar esta Secretaría?");
            if (confirm)
            {
                await SecretariaService.EliminarAsync(Id.Value);
                await JS.InvokeVoidAsync("alert", "Secretaría eliminada correctamente");
                Nav.NavigateTo("/secretarias");
            }
        }
    }
}


