using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class CondicionIvaBase : ComponentBase
{
    [Inject] protected CondicionIvaService? CondicionIvaService { get; set; }
    [Inject] protected NavigationManager? Nav { get; set; }

    [Inject] private IJSRuntime? JS { get; set; }

    protected List<CondicionIvaDto>? Condiciones;

    protected override async Task OnInitializedAsync()
    {
        Condiciones = await CondicionIvaService!.ObtenerTodasAsync();
    }

    protected void NuevaCondicion()
    {
        Nav!.NavigateTo("/condicioniva/editar");
    }

    protected void EditarCondicion(int id)
    {
        Nav!.NavigateTo($"/condicioniva/editar/{id}");
    }

    protected async Task EliminarCondicion(int id)
    {
        var confirmar = await JS!.InvokeAsync<bool>("confirm", "¿Seguro que deseas eliminar esta condición de IVA?");
        if (confirmar)
        {
            await CondicionIvaService!.EliminarAsync(id);
            Condiciones = await CondicionIvaService.ObtenerTodasAsync(); // Refrescar listado
            StateHasChanged();
        }
    }
}
