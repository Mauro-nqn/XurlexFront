using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class CrearEditarTipoEstadoProcesoBase : ComponentBase
{
    [Inject] protected TipoEstadoProcesoService? Service { get; set; }
    [Inject] protected NavigationManager? Nav { get; set; }
    [Inject] protected IJSRuntime? JS { get; set; }

    [Parameter] public int? Id { get; set; }
    protected TipoEstadoProcesoDto Estado { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            var dto = await Service!.ObtenerPorIdAsync(Id.Value);
            if (dto != null) Estado = dto;
        }
    }

    protected async Task Guardar()
    {
        if (Id.HasValue)
            await Service!.ActualizarAsync(Estado.Id, Estado);
        else
            await Service!.CrearAsync(Estado);

        Nav!.NavigateTo("/tiposestadoproceso");
    }

    protected async Task Eliminar()
    {
        if (Id.HasValue)
        {
            var confirmar = await JS!.InvokeAsync<bool>("confirm", $"¿Eliminar {Estado.Nombre}?");
            if (confirmar)
            {
                await Service!.EliminarAsync(Id.Value);
                Nav!.NavigateTo("/tiposestadoproceso");
            }
        }
    }

    protected void Cancelar() => Nav!.NavigateTo("/tiposestadoproceso");
}
