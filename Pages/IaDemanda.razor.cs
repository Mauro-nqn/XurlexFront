using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Iademandas;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Net.Http;
using System.Text.Json;

namespace IurixBlazor.Pages
{
    public class IaDemandaBase : ComponentBase
    {
        // Inyectamos el factory (no por constructor)
        [Inject] protected IHttpClientFactory HttpClientFactory { get; set; } = default!;
        [Inject] protected IJSRuntime JS { get; set; } = default!;


        // Usamos una propiedad helper para obtener el cliente "Api"
        protected HttpClient ApiClient => HttpClientFactory.CreateClient("Api");

        public DemandaEjecutivaRequestDto modelo = new()
        {
            Jurisdiccion = "Neuquén",
            TipoCredito = "Apremio municipal",
            Abogado = new AbogadoDto
            {
                Nombre = "Agustina Heavey",
                Matricula = "1880",
                Cuit = "27-30226664-1",
                CondicionIva = "Responsable Inscripta",
                DomicilioProcesal = "Islas Malvinas 574, Neuquén",
                DomicilioElectronico = "nq1880"
            },
            Cliente = new ParteDto
            {
                Nombre = "Juan Pérez",
                Domicilio = "Neuquén capital",
                Cuit = "20-12345678-9"
            },
            Deudor = new ParteDto
            {
                Nombre = "ACME S.A.",
                Domicilio = "Catriel 1080",
                Cuit = "30-11223344-9",
                Ciudad = "Zapala"
            },
            Certificado = new CertificadoDeudaDto
            {
                Monto = 150000.50m,
                FechaCertificado = "2025-10-10",
                FechaLiquidacion = "2025-10-10",
                Numero = "146",
                OrganismoEmisor = "Subsecretaría de Administración Municipal de Ingresos Públicos",
                Dependencia = "Secretaría de Economía y Hacienda de la Municipalidad de Neuquén"
            }
        };

        public DemandaEjecutivaResponseDto? respuesta;

        public async Task GenerarDemanda()
        {
            // Convertimos el modelo a JSON para ver EXACTAMENTE lo que mandamos
            var json = JsonSerializer.Serialize(modelo, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            // Log en consola del navegador //en desarrollo sacar en produccion
            await JS.InvokeVoidAsync("console.log", "JSON enviado a la API:", json);

            var resp = await ApiClient.PostAsJsonAsync("api/ia/demandas/ejecutiva", modelo);

            if (resp.IsSuccessStatusCode)
            {
                respuesta = await resp.Content.ReadFromJsonAsync<DemandaEjecutivaResponseDto>();
            }
            else
            {
                var body = await resp.Content.ReadAsStringAsync();
                respuesta = new DemandaEjecutivaResponseDto
                {
                    BorradorDemanda = $"Error llamando al backend: {resp.StatusCode}\n{body}"
                };
            }
        }


    }
}
