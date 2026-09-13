using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class TipoEstadoProcesoBase : ComponentBase
{
    [Inject] protected TipoEstadoProcesoService? Service { get; set; }
    [Inject] protected NavigationManager? Nav { get; set; }
    [Inject] protected IJSRuntime? JS { get; set; }

    protected List<TipoEstadoProcesoDto>? Estados;

    protected override async Task OnInitializedAsync()
    {
        Estados = await Service!.ObtenerTodosAsync();
    }

    protected void Nuevo() => Nav!.NavigateTo("/tiposestadoproceso/crear-editar");
    protected void Editar(int id) => Nav!.NavigateTo($"/tiposestadoproceso/crear-editar/{id}");

    protected async Task Eliminar(int id)
    {
        var confirmar = await JS!.InvokeAsync<bool>("confirm", "¿Eliminar este estado de proceso?");
        if (confirmar)
        {
            await Service!.EliminarAsync(id);
            Estados = await Service.ObtenerTodosAsync();
        }
    }
}
