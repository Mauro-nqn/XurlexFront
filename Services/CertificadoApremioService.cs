using IurixBlazor.Shared.Iademandas; // o el namespace donde pongas el DTO resultado
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components.Forms;

using System.Net.Http.Headers;
using System.Net.Http.Json;

public class CertificadoApremioService
{
    private readonly HttpClient _api;

    public CertificadoApremioService(IHttpClientFactory factory)
    {
        _api = factory.CreateClient("Api");
    }

    public async Task<CertificadoApremioParseResultDto?> ParseAsync(
        IBrowserFile archivo,
        int procesoJudicialId,
        CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();

        var stream = archivo.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024, ct);
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

        content.Add(fileContent, "archivo", archivo.Name);

        var resp = await _api.PostAsync(
            $"api/certificados/apremio/parse?procesoJudicialId={procesoJudicialId}",
            content,
            ct);

        if (!resp.IsSuccessStatusCode)
        {
            var txt = await resp.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Error al procesar certificado: {txt}");
        }

        var dto = await resp.Content.ReadFromJsonAsync<CertificadoApremioParseResultDto>(cancellationToken: ct);

        return dto;
    }
}

