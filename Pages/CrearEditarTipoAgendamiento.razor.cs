using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;

public class CrearEditarTipoAgendamientoBase : ComponentBase
{
    [Inject] protected TipoAgendamientoService TipoService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected TipoAgendamientoDto Tipo { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            var existente = await TipoService.ObtenerPorIdAsync(Id.Value);
            if (existente != null)
                Tipo = existente;
        }
    }

    protected async Task Guardar()
    {
        if (Id.HasValue)
            await TipoService.ActualizarAsync(Id.Value, Tipo);
        else
            await TipoService.CrearAsync(new CrearTipoAgendamientoDto
            {
                Nombre = Tipo.Nombre
            });

        Nav.NavigateTo("/tipo-agendamiento");
    }

    protected void Cancelar() => Nav.NavigateTo("/tipo-agendamiento");
}
