using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace IurixBlazor.Services
{
    public class DistribucionHonorarioService
    {
        private readonly HttpClient _httpClient;

        public DistribucionHonorarioService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient = factory.CreateClient("Api");
        }

        public async Task<List<DistribucionHonorarioDto>> ObtenerTodosAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<DistribucionHonorarioDto>>("api/DistribucionHonorarios")
                   ?? new List<DistribucionHonorarioDto>();
        }

        public async Task<DistribucionHonorarioDto?> ObtenerPorIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<DistribucionHonorarioDto>($"api/DistribucionHonorarios/{id}");
        }

        //public async Task CrearAsync(DistribucionHonorarioDto dto)
        //{
        //    var response = await _httpClient.PostAsJsonAsync("api/DistribucionHonorarios", dto);
        //    response.EnsureSuccessStatusCode();
        //}

        public async Task<DistribucionHonorarioDto> CrearAsync(DistribucionHonorarioDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/DistribucionHonorarios", dto);
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

            var request = new HttpRequestMessage(HttpMethod.Patch, $"api/DistribucionHonorarios/{id}")
            {
                Content = JsonContent.Create(dto)
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode(); // ✅ Asegura que no haya error silencioso
        }


        public async Task EliminarAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/DistribucionHonorarios/{id}");
            response.EnsureSuccessStatusCode();
        }




        //PARA DETALLES DE LA DISTRIBUCION HONORARIOS 


        public async Task<List<DistribucionDetalleDto>> ObtenerDetallesPorDistribucionAsync(int distribucionId)
        {
            var response = await _httpClient.GetAsync($"api/DistribucionDetalles/porDistribucion/{distribucionId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<DistribucionDetalleDto>>() ?? new();
        }

        public async Task<DistribucionDetalleDto> CrearDetalleAsync(DistribucionDetalleDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/DistribucionDetalles", dto);
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

            var response = await _httpClient.PatchAsJsonAsync($"api/DistribucionDetalles/{id}", patchDto);
            response.EnsureSuccessStatusCode();
        }



        public async Task EliminarDetalleAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/DistribucionDetalles/{id}");
            response.EnsureSuccessStatusCode();
        }

    }
}
