using IurixBlazor.Shared.Dtos;
using IurixBlazor.Services;
using Microsoft.AspNetCore.Components;

namespace IurixBlazor.Pages
{
    public class DistribucionHonorariosBase : ComponentBase
    {
        [Inject] protected DistribucionHonorarioService DistribucionService { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;

        protected List<DistribucionHonorarioDto> Distribuciones { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            Distribuciones = await DistribucionService.ObtenerTodosAsync();
        }

        protected void CrearNueva()
        {
            Nav.NavigateTo("/distribucion-honorarios/editar");
        }

        protected void Editar(int id)
        {
            Nav.NavigateTo($"/distribucion-honorarios/editar/{id}");
        }

        protected async Task Eliminar(int id)
        {
            await DistribucionService.EliminarAsync(id);
            Distribuciones = await DistribucionService.ObtenerTodosAsync();
        }
    }
}
