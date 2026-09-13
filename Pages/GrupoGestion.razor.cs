using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class GrupoGestionBase : ComponentBase
{
    [Inject] protected GrupoGestionService Service { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected List<GrupoGestionDto>? Grupos;

    protected override async Task OnInitializedAsync()
    {
        Grupos = await Service.ObtenerTodosAsync();
    }

    protected void Nuevo() => Nav.NavigateTo("/gruposgestion/crear-editar");

    protected void Editar(int id) => Nav.NavigateTo($"/gruposgestion/crear-editar/{id}");

    protected async Task Eliminar(int id)
    {
        var confirmar = await JS.InvokeAsync<bool>("confirm", "¿Eliminar este grupo?");
        if (confirmar)
        {
            await Service.EliminarAsync(id);
            Grupos = await Service.ObtenerTodosAsync();
        }
    }
}
