using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class PuntoVentaBase : ComponentBase
{
    [Inject] protected PuntoVentaService PuntoService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected List<PuntoVentaDto> Puntos { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Puntos = await PuntoService.ObtenerTodosAsync();
    }

    protected void NuevoPunto() => Nav.NavigateTo("/puntos-venta/editar");

    protected void Editar(int id) => Nav.NavigateTo($"/puntos-venta/editar/{id}");

    protected async Task Eliminar(int id)
    {
        if (await JS.InvokeAsync<bool>("confirm", "¿Seguro que desea eliminar este punto de venta?"))
        {
            await PuntoService.EliminarAsync(id);
            Puntos = await PuntoService.ObtenerTodosAsync();
        }
    }
}
