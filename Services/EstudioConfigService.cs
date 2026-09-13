using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IurixBlazor.Services;

public class EstudioConfigService
{
    private readonly HttpClient _http;

    public EstudioConfigService(IHttpClientFactory factory, ConfigService config)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _http = factory.CreateClient("Api");
    }

    // GET api/estudio-config
    public async Task<EstudioConfigDto?> ObtenerAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<EstudioConfigDto>("api/estudioconfig");
        }
        catch (HttpRequestException ex)
        {
            // 404 -> null; otros -> throw
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            throw;
        }
    }

    // POST api/estudio-config
    public async Task CrearAsync(CrearEstudioConfigDto dto)
    {
        var resp = await _http.PostAsJsonAsync("api/estudioconfig", dto);
        resp.EnsureSuccessStatusCode();
    }

    // PATCH api/estudio-config
    public async Task PatchAsync(ActualizarEstudioConfigDto dto)
    {
        using var content = JsonContent.Create(dto);
        var resp = await _http.PatchAsync("api/estudioconfig", content);
        resp.EnsureSuccessStatusCode();
    }

    // PUT api/estudio-config (opcional)
    public async Task PutAsync(CrearEstudioConfigDto dto)
    {
        var resp = await _http.PutAsJsonAsync("api/estudioconfig", dto);
        resp.EnsureSuccessStatusCode();
    }

    // POST api/estudio-config/logo (opcional si querés multipart)
    public async Task SubirLogoAsync(Stream stream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "archivo", fileName);

        var resp = await _http.PostAsync("api/estudio-config/logo", content);
        resp.EnsureSuccessStatusCode();
    }
}

