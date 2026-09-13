using IurixBlazor.Services;
using IurixBlazor.Shared.Enums;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;

public class CrearEditarRubroBase : ComponentBase
{
    [Inject] protected RubroService Service { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected RubroDto Rubro { get; set; } = new();
    protected string[] Tipos = RubroTipos.Todos;

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            var existente = await Service.ObtenerPorIdAsync(Id.Value);
            if (existente != null)
                Rubro = existente;
        }
        else
        {
            // valores por defecto para crear
            Rubro = new RubroDto { Activo = true, Tipo = RubroTipos.Ingreso };
        }
    }

    protected async Task Guardar()
    {
        if (Id.HasValue)
        {
            await Service.ActualizarAsync(Id.Value, Rubro);
        }
        else
        {
            await Service.CrearAsync(new CrearRubroDto
            {
                Nombre = Rubro.Nombre ?? "",
                Tipo = Rubro.Tipo ?? RubroTipos.Ingreso,
                Activo = Rubro.Activo
            });
        }
        Nav.NavigateTo("/rubros");
    }

    protected void Cancelar() => Nav.NavigateTo("/rubros");
}
