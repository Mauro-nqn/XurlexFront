using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;

namespace IurixBlazor.Shared.Services
{
    public class CollaboraService
    {
        private readonly IApiRequestSender _sender;

        public CollaboraService(IApiRequestSender sender)
        {
            _sender = sender;
        }

        public async Task<List<CollaboraEscritoDto>> ListarAsync()
        {
            using var response = await _sender.SendAsync(
                HttpMethod.Get,
                "api/collabora/listar"
            );

            if (!response.IsSuccessStatusCode)
                return new();

            return await response.Content
                .ReadFromJsonAsync<List<CollaboraEscritoDto>>()
                ?? new();
        }

        public async Task<CollaboraEscritoDto?> NuevoAsync(string titulo)
        {
            var url =
                $"api/collabora/nuevo?titulo={Uri.EscapeDataString(titulo)}";

            using var response = await _sender.SendAsync(
                HttpMethod.Post,
                url
            );

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadFromJsonAsync<CollaboraEscritoDto>();
        }

        public async Task<bool> EliminarAsync(int id)
        {
            using var response = await _sender.SendAsync(
                HttpMethod.Delete,
                $"api/collabora/{id}"
            );

            return response.IsSuccessStatusCode;
        }

        public async Task<CollaboraEscritoDto?> ImportarAsync(
            Stream stream,
            string nombreArchivo)
        {
            using var contenido = new MultipartFormDataContent();

            using var archivo = new StreamContent(stream);

            archivo.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                );

            contenido.Add(
                archivo,
                "archivo",
                nombreArchivo
            );

            using var response = await _sender.SendAsync(
                HttpMethod.Post,
                "api/collabora/importar",
                contenido
            );

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadFromJsonAsync<CollaboraEscritoDto>();
        }






        public async Task<CollaboraEscritoDto?> ImportarAsync(
    Stream stream,
    string nombreArchivo,
    string contentType)
        {
            using var form = new MultipartFormDataContent();

            using var contenidoArchivo = new StreamContent(stream);

            contenidoArchivo.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(contentType)
                        ? "application/octet-stream"
                        : contentType
                );

            form.Add(
                contenidoArchivo,
                "archivo",
                nombreArchivo
            );

            using var response = await _sender.SendAsync(
                HttpMethod.Post,
                "api/collabora/importar",
                form
            );

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadFromJsonAsync<CollaboraEscritoDto>();
        }



        public async Task<CollaboraEscritoDto?> GuardarComoAsync(
            int id,
            string titulo)
        {
            var dto = new
            {
                titulo
            };

            using var response = await _sender.SendAsync(
                HttpMethod.Post,
                $"api/collabora/{id}/guardar-como",
                JsonContent.Create(dto)
            );

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadFromJsonAsync<CollaboraEscritoDto>();
        }





    }







}