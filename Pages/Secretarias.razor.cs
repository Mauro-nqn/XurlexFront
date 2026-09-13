using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class SecretariaBase : ComponentBase
{
    [Inject] protected SecretariaService? Service { get; set; }
    [Inject] protected NavigationManager? Nav { get; set; }
    [Inject] protected IJSRuntime? JS { get; set; }

    protected List<SecretariaDto>? Secretarias;

    protected override async Task OnInitializedAsync()
    {
        Secretarias = await Service!.ObtenerTodosAsync();
    }

    protected void Nueva() => Nav!.NavigateTo("/secretarias/crear-editar");
    protected void Editar(int id) => Nav!.NavigateTo($"/secretarias/crear-editar/{id}");

    protected async Task Eliminar(int id)
    {
        var confirmar = await JS!.InvokeAsync<bool>("confirm", "¿Eliminar esta secretaría?");
        if (confirmar)
        {
            await Service!.EliminarAsync(id);
            Secretarias = await Service.ObtenerTodosAsync();
        }
    }
}
