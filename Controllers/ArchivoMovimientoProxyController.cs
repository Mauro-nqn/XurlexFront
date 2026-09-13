using IurixBlazor.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;


namespace IurixBlazor.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("files/archivomovimiento")]
    public class ArchivoMovimientoProxyController : ControllerBase
    {

        private readonly IHttpClientFactory _factory;
        private readonly ITokenStore _tokenStore;

        public ArchivoMovimientoProxyController(IHttpClientFactory factory, ITokenStore tokenStore)
        {
            _factory = factory;
            _tokenStore = tokenStore;
        }

        [HttpGet("{id:int}/ver")]
        public async Task<IActionResult> Ver(int id)
        {
            var token = await _tokenStore.GetAsync();
            if (string.IsNullOrWhiteSpace(token))
                return Unauthorized();

            var client = _factory.CreateClient("Api"); // BaseAddress -> backend :5000
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Pedimos INLINE al backend (tu endpoint /ver)
            var resp = await client.GetAsync($"/api/archivomovimiento/{id}/ver",
                HttpCompletionOption.ResponseHeadersRead);

            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode);

            var contentType = resp.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

            // Si el backend no manda filename, ponemos uno
            var fileName =
                resp.Content.Headers.ContentDisposition?.FileNameStar ??
                resp.Content.Headers.ContentDisposition?.FileName ??
                $"archivo-{id}";

            var stream = await resp.Content.ReadAsStreamAsync();

            // Forzar inline acá también (por las dudas)
            Response.Headers["Content-Disposition"] =
                $"inline; filename=\"{fileName}\"; filename*=UTF-8''{Uri.EscapeDataString(fileName)}";

            return File(stream, contentType);
        }

        [HttpGet("{id:int}/contenido")]
        public async Task<IActionResult> Descargar(int id)
        {
            var token = await _tokenStore.GetAsync();
            if (string.IsNullOrWhiteSpace(token))
                return Unauthorized();

            var client = _factory.CreateClient("Api");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync($"/api/archivomovimiento/{id}/contenido",
                HttpCompletionOption.ResponseHeadersRead);

            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode);

            var contentType = resp.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var fileName =
                resp.Content.Headers.ContentDisposition?.FileNameStar ??
                resp.Content.Headers.ContentDisposition?.FileName ??
                $"archivo-{id}";

            var stream = await resp.Content.ReadAsStreamAsync();

            // Descargar (attachment)
            return File(stream, contentType, fileName);
        }
    }

    }

