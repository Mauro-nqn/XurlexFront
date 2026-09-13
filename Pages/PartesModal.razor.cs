using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;

public partial class PartesModalBase : ComponentBase
{

    // 🔹 INYECCIONES
    [Inject] protected PersonaService PersonaService { get; set; } = default!;
    [Inject] protected CaracterIntervencionService CaracterService { get; set; } = default!;
    [Inject] protected ParteProcesoService PartesService { get; set; } = default!;


    [Parameter] public int? ProcesoJudicialId { get; set; }
    [Parameter] public int? ProcesoExtrajudicialId { get; set; }
    [Parameter] public string? TipoGestion { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    protected bool Visible => (ProcesoJudicialId.HasValue || ProcesoExtrajudicialId.HasValue);

    protected List<ParteProcesoDto> Partes { get; set; } = new();
    protected List<PersonaDto> Personas { get; set; } = new();
    protected List<CaracterIntervencionDto> Caracteres { get; set; } = new();

    protected int? PersonaIdSeleccionada { get; set; }
    protected int? CaracterIdSeleccionado { get; set; }

    protected bool PuedeAgregar =>
        PersonaIdSeleccionada.HasValue &&
        CaracterIdSeleccionado.HasValue;

    protected override async Task OnInitializedAsync()
    {
        Personas = await PersonaService.ObtenerPersonasAsync();
        Caracteres = await CaracterService.ObtenerTodosAsync();

        if (ProcesoJudicialId.HasValue)
        {
            Partes = await PartesService.ObtenerPorProcesoJudicialAsync(ProcesoJudicialId.Value);
        }
        else if (ProcesoExtrajudicialId.HasValue)
        {
            Partes = await PartesService.ObtenerPorProcesoExtrajudicialAsync(ProcesoExtrajudicialId.Value);
        }
    }

    protected async Task AgregarParte()
    {
        if (!PuedeAgregar) return;

        var dto = new CrearParteProcesoDto
        {
            ProcesoJudicialId = ProcesoJudicialId,
            ProcesoExtrajudicialId = ProcesoExtrajudicialId,
            PersonaId = PersonaIdSeleccionada!.Value,
            CaracterIntervencionId = CaracterIdSeleccionado!.Value
        };

        await PartesService.CrearAsync(dto);

        // recargar lista
        if (ProcesoJudicialId.HasValue)
        {
            Partes = await PartesService.ObtenerPorProcesoJudicialAsync(ProcesoJudicialId.Value);
        }
        else if (ProcesoExtrajudicialId.HasValue)
        {
            Partes = await PartesService.ObtenerPorProcesoExtrajudicialAsync(ProcesoExtrajudicialId.Value);
        }

        // limpiar selección
        PersonaIdSeleccionada = null;
        CaracterIdSeleccionado = null;
    }

    protected async Task EliminarParte(int id)
    {
        await PartesService.EliminarAsync(id);

        if (ProcesoJudicialId.HasValue)
        {
            Partes = await PartesService.ObtenerPorProcesoJudicialAsync(ProcesoJudicialId.Value);
        }
        else if (ProcesoExtrajudicialId.HasValue)
        {
            Partes = await PartesService.ObtenerPorProcesoExtrajudicialAsync(ProcesoExtrajudicialId.Value);
        }
    }

    protected async Task Cerrar()
    {
        if (OnClose.HasDelegate)
            await OnClose.InvokeAsync();

        // el padre se encarga de cambiar MostrarModalPartes = false
    }
}
