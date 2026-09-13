using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class CaracterIntervencionBase : ComponentBase
{
    [Inject] protected CaracterIntervencionService? Service { get; set; }
    [Inject] protected NavigationManager? Nav { get; set; }
    [Inject] protected IJSRuntime? JS { get; set; }

    protected List<CaracterIntervencionDto>? Caracteres;

    protected override async Task OnInitializedAsync()
    {
        Caracteres = await Service!.ObtenerTodosAsync();
    }

    protected void Nuevo() => Nav!.NavigateTo("/caracteresintervencion/crear-editar");
    protected void Editar(int id) => Nav!.NavigateTo($"/caracteresintervencion/crear-editar/{id}");

    protected async Task Eliminar(int id)
    {
        var confirmar = await JS!.InvokeAsync<bool>("confirm", "¿Eliminar este carácter de intervención?");
        if (confirmar)
        {
            await Service!.EliminarAsync(id);
            Caracteres = await Service.ObtenerTodosAsync();
        }
    }
}
