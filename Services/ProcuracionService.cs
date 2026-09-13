// IurixBlazor/Services/ProcuracionService.cs
using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;


namespace IurixBlazor.Services
{


    public class ProcuracionService
    {
        private readonly HttpClient _http;
        
        public ProcuracionService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_http = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _http = factory.CreateClient("Api");
        }

        public async Task<ProcuracionRunResultDto?> SyncUnoAsync(int procesoJudicialId)
        {
            var resp = await _http.PostAsync($"api/procuracion/dextra/sync/proceso/{procesoJudicialId}", content: null);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ProcuracionRunResultDto>();
        }
        
        public async Task<ProcuracionRunAllResultDto?> SyncTodosAsync()
        {
            var resp = await _http.PostAsync("api/procuracion/dextra/sync/todos", content: null);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ProcuracionRunAllResultDto>();
        }
    }
}








