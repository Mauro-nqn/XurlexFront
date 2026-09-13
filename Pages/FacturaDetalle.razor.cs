using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;

public class FacturaDetalleBase : ComponentBase
{
    [Parameter] public int Id { get; set; }
    [Inject] protected HttpClient Http { get; set; } = default!;

    // 👇 toma ?returnUrl=...
    [SupplyParameterFromQuery(Name = "returnUrl")]
    public string? ReturnUrl { get; set; }
 
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    [Inject] protected FacturaService FacturaService { get; set; } = default!;

    [Inject] private IvaAlicuotaService IvaAlicuotaService { get; set; } = default!;
    protected Dictionary<int, decimal> IvaAlicuotasCache { get; set; } = new();

    protected FacturaDto? factura;

    protected override async Task OnInitializedAsync()
    {
        factura = await FacturaService.ObtenerPorIdAsync(Id);

        if (factura != null)
        {
            // Obtener alícuotas necesarias (de detalles + iva discriminado)
            var idsAlicuotas = factura.Detalles.Select(d => d.IvaAlicuotaId)
                                .Union(factura.IvaDiscriminado.Select(i => i.AlicuotaId))
                                .Distinct();

            foreach (var id in idsAlicuotas)
            {
                var alicuota = await IvaAlicuotaService.ObtenerPorIdAsync(id);
                if (alicuota != null)
                    IvaAlicuotasCache[id] = alicuota.Porcentaje;
            }
        }
    }

    protected string ObtenerPorcentajeIva(int alicuotaId)
    {
        return IvaAlicuotasCache.TryGetValue(alicuotaId, out var porcentaje)
            ? porcentaje.ToString("0.##") // Formato con decimales si aplica
            : "—";
    }


    //esto a implementar en el backend solo para cambiar facturas punto venta 0
    //protected async Task GuardarCambios()
    //{
    //    await Http.PutAsJsonAsync($"api/factura/{Id}/observaciones", new { factura!.Observaciones });
    //}



    protected void Volver()
    {
        // si vino returnUrl, volver ahí; si no, a /facturas
        Navigation.NavigateTo(!string.IsNullOrWhiteSpace(ReturnUrl) ? ReturnUrl! : "/facturas");
    }


    //protected void Volver() => Navigation.NavigateTo("/facturas");
}
