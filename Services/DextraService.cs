using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace IurixBlazor.Services
{
    public class DextraService
    {

        private readonly HttpClient _http;

        public DextraService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_http = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _http = factory.CreateClient("Api");
        }

        public async Task<ProcuracionRunResultDto?> SyncUnoAsync(int procesoJudicialId, bool preview = false)
        {
            var resp = await _http.PostAsync($"api/dextra/sync/proceso/{procesoJudicialId}?preview={preview.ToString().ToLower()}", content: null);
            //resp.EnsureSuccessStatusCode();
            if (!resp.IsSuccessStatusCode)
            {
                var problem = await resp.Content.ReadFromJsonAsync<ProblemDetails>();

                throw new Exception(
                    problem?.Detail ??
                    problem?.Title ??
                    "Error al sincronizar.");
            }
            return await resp.Content.ReadFromJsonAsync<ProcuracionRunResultDto>();
        }


      


        //public async Task<ProcuracionRunAllResultDto?> SyncTodosAsync()
        //{
        //    var resp = await _http.PostAsync("api/procuracion/dextra/sync/todos", content: null);
        //    resp.EnsureSuccessStatusCode();
        //    return await resp.Content.ReadFromJsonAsync<ProcuracionRunAllResultDto>();
        //}
    }
}

