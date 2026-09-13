using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class CrearEditarAjusteVariableBase : ComponentBase
    {
    [Inject] AjusteVariableService AjusteVariableSrv { get; set; } = default!;
    [Inject] NavigationManager Nav { get; set; } = default!;

    [Parameter] public int? Id { get; set; }
    protected AjusteVariableDto vm = new() { Clave = "", Nombre = "", ValorActual = 0, Activa = true };

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            var x = await AjusteVariableSrv.ObtenerPorIdAsync(Id.Value);
            if (x != null) vm = x;
        }
    }

    protected async Task Guardar()
    {
        if (Id.HasValue)
        {
            await AjusteVariableSrv.ActualizarAsync(Id.Value, new PatchAjusteVariableDto
            {
                Nombre = vm.Nombre,
                ValorActual = vm.ValorActual,
                Activa = vm.Activa
            });
        }
        else
        {
            await AjusteVariableSrv.CrearAsync(new CrearAjusteVariableDto
            {
                Clave = (vm.Clave ?? "").Trim().ToUpperInvariant(),
                Nombre = vm.Nombre ?? "",
                ValorActual = vm.ValorActual,
                Activa = vm.Activa
            });
        }
        Nav.NavigateTo("/ajustes-variables");
    }
    protected void Cancelar() => Nav.NavigateTo("/ajustes-variables");
}



    

