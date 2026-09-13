using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class JuzgadoBase : ComponentBase
{
    [Inject] protected JuzgadoService? Service { get; set; }
    [Inject] protected NavigationManager? Nav { get; set; }
    [Inject] protected IJSRuntime? JS { get; set; }

    protected List<JuzgadoDto>? Juzgados;

    protected override async Task OnInitializedAsync()
    {
        Juzgados = await Service!.ObtenerTodosAsync();
    }

    protected void Nuevo() => Nav!.NavigateTo("/juzgados/crear-editar");
    protected void Editar(int id) => Nav!.NavigateTo($"/juzgados/crear-editar/{id}");

    protected async Task Eliminar(int id)
    {
        var confirmar = await JS!.InvokeAsync<bool>("confirm", "¿Eliminar este juzgado?");
        if (confirmar)
        {
            await Service!.EliminarAsync(id);
            Juzgados = await Service.ObtenerTodosAsync();
        }
    }
}
