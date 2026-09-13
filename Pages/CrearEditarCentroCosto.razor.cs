using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;

public class CrearEditarCentroCostoBase : ComponentBase
{
    [Inject] protected CentroCostoService Service { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected CentroCostoDto Centro { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            var existente = await Service.ObtenerPorIdAsync(Id.Value);
            if (existente != null) Centro = existente;
        }
        else
        {
            Centro = new CentroCostoDto { Activo = true };
        }
    }

    protected async Task Guardar()
    {
        if (Id.HasValue)
        {
            await Service.ActualizarAsync(Id.Value, Centro);
        }
        else
        {
            await Service.CrearAsync(new CrearCentroCostoDto
            {
                Nombre = Centro.Nombre ?? "",
                Activo = Centro.Activo
            });
        }
        Nav.NavigateTo("/centros-costo");
    }

    protected void Cancelar() => Nav.NavigateTo("/centros-costo");
}
