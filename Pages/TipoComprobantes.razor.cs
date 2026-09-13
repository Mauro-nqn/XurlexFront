using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class TipoComprobantesBase : ComponentBase
{
    [Inject] protected TipoComprobanteService TipoService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected List<TipoComprobanteDto> Tipos { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Tipos = await TipoService.ObtenerTodosAsync();
    }

    protected void NuevoTipo() => Nav.NavigateTo("/tipo-comprobantes/editar");

    protected void Editar(int codigo) => Nav.NavigateTo($"/tipo-comprobantes/editar/{codigo}");

    protected async Task Eliminar(int codigo)
    {
        if (await JS.InvokeAsync<bool>("confirm", "¿Seguro que desea eliminar este tipo?"))
        {
            await TipoService.EliminarAsync(codigo);
            Tipos = await TipoService.ObtenerTodosAsync();
        }
    }
}
