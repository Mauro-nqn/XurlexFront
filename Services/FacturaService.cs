using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace IurixBlazor.Services
{
    public class FacturaService
    {
        private readonly HttpClient _httpClient;

        public FacturaService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient = factory.CreateClient("Api");
        }

        public async Task<List<FacturaDto>> ObtenerTodasAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<FacturaDto>>("api/factura") ?? new();
        }

        public async Task<FacturaDto?> ObtenerPorIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<FacturaDto>($"api/factura/{id}");
        }




        public Task<List<FacturaListItemDto>?> BuscarAsync(
            DateOnly? desde, DateOnly? hasta, int? personaId, int? usuarioId, int? puntoVentaId, string? numero, string? operacion)
        {
            var qs = new List<string>();
            if (desde.HasValue) qs.Add($"desde={desde:yyyy-MM-dd}");
            if (hasta.HasValue) qs.Add($"hasta={hasta:yyyy-MM-dd}");
            if (personaId is > 0) qs.Add($"personaId={personaId}");
            if (usuarioId is > 0) qs.Add($"usuarioId={usuarioId}");
            if (puntoVentaId is > 0) qs.Add($"puntoVentaId={puntoVentaId}");
            if (!string.IsNullOrWhiteSpace(numero)) qs.Add($"numero={Uri.EscapeDataString(numero!)}");
            if (!string.IsNullOrWhiteSpace(operacion)) qs.Add($"operacion={Uri.EscapeDataString(operacion)}");

            var url = "api/Factura/buscar" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");
            return _httpClient.GetFromJsonAsync<List<FacturaListItemDto>>(url);
        }


        //Metodo para obtener facturas para nota de credito y debito
        public async Task<List<FacturaDto>> ObtenerComprobantesAsociablesPorPersonaAsync(
    int personaId,
    string operacion)
        {
            return await _httpClient.GetFromJsonAsync<List<FacturaDto>>(
                $"api/Factura/asociables/persona/{personaId}?operacion={operacion}")
                ?? new List<FacturaDto>();
        }




        public async Task<FacturaImpresionDto?> ObtenerFacturaParaImpresionAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<FacturaImpresionDto>($"api/factura/{id}/impresion");
        }






        public async Task CrearAsync(FacturaDto factura)
        {
            //  Usado solo para PuntoVentaId=0 (sin AFIP)
            var response = await _httpClient.PostAsJsonAsync("api/factura", factura);
            response.EnsureSuccessStatusCode();
        }





        //public async Task<FacturaDto?> CrearSoloDbAsync(FacturaDto factura)
        //{
        //    var resp = await _httpClient.PostAsJsonAsync("api/factura/solo-db", factura);

        //    if (resp.StatusCode == System.Net.HttpStatusCode.Conflict)
        //    {
        //        var msg = await resp.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        //        throw new InvalidOperationException(msg?["message"] ?? "Conflicto al crear factura.");
        //    }

        //    resp.EnsureSuccessStatusCode();
        //    return await resp.Content.ReadFromJsonAsync<FacturaDto>();
        //}





        public async Task<FacturaDto?> CrearSoloDbAsync(CrearFacturaDto crear)
        {
            var resp = await _httpClient.PostAsJsonAsync("api/factura/solo-db", crear);
            if (resp.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                var msg = await resp.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                throw new InvalidOperationException(msg?["message"] ?? "Conflicto al crear factura.");
            }
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<FacturaDto>();
        }




        //public async Task CrearSoloDbAsync(FacturaDto factura)
        //{
        //    var facturaDb = new FacturaDto
        //    {
        //        Id = factura.Id,
        //        Numero = factura.Numero,
        //        PuntoVentaId = factura.PuntoVentaId,
        //        PuntoVentaNumero = factura.PuntoVentaNumero,
        //        PuntoVentaDescripcion = factura.PuntoVentaDescripcion,
        //        TipoComprobanteId = factura.TipoComprobanteId,
        //        TipoComprobanteDescripcion = factura.TipoComprobanteDescripcion,
        //        TipoComprobanteLetra = factura.TipoComprobanteLetra,
        //        Fecha = factura.Fecha,
        //        PersonaId = factura.PersonaId,
        //        PersonaNombre = factura.PersonaNombre,
        //        PersonaCUIT = factura.PersonaCUIT,
        //        CondicionIVAReceptorId = factura.CondicionIVAReceptorId,
        //        Neto = factura.Neto,
        //        Iva = factura.Iva,
        //        Total = factura.Total,
        //        Observaciones = factura.Observaciones,
        //        CbteTipo = factura.CbteTipo,
        //        DocTipo = factura.DocTipo,
        //        DocNro = factura.DocNro,
        //        MonId = factura.MonId,
        //        MonCotiz = factura.MonCotiz,
        //        Concepto = factura.Concepto,
        //        UsuarioId = factura.UsuarioId,
        //        CAE = factura.CAE,
        //        CAEFchVto = factura.CAEFchVto,
        //        Detalles = factura.Detalles,
        //        IvaDiscriminado = factura.IvaDiscriminado //  Solo esto
        //    };

        //    var response = await _httpClient.PostAsJsonAsync("api/factura", facturaDb);
        //    response.EnsureSuccessStatusCode();
        //}



        public async Task EliminarAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/factura/{id}");

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                // DTO simple para leer el mensaje del backend
                var body = await response.Content.ReadFromJsonAsync<ErrorRespuestaDto>();
                var msg = body?.mensaje
                          ?? "No se puede eliminar la factura porque tiene registros asociados.";

                throw new InvalidOperationException(msg);
            }

            // Otros códigos de error → HttpRequestException
            response.EnsureSuccessStatusCode();
        }

        public class ErrorRespuestaDto
        {
            public string? mensaje { get; set; }
        }

    }
}
