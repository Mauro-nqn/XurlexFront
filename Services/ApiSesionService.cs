using IurixBlazor.Services.Auth;
using IurixBlazor.Services.Interfaces;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace IurixBlazor.Services
{


    public class ApiSesionService : IApiSesionService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _js;

        [Inject]
        public ITokenStore? TokenStore { get; set; }

        [Inject]
        public AuthenticationStateProvider? AuthStateProvider { get; set; } = default!;


        public ApiSesionService(HttpClient httpClient, IJSRuntime js)
        {
            _httpClient = httpClient;
            _js = js;
        }

        public async Task<LoginResponseDto?> LoginAsync(UsuarioLoginDto loginDto)
        {
            try
            {
                //await _js.InvokeVoidAsync("console.log", $"Consultando en: {_httpClient.BaseAddress}");
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDto);

                var responseContent = await response.Content.ReadAsStringAsync();

                //await _js.InvokeVoidAsync("console.log", $"[Login] Status: {(int)response.StatusCode} {response.StatusCode}");
                //await _js.InvokeVoidAsync("console.log", $"[Login] Respuesta: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return loginResponse;
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("?? Intentando parsear respuesta de error:");
                    //System.Diagnostics.Debug.WriteLine(responseContent);

                    using var doc = JsonDocument.Parse(responseContent);
                    if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                        doc.RootElement.TryGetProperty("mensaje", out var mensajeElement))
                    {
                        var mensaje = mensajeElement.GetString();
                        if (!string.IsNullOrWhiteSpace(mensaje))
                        {
                            //System.Diagnostics.Debug.WriteLine("? Mensaje extraído: " + mensaje);
                            throw new Exception(mensaje);
                        }
                    }

                    throw new Exception("Error desconocido al iniciar sesión.");
                }
            }
            catch
            {
                throw;
            }
        }

        //public async Task<bool> CerrarSesionAsync(string claveLicencia, string dispositivoSesionId, string token)
        //{
        //    try
        //    {
        //        var data = new
        //        {
        //            claveLicencia,
        //            dispositivoSesionId
        //        };

        //        var json = JsonSerializer.Serialize(data);
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        //System.Diagnostics.Debug.WriteLine("?? JSON a enviar: " + json);
        //        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //        var response = await _httpClient.PostAsync("api/Sesiones/cerrar-sesion", content);

        //        if (response.IsSuccessStatusCode) { 
        //        await _js.InvokeVoidAsync("localStorage.clear");
        //        await _js.InvokeVoidAsync("localStorage.removeItem", "token");


        //        if (TokenStore is not null)
        //            await TokenStore.ClearAsync();

        //        if (AuthStateProvider is CustomAuthenticationStateProvider customProvider)
        //            customProvider.NotifyUserLogout();
        //        }

        //        return response.IsSuccessStatusCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("? Error al cerrar sesión: " + ex.Message);
        //        return false;
        //    }




        //}

        //public async Task<bool> CerrarSesionAsync(string claveLicencia, string dispositivoSesionId, string token)
        //{
        //    try
        //    {
        //        var dto = new CerrarSesionDto
        //        {
        //            claveLicencia = claveLicencia,
        //            dispositivoSesionId = dispositivoSesionId,
        //            /*NombreDispositivo = "WebApp"*/ // o lo que corresponda
        //        };

        //        var json = JsonSerializer.Serialize(dto);
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //        var response = await _httpClient.PostAsync("api/Sesiones/cerrar-sesion", content);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            await _js.InvokeVoidAsync("localStorage.clear");

        //            if (TokenStore is not null)
        //                await TokenStore.ClearAsync();

        //            if (AuthStateProvider is CustomAuthenticationStateProvider customProvider)
        //                customProvider.NotifyUserLogout();
        //        }

        //        return response.IsSuccessStatusCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"? Error al cerrar sesión: {ex.Message}");
        //        return false;
        //    }
        //}


        public async Task<bool> CerrarSesionAsync(string claveLicencia, string dispositivoSesionId, string token)
        {
            try
            {
                var dto = new CerrarSesionDto
                {
                    claveLicencia = claveLicencia,
                    dispositivoSesionId = dispositivoSesionId
                };

                var json = JsonSerializer.Serialize(dto);

                // ?? Log en consola del navegador: datos que se van a enviar
                await _js.InvokeVoidAsync("console.log", "?? Enviando cierre de sesión al backend:");
                await _js.InvokeVoidAsync("console.log", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // ?? Log en consola del navegador: URL de destino
                await _js.InvokeVoidAsync("console.log", "?? POST a /api/Sesiones/cerrar-sesion");

                var response = await _httpClient.PostAsync("api/Sesiones/cerrar-sesion", content);

                // ?? Log en consola del navegador: resultado
                await _js.InvokeVoidAsync("console.log", $"?? Respuesta del backend: {(int)response.StatusCode} {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    await _js.InvokeVoidAsync("console.log", "? Sesión cerrada correctamente. Limpiando localStorage.");

                    await _js.InvokeVoidAsync("localStorage.clear");

                    if (TokenStore is not null)
                        await TokenStore.ClearAsync();

                    if (AuthStateProvider is CustomAuthenticationStateProvider customProvider)
                        customProvider.NotifyUserLogout();
                }
                else
                {
                    await _js.InvokeVoidAsync("console.log", "?? Falló el cierre de sesión en el backend.");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                await _js.InvokeVoidAsync("console.error", $"? Error al cerrar sesión: {ex.Message}");
                return false;
            }
        }











    }
}
