using IurixBlazor.Services.Http;

using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace IurixBlazor.Services
{
    public class DistribucionHonorarioService
    {
        private readonly IApiRequestSender _sender;

        public DistribucionHonorarioService(IApiRequestSender sender)
        {
            _sender = sender;
        }

        public async Task<List<DistribucionHonorarioDto>> ObtenerTodosAsync()
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, "api/DistribucionHonorarios");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<DistribucionHonorarioDto>>()
                   ?? new List<DistribucionHonorarioDto>();
        }

        public async Task<DistribucionHonorarioDto?> ObtenerPorIdAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/DistribucionHonorarios/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DistribucionHonorarioDto>();
        }

        //public async Task CrearAsync(DistribucionHonorarioDto dto)
        //{
        //    var response = await _httpClient.PostAsJsonAsync("api/DistribucionHonorarios", dto);
        //    response.EnsureSuccessStatusCode();
        //}

        public async Task<DistribucionHonorarioDto> CrearAsync(DistribucionHonorarioDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Post, "api/DistribucionHonorarios", JsonContent.Create(dto));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DistribucionHonorarioDto>()
                   ?? throw new Exception("No se pudo crear la distribución.");
        }

   

        //public async Task ActualizarAsync(int id, DistribucionHonorarioDto dto)
        //{
        //    var json = JsonSerializer.Serialize(dto);
        //    System.Diagnostics.Debug.WriteLine("📤 JSON enviado: " + json);
        //    //var response = await _httpClient.PutAsJsonAsync($"api/DistribucionHonorarios/{id}", dto);
        //    var request = new HttpRequestMessage(HttpMethod.Patch, $"api/DistribucionHonorarios/{id}")
        //    {
        //        Content = JsonContent.Create(dto)
        //    };
        //    await _httpClient.SendAsync(request);
        //    //response.EnsureSuccessStatusCode();
        //}


        public async Task ActualizarAsync(int id, DistribucionHonorarioDto dto)
        {
            var json = JsonSerializer.Serialize(dto);
            System.Diagnostics.Debug.WriteLine("📤 JSON enviado: " + json);

            using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/DistribucionHonorarios/{id}", JsonContent.Create(dto));
            response.EnsureSuccessStatusCode(); // ✅ Asegura que no haya error silencioso
        }


        public async Task EliminarAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/DistribucionHonorarios/{id}");
            response.EnsureSuccessStatusCode();
        }




        //PARA DETALLES DE LA DISTRIBUCION HONORARIOS 


        public async Task<List<DistribucionDetalleDto>> ObtenerDetallesPorDistribucionAsync(int distribucionId)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/DistribucionDetalles/porDistribucion/{distribucionId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<DistribucionDetalleDto>>() ?? new();
        }

        public async Task<DistribucionDetalleDto> CrearDetalleAsync(DistribucionDetalleDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Post, "api/DistribucionDetalles", JsonContent.Create(dto));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DistribucionDetalleDto>() ?? dto;
        }

        //public async Task ActualizarDetalleAsync(int id, DistribucionDetalleDto dto)
        //{
        //    var response = await _httpClient.PatchAsJsonAsync($"api/DistribucionDetalles/{id}", dto);
        //    response.EnsureSuccessStatusCode();
        //}


        //public async Task ActualizarDetalleAsync(int id, DistribucionDetalleDto dto)
        //{
        //    var patchDto = new
        //    {
        //        Id = dto.Id,
        //        DistribucionHonorarioId = dto.DistribucionHonorarioId,
        //        UsuarioId = dto.UsuarioId,
        //        Porcentaje = dto.Porcentaje,
        //        Concepto = dto.Concepto
        //    };

        //    var response = await _httpClient.PatchAsJsonAsync($"api/DistribucionDetalles/{id}", patchDto);
        //    response.EnsureSuccessStatusCode();
        //}



        public async Task ActualizarDetalleAsync(int id, DistribucionDetalleDto dto)
        {
            // Convertir de DistribucionDetalleDto (lectura) a DistribucionDetallePatchDto (escritura)
            var patchDto = new DistribucionDetallePatchDto
            {
                Id = dto.Id,
                DistribucionHonorarioId = dto.DistribucionHonorarioId,
                UsuarioId = dto.UsuarioId,
                Porcentaje = dto.Porcentaje,
                Concepto = dto.Concepto
            };

            using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/DistribucionDetalles/{id}", JsonContent.Create(patchDto));
            response.EnsureSuccessStatusCode();
        }



        public async Task EliminarDetalleAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/DistribucionDetalles/{id}");
            response.EnsureSuccessStatusCode();
        }

    }
}
