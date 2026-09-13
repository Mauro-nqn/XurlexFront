using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class TipoProcesoBase : ComponentBase
{
    [Inject] protected TipoProcesoService Service { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected List<TipoProcesoDto>? Tipos;

    protected override async Task OnInitializedAsync()
    {
        Tipos = await Service.ObtenerTodosAsync();
    }

    protected void Nuevo() => Nav.NavigateTo("/tiposproceso/crear-editar");

    protected void Editar(int id) => Nav.NavigateTo($"/tiposproceso/crear-editar/{id}");

    protected async Task Eliminar(int id)
    {
        var confirmar = await JS.InvokeAsync<bool>("confirm", "¿Eliminar este tipo de proceso?");
        if (confirmar)
        {
            await Service.EliminarAsync(id);
            Tipos = await Service.ObtenerTodosAsync();
        }
    }
}
