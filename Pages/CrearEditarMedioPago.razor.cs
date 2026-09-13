using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;

public class CrearEditarMedioPagoBase : ComponentBase
{
    [Inject] protected MedioPagoService Service { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected MedioPagoDto Medio { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            var existente = await Service.ObtenerPorIdAsync(Id.Value);
            if (existente != null) Medio = existente;
        }
        else
        {
            Medio = new MedioPagoDto { Activo = true };
        }
    }

    protected async Task Guardar()
    {
        if (Id.HasValue)
        {
            await Service.ActualizarAsync(Id.Value, Medio);
        }
        else
        {
            await Service.CrearAsync(new CrearMedioPagoDto
            {
                Nombre = Medio.Nombre ?? "",
                Activo = Medio.Activo 
            });
        }
        Nav.NavigateTo("/medios-pago");
    }

    protected void Cancelar() => Nav.NavigateTo("/medios-pago");
}
