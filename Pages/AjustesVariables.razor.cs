using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;


public class AjustesVariablesBase : ComponentBase
{
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    [Inject] protected AjusteVariableService AjusteVariableSrv {get; set;} = default!;

    protected List<AjusteVariableDto> items = new();

        protected override async Task OnInitializedAsync()
        {
            items = await AjusteVariableSrv.ObtenerTodosAsync() ?? new();
        }

        protected void Nuevo() => Nav.NavigateTo("/ajustes-variables/editar");
        protected void Editar(int id) => Nav.NavigateTo($"/ajustes-variables/editar/{id}");

        protected async Task Eliminar(int id)
        {
            if (await JS.InvokeAsync<bool>("confirm", "¿Eliminar variable?"))
            {
                await AjusteVariableSrv.EliminarAsync(id);
                items = await AjusteVariableSrv.ObtenerTodosAsync() ?? new();
                StateHasChanged();
            }
        }
    }

