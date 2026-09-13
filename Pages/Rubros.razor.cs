using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class RubrosBase : ComponentBase
{
    [Inject] protected RubroService Service { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected List<RubroDto> Rubros { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Rubros = await Service.ObtenerTodosAsync() ?? new();
    }

    protected void Nuevo() => Nav.NavigateTo("/rubros/editar");

    protected void Editar(int id) => Nav.NavigateTo($"/rubros/editar/{id}");

    protected async Task Eliminar(int id)
    {
        if (await JS.InvokeAsync<bool>("confirm", "¿Seguro que desea eliminar este rubro?"))
        {
            await Service.EliminarAsync(id);
            Rubros = await Service.ObtenerTodosAsync() ?? new();
            StateHasChanged();
        }
    }
}
