
    using IurixBlazor.Shared.Dtos;
    using IurixBlazor.Services.Http;
    using System.Net.Http.Json;

    namespace IurixBlazor.Services
    {
        public class ProcesoJudicialService
        {
            private readonly IApiRequestSender _sender;

            public ProcesoJudicialService(IApiRequestSender sender)
            {
                _sender = sender;
            }

            public async Task<ProcesoJudicialDto?> ObtenerPorIdAsync(int id)
            {
                using var resp = await _sender.SendAsync(HttpMethod.Get, $"api/ProcesoJudicial/{id}");

                if (!resp.IsSuccessStatusCode)
                    return null;

                return await resp.Content.ReadFromJsonAsync<ProcesoJudicialDto>();
            }

            public async Task ActualizarCertificadoAsync(int procesoJudicialId, ActualizarCertificadoProcesoJudicialDto dto)
            {
                using var resp = await _sender.SendAsync(HttpMethod.Patch,
                    $"api/ProcesoJudicial/{procesoJudicialId}/certificado",
                    JsonContent.Create(dto)
                );

                resp.EnsureSuccessStatusCode();
            }
    }
    }


