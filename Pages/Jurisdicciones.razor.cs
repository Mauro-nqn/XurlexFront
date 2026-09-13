using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class JurisdiccionBase : ComponentBase
{
    [Inject] protected JurisdiccionService? Service { get; set; }
    [Inject] protected NavigationManager? Nav { get; set; }
    [Inject] protected IJSRuntime? JS { get; set; }

    protected List<JurisdiccionDto>? Jurisdicciones;

    protected override async Task OnInitializedAsync()
    {
        Jurisdicciones = await Service!.ObtenerTodosAsync();
    }

    protected void Nueva() => Nav!.NavigateTo("/jurisdicciones/crear-editar");
    protected void Editar(int id) => Nav!.NavigateTo($"/jurisdicciones/crear-editar/{id}");

    protected async Task Eliminar(int id)
    {
        var confirmar = await JS!.InvokeAsync<bool>("confirm", "¿Eliminar esta jurisdicción?");
        if (confirmar)
        {
            await Service!.EliminarAsync(id);
            Jurisdicciones = await Service.ObtenerTodosAsync();
        }
    }
}
