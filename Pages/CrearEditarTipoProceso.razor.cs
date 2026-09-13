using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class CrearEditarTipoProcesoBase : ComponentBase
{
    [Inject] protected TipoProcesoService Service { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected CrearTipoProcesoDto Tipo { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            var dto = await Service.ObtenerPorIdAsync(Id.Value);
            if (dto != null)
                Tipo = new CrearTipoProcesoDto { Nombre = dto.Nombre };
        }
    }

    protected async Task Guardar()
    {
        if (Id.HasValue)
            await Service.ActualizarAsync(Id.Value, Tipo);
        else
            await Service.CrearAsync(Tipo);

        Nav.NavigateTo("/tiposproceso");
    }

    protected async Task Eliminar()
    {
        if (Id.HasValue)
        {
            var confirmar = await JS.InvokeAsync<bool>("confirm", $"¿Eliminar {Tipo.Nombre}?");
            if (confirmar)
            {
                await Service.EliminarAsync(Id.Value);
                Nav.NavigateTo("/tiposproceso");
            }
        }
    }

    protected void Cancelar() => Nav.NavigateTo("/tiposproceso");
}
