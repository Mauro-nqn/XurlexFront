using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class CrearEditarCaracterIntervencionBase : ComponentBase
{
    [Inject] protected CaracterIntervencionService? Service { get; set; }
    [Inject] protected NavigationManager? Nav { get; set; }
    [Inject] protected IJSRuntime? JS { get; set; }

    [Parameter] public int? Id { get; set; }
    protected CaracterIntervencionDto Caracter { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            var dto = await Service!.ObtenerPorIdAsync(Id.Value);
            if (dto != null) Caracter = dto;
        }
    }

    protected async Task Guardar()
    {
        if (Id.HasValue)
            await Service!.ActualizarAsync(Caracter.Id, Caracter);
        else
            await Service!.CrearAsync(Caracter);

        Nav!.NavigateTo("/caracteresintervencion");
    }

    protected async Task Eliminar()
    {
        if (Id.HasValue)
        {
            var confirmar = await JS!.InvokeAsync<bool>("confirm", $"¿Eliminar {Caracter.Nombre}?");
            if (confirmar)
            {
                await Service!.EliminarAsync(Id.Value);
                Nav!.NavigateTo("/caracteresintervencion");
            }
        }
    }

    protected void Cancelar() => Nav!.NavigateTo("/caracteresintervencion");
}
