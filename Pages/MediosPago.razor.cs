using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class MediosPagoBase : ComponentBase
{
    [Inject] protected MedioPagoService Service { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected List<MedioPagoDto> Medios { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Medios = await Service.ObtenerTodosAsync() ?? new();
    }

    protected void Nuevo() => Nav.NavigateTo("/medios-pago/editar");

    protected void Editar(int id) => Nav.NavigateTo($"/medios-pago/editar/{id}");

    protected async Task Eliminar(int id)
    {
        if (await JS.InvokeAsync<bool>("confirm", "¿Seguro que desea eliminar este medio de pago?"))
        {
            await Service.EliminarAsync(id);
            Medios = await Service.ObtenerTodosAsync() ?? new();
            StateHasChanged();
        }
    }
}
