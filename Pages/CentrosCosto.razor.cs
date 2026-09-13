using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class CentrosCostoBase : ComponentBase
{
    [Inject] protected CentroCostoService Service { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected List<CentroCostoDto> Centros { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Centros = await Service.ObtenerTodosAsync() ?? new();
    }

    protected void Nuevo() => Nav.NavigateTo("/centros-costo/editar");

    protected void Editar(int id) => Nav.NavigateTo($"/centros-costo/editar/{id}");

    protected async Task Eliminar(int id)
    {
        if (await JS.InvokeAsync<bool>("confirm", "¿Seguro que desea eliminar este centro de costo?"))
        {
            await Service.EliminarAsync(id);
            Centros = await Service.ObtenerTodosAsync() ?? new();
            StateHasChanged();
        }
    }
}
