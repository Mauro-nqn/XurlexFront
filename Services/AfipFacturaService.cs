using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;   
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace IurixBlazor.Services
    {
        public class AfipFacturaService
        {
            private readonly HttpClient _httpClient;
            private readonly IJSRuntime _js;

        public AfipFacturaService(IHttpClientFactory factory, ConfigService config, IJSRuntime js)
            {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient = factory.CreateClient("Api");

            _js = js;
            }

            /// <summary>
            /// Solicita CAE para una factura a través del backend.
            /// </summary>
            public async Task<FeCAEResponseDto> SolicitarCaeAsync(FacturaDto factura)
            {

            // 🔍 Serializamos el JSON para inspección
            var json = System.Text.Json.JsonSerializer.Serialize(factura, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            System.Diagnostics.Debug.WriteLine("📦 JSON enviado a backend:\n" + json);

            // ✅ O mostrar en un modal/alert en Blazor para pruebas:
            await _js.InvokeVoidAsync("console.log", "Factura JSON:", json);


            var response = await _httpClient.PostAsJsonAsync("api/afipfactura/solicitar-cae", factura);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<FeCAEResponseDto>()
                       ?? throw new Exception("Error al obtener respuesta de AFIP.");
            }

            /// <summary>
            /// Obtiene el token y sign de AFIP desde el backend.
            /// </summary>
            public async Task<AuthResponseDto> ObtenerTokenAsync()
            {
                var result = await _httpClient.GetFromJsonAsync<AuthResponseDto>("api/afipauth/auth");
                if (result == null)
                    throw new Exception("No se pudo obtener token de AFIP.");
                return result;
            }
        }
    }


