using IurixBlazor.Services.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class CasualService
    {
        private readonly IApiRequestSender _sender;

        public CasualService(IApiRequestSender sender)
        {
            _sender = sender;
        }


        public async Task<byte[]?> ObtenerContenidoAsync(int id)
        {
            using var response = await _sender.SendAsync(
                HttpMethod.Get,
                $"api/casual/{id}/contenido"
            );

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<List<CasualDocumentoDto>> ListarAsync()
        {
            using var response = await _sender.SendAsync(
                HttpMethod.Get,
                "api/casual/listar"
            );

            if (!response.IsSuccessStatusCode)
                return new();

            return await response.Content
                .ReadFromJsonAsync<List<CasualDocumentoDto>>()
                ?? new();
        }

        public async Task<CasualDocumentoDto?> ImportarAsync(
            Stream stream,
            string nombreArchivo)
        {
            using var form = new MultipartFormDataContent();

            using var archivo = new StreamContent(stream);

            archivo.Headers.ContentType =
                new MediaTypeHeaderValue(
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                );

            form.Add(
                archivo,
                "archivo",
                nombreArchivo
            );

            using var response = await _sender.SendAsync(
                HttpMethod.Post,
                "api/casual/importar",
                form
            );

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadFromJsonAsync<CasualDocumentoDto>();
        }

        public async Task<bool> EliminarAsync(int id)
        {
            using var response = await _sender.SendAsync(
                HttpMethod.Delete,
                $"api/casual/{id}"
            );

            return response.IsSuccessStatusCode;
        }



        public async Task<bool> GuardarAsync(
    int id,
    byte[] contenido)
        {
            using var content =
                new ByteArrayContent(contenido);

            content.Headers.ContentType =
                new MediaTypeHeaderValue(
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                );

            using var response = await _sender.SendAsync(
                HttpMethod.Put,
                $"api/casual/{id}/contenido",
                content
            );

            return response.IsSuccessStatusCode;
        }




        public async Task<CasualDocumentoDto?> GuardarComoAsync(
    string titulo,
    byte[] contenido)
        {
            using var content =
                new ByteArrayContent(contenido);

            content.Headers.ContentType =
                new MediaTypeHeaderValue(
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                );

            using var response = await _sender.SendAsync(
                HttpMethod.Post,
                $"api/casual/guardar-como?titulo={Uri.EscapeDataString(titulo)}",
                content
            );

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadFromJsonAsync<CasualDocumentoDto>();
        }


    }

    public class CasualDocumentoDto
    {
        public int Id { get; set; }

        public string Titulo { get; set; } = "";

        public DateTime FechaCreacion { get; set; }

        public string Formato { get; set; } = "";
    }
}