using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace IurixBlazor.Pages
{
    public partial class EscritosBase : ComponentBase
    {
        [Inject] private EscritoService EscritoService { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;


        protected List<EscritoDto> Escritos = new();
        protected string FiltroActual { get; set; } = string.Empty;


        protected bool IsLoading { get; set; } = true;
        protected bool PuedeCargarMas { get; set; } = false;

        private int PaginaActual = 1;
        private const int TamanioPagina = 20;

        protected override async Task OnInitializedAsync()
        {
            await CargarEscritos(true);
        }

        protected async Task CargarEscritos(bool resetear)
        {
            IsLoading = true;

            if (resetear)
            {
                PaginaActual = 1;
                Escritos.Clear();
            }

            var resultado = await EscritoService.ObtenerEscritosAsync(PaginaActual, TamanioPagina, FiltroActual);

            if (resultado.Items.Any())
            {
                Escritos.AddRange(resultado.Items);
                PaginaActual++;
                PuedeCargarMas = resultado.Total > Escritos.Count;
            }
            else
            {
                PuedeCargarMas = false;
            }

            IsLoading = false;
        }

        protected async Task CargarMas()
        {
            await CargarEscritos(false);
        }

        protected async Task AbrirEscrito(int id)
        {
            var escrito = await EscritoService.ObtenerEscritoPorIdAsync(id);
            if (escrito != null)
            {
                Navigation.NavigateTo($"/editor/{id}");
            }
        }

        protected async Task EliminarEscrito(int id)
        {
            var confirmado = await JSRuntime.InvokeAsync<bool>("confirm", $"¿Eliminar este escrito?");
            if (!confirmado) return;

            var exito = await EscritoService.EliminarEscritoAsync(id);
            if (exito)
            {
                Escritos.RemoveAll(e => e.Id == id);
            }
        }

        protected void NuevoEscrito()
        {
            Navigation.NavigateTo("/editor");
        }

        protected async Task OnFiltroChanged(ChangeEventArgs e)
        {
            FiltroActual = e.Value?.ToString() ?? string.Empty;
            await CargarEscritos(true);


        }
    }
}
