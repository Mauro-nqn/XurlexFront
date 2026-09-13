using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;

namespace IurixBlazor.Pages
{
    public class FacturaPreviewBase : ComponentBase
    {
        [Parameter] public int FacturaId { get; set; }

        [Inject] protected FacturaService FacturaService { get; set; } = default!;

        protected FacturaImpresionDto? factura;



        protected override async Task OnInitializedAsync()
        {
            // 🔹 Cargar datos reales desde el servicio
            factura = await FacturaService.ObtenerFacturaParaImpresionAsync(FacturaId);

            if (factura == null)
            {
                Console.WriteLine($"⚠️ No se encontró la factura con ID {FacturaId}. Cargando datos de ejemplo...");

                // 🔹 Fallback: Datos de prueba para preview
                factura = new FacturaImpresionDto
                {
                    NombreTitularEmisor = "JOHN DOE",
                    CuitEmisor = "11234567899",
                    CondicionFiscalEmisor = "Responsable Monotributo",
                    PuntoVentaNumero = 1,
                    Numero = 111,
                    Fecha = DateTime.Now,
                    IIBBEmisor = "Exento",
                    InicioActividadesEmisor = DateTime.Now.AddYears(-5).ToString("dd/MM/yyyy"),
                    PersonaNombre = "JANE",
                    PersonaApellido = "DOE",
                    PersonaCUIT = "20123456789",
                    PersonaDomicilio = "Islas Malvinas 574",
                    PersonaProvincia = "Neuquen",
                    PersonaRazonSocial = "",
                    CondicionIVAReceptorId = 1,
                    Neto = 10000,
                    Iva = 2100,
                    Total = 12100,
                    QRUrl = "https://via.placeholder.com/100",
                    CAE = "12345678901234",
                    CAEFchVto = DateTime.Now.AddDays(10),
                    Detalles = new List<FacturaDetalleDto>
                    {
                        new FacturaDetalleDto { Descripcion = "Servicio Profesional", Cantidad = 1, PrecioUnitario = 10000, IvaPorcentaje = 21 }
                    }
                };
            }
        }
    }
}
