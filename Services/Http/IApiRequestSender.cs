namespace IurixBlazor.Services.Http;

public interface IApiRequestSender
{
    // El emisor dispone el request y su contenido; el consumidor dispone la respuesta.
    Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string relativeUri,
        HttpContent? content = null,
        CancellationToken cancellationToken = default);
}
