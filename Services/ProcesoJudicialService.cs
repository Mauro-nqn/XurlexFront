
    using IurixBlazor.Shared.Dtos;   
    using System.Net.Http.Json;

    namespace IurixBlazor.Services
    {
        public class ProcesoJudicialService
        {
            private readonly HttpClient _http;

            public ProcesoJudicialService(IHttpClientFactory httpClientFactory)
            {
                _http = httpClientFactory.CreateClient("Api");
            }

            public async Task<ProcesoJudicialDto?> ObtenerPorIdAsync(int id)
            {
                var resp = await _http.GetAsync($"api/ProcesoJudicial/{id}");

                if (!resp.IsSuccessStatusCode)
                    return null;

                return await resp.Content.ReadFromJsonAsync<ProcesoJudicialDto>();
            }

            public async Task ActualizarCertificadoAsync(int procesoJudicialId, ActualizarCertificadoProcesoJudicialDto dto)
            {
                var resp = await _http.PatchAsync(
                    $"api/ProcesoJudicial/{procesoJudicialId}/certificado",
                    JsonContent.Create(dto)
                );

                resp.EnsureSuccessStatusCode();
            }
    }
    }


