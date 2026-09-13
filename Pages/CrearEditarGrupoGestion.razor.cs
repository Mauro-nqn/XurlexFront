using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class CrearEditarGrupoGestionBase : ComponentBase
{
    [Inject] protected GrupoGestionService Service { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected CrearGrupoGestionDto Grupo { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            var dto = await Service.ObtenerPorIdAsync(Id.Value);
            if (dto != null)
                Grupo = new CrearGrupoGestionDto { Nombre = dto.Nombre };
        }
    }

    protected async Task Guardar()
    {
        if (Id.HasValue)
            await Service.ActualizarAsync(Id.Value, Grupo);
        else
            await Service.CrearAsync(Grupo);

        Nav.NavigateTo("/gruposgestion");
    }

    protected async Task Eliminar()
    {
        if (Id.HasValue)
        {
            var confirmar = await JS.InvokeAsync<bool>("confirm", $"¿Eliminar {Grupo.Nombre}?");
            if (confirmar)
            {
                await Service.EliminarAsync(Id.Value);
                Nav.NavigateTo("/gruposgestion");
            }
        }
    }

    protected void Cancelar() => Nav.NavigateTo("/gruposgestion");
}
