using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;

public class CrearEditarTipoComprobanteBase : ComponentBase
{
    [Inject] protected TipoComprobanteService TipoService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    [Parameter] public int? Codigo { get; set; }

    protected TipoComprobanteDto Tipo { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        if (Codigo.HasValue)
        {
            var existente = await TipoService.ObtenerPorCodigoAsync(Codigo.Value);
            if (existente != null)
                Tipo = existente;
        }
    }

    protected async Task Guardar()
    {
        var dto = new CrearTipoComprobanteDto
        {
            Id = Tipo.Id,
            Descripcion = Tipo.Descripcion,
            Letra = Tipo.Letra
        };

        if (Codigo.HasValue)
            await TipoService.ActualizarAsync(Codigo.Value, dto);
        else
            await TipoService.CrearAsync(dto);

        Nav.NavigateTo("/tipo-comprobantes");
    }

    protected void Cancelar() => Nav.NavigateTo("/tipo-comprobantes");
}
