using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using static Org.BouncyCastle.Math.EC.ECCurve;

public class ArchivoMovimientoService
{
    private readonly HttpClient _http;

    public ArchivoMovimientoService(IHttpClientFactory factory, ConfigService config)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _http = factory.CreateClient("Api");
    }


    /* =========================
   URLs públicas (para <a>)
   ========================= */

    //public string UrlDescarga(int archivoId)
    //    => $"/api/archivomovimiento/{archivoId}/contenido";

    //public string UrlVerInline(int archivoId)
    //    => $"/api/archivomovimiento/{archivoId}/ver";

    //public string UrlVerInline(int archivoId)
    //{
    //    var baseUri = _http.BaseAddress ?? throw new InvalidOperationException("Api BaseAddress no configurada");
    //    return new Uri(baseUri, $"api/archivomovimiento/{archivoId}/ver").ToString();
    //}

    //public string UrlDescarga(int archivoId)
    //{
    //    var baseUri = _http.BaseAddress ?? throw new InvalidOperationException("Api BaseAddress no configurada");
    //    return new Uri(baseUri, $"api/archivomovimiento/{archivoId}/contenido").ToString();
    //}

    public string UrlVerInlineUi(int archivoId) => $"/files/archivomovimiento/{archivoId}/ver";
    public string UrlDescargaUi(int archivoId) => $"/files/archivomovimiento/{archivoId}/contenido";




    public async Task<ArchivoTtsDto?> ObtenerArchivoTtsAsync(int archivoId)
    {
        return await _http.GetFromJsonAsync<ArchivoTtsDto>(
            $"api/archivos-movimiento/{archivoId}/tts");
    }



    public async Task<List<ArchivoMovimientoDto>> ObtenerAdjuntosAsync(int movimientoId)
    {
        return await _http.GetFromJsonAsync<List<ArchivoMovimientoDto>>(
            $"api/archivomovimiento/por-movimiento/{movimientoId}")
            ?? new();
    }

    public async Task<byte[]?> DescargarContenidoAsync(int archivoId)
    {
        try
        {
            var url = $"api/archivomovimiento/{archivoId}/contenido";
            return await _http.GetByteArrayAsync(url); // ✅ para obtener raw file
        }
        catch
        {
            return null;
        }
    }

    // Opcional si devolvés base64
    public async Task<string?> ObtenerContenidoBase64Async(int archivoId)
    {
        try
        {
            var bytes = await DescargarContenidoAsync(archivoId);
            return bytes is not null ? Convert.ToBase64String(bytes) : null;
        }
        catch
        {
            return null;
        }
    }




    // NUEVO: lista por movimiento (sólo metadata)
    public async Task<List<ArchivoTemporalDto>?> ListarPorMovimientoAsync(int movimientoId)
        => await _http.GetFromJsonAsync<List<ArchivoTemporalDto>>(
               $"api/archivomovimiento/por-movimiento/{movimientoId}");





}
