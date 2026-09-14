using IurixBlazor.Shared.Config;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Http.Headers;

namespace IurixBlazor.Services.Http;

// Scoped al circuito. No se registra como handler de IHttpClientFactory.
public sealed class ApiRequestSender : IApiRequestSender
{
    private readonly IHttpClientFactory _factory;
    private readonly IJSRuntime _js;
    private readonly ConfigService _config;

    public ApiRequestSender(IHttpClientFactory factory, IJSRuntime js, ConfigService config)
    {
        _factory = factory;
        _js = js;
        _config = config;
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string relativeUri,
        HttpContent? content = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // No permitir que un consumidor envie el bearer a otro destino.
        if (!Uri.TryCreate(relativeUri, UriKind.Relative, out var uri)
            || relativeUri.StartsWith('/') || relativeUri.Contains('\\'))
            throw new ArgumentException("Se requiere una ruta relativa de la API.", nameof(relativeUri));

        using var request = new HttpRequestMessage(method, uri) { Content = content };
        var mode = _config.AuthMode;
        if (mode != AuthMode.Off)
        {
            // Fuente transitoria del circuito actual. Nunca usar el TokenStore singleton
            // como fallback si JS no esta disponible (prerender o desconexion).
            var token = await _js.InvokeAsync<string?>(
                "localStorage.getItem", cancellationToken, new object?[] { "token" });

            if (string.IsNullOrWhiteSpace(token))
            {
                if (mode == AuthMode.Required)
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        ReasonPhrase = "Token requerido"
                    };
            }
            else
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        using var client = _factory.CreateClient("ApiTransport");
        // Contenido buffered: disponer este cliente no cierra el pool de la factory.
        return await client.SendAsync(request, cancellationToken);
    }
}
