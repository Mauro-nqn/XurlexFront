using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class TipoAgendamientoBase : ComponentBase
{
    [Inject] protected TipoAgendamientoService TipoService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected List<TipoAgendamientoDto> Tipos { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Tipos = await TipoService.ObtenerTodosAsync();
    }

    protected void Nuevo() => Nav.NavigateTo("/tipo-agendamiento/editar");

    protected void Editar(int id) => Nav.NavigateTo($"/tipo-agendamiento/editar/{id}");

    protected async Task Eliminar(int id)
    {
        if (await JS.InvokeAsync<bool>("confirm", "¿Seguro que desea eliminar este tipo de agendamiento?"))
        {
            await TipoService.EliminarAsync(id);
            Tipos = await TipoService.ObtenerTodosAsync();
        }
    }
}
