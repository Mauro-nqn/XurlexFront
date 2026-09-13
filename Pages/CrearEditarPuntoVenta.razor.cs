using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;

public class CrearEditarPuntoVentaBase : ComponentBase
{
    [Inject] protected PuntoVentaService PuntoService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected PuntoVentaDto Punto { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            var existente = await PuntoService.ObtenerPorIdAsync(Id.Value);
            if (existente != null)
                Punto = existente;
        }
    }

    //protected async Task Guardar()
    //{
    //    if (Id.HasValue)
    //        await PuntoService.ActualizarAsync(Id.Value, new PuntoVentaDto { Numero = Punto.Numero, Descripcion = Punto.Descripcion });
    //    else
    //        await PuntoService.CrearAsync(new CrearPuntoVentaDto { Numero = Punto.Numero, Descripcion = Punto.Descripcion });

    //    Nav.NavigateTo("/puntos-venta");
    //}

    protected async Task Guardar()
    {
        if (Id.HasValue)
            await PuntoService.ActualizarAsync(Id.Value, Punto); // Usar el objeto existente con su Id
        else
            await PuntoService.CrearAsync(new CrearPuntoVentaDto
            {
                Numero = Punto.Numero,
                Descripcion = Punto.Descripcion
            });

        Nav.NavigateTo("/puntos-venta");
    }

    protected void Cancelar() => Nav.NavigateTo("/puntos-venta");
}
