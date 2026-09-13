using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class IvaAlicuotaBase : ComponentBase
{
    [Inject] protected IvaAlicuotaService? IvaAlicuotaService { get; set; }
    [Inject] protected NavigationManager? Nav { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }

    protected List<IvaAlicuotaDto>? Alicuotas;

    protected override async Task OnInitializedAsync()
    {
        Alicuotas = await IvaAlicuotaService!.ObtenerTodasAsync();
    }

    protected void NuevaAlicuota()
    {
        Nav!.NavigateTo("/ivaalicuotas/editar");
    }

    protected void EditarAlicuota(int id)
    {
        Nav!.NavigateTo($"/ivaalicuotas/editar/{id}");
    }

    protected async Task EliminarAlicuota(int id)
    {
        var confirmar = await JS!.InvokeAsync<bool>("confirm", "¿Seguro que deseas eliminar esta alícuota?");
        if (confirmar)
        {
            await IvaAlicuotaService!.EliminarAsync(id);
            Alicuotas = await IvaAlicuotaService.ObtenerTodasAsync(); // Refrescar listado
            StateHasChanged();
        }
    }
}
