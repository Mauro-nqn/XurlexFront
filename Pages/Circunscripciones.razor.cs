using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class CircunscripcionBase : ComponentBase
{
    [Inject] protected CircunscripcionService? Service { get; set; }
    [Inject] protected NavigationManager? Nav { get; set; }
    [Inject] protected IJSRuntime? JS { get; set; }

    protected List<CircunscripcionDto>? Circunscripciones;

    protected override async Task OnInitializedAsync()
    {
        Circunscripciones = await Service!.ObtenerTodosAsync();
    }

    protected void Nueva() => Nav!.NavigateTo("/circunscripciones/crear-editar");
    protected void Editar(int id) => Nav!.NavigateTo($"/circunscripciones/crear-editar/{id}");

    protected async Task Eliminar(int id)
    {
        var confirmar = await JS!.InvokeAsync<bool>("confirm", "¿Eliminar esta circunscripción?");
        if (confirmar)
        {
            await Service!.EliminarAsync(id);
            Circunscripciones = await Service.ObtenerTodosAsync();
        }
    }
}

