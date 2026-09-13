using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace iurixGo.Client.Pages.FacturaImpresion
{
    public class FacturaImpresionBase : ComponentBase
    {
        [Parameter] public int FacturaId { get; set; }
        [Inject] protected FacturaService FacturaService { get; set; } = default!;
        [Inject] protected IJSRuntime JS { get; set; } = default!;

        protected FacturaImpresionDto? factura;

        protected override async Task OnInitializedAsync()
        {
            factura = await FacturaService.ObtenerFacturaParaImpresionAsync(FacturaId);

            if (factura == null)
                Console.WriteLine("⚠️ No se encontró la factura para impresión.");
        }

        protected async Task Imprimir()
        {
            await JS.InvokeVoidAsync("window.print");
        }
    }
}
