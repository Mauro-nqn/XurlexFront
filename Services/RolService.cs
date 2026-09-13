using IurixBlazor.Services.Auth;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Text.Json;
using static System.Net.WebRequestMethods;


namespace IurixBlazor.Services
{
    public class RolService
    {
        private readonly HttpClient _http;

        private readonly IJSRuntime _js;

        private readonly UsuarioActualService _usuarioActual;


        private readonly ITokenStore _tokenStore;

        private readonly AuthenticationStateProvider _authStateProvider;


        public RolService(
            IHttpClientFactory httpClientFactory, 
            IJSRuntime js, 
            UsuarioActualService usuarioActual,
            ITokenStore tokenStore,
            AuthenticationStateProvider authStateProvider
            )
        {
            _http = httpClientFactory.CreateClient("Api");
            _js = js;
            _usuarioActual = usuarioActual;
            _tokenStore = tokenStore;
            _authStateProvider = authStateProvider;
        }

        public async Task<List<RolDto>> ObtenerRolesAsync()
            => await _http.GetFromJsonAsync<List<RolDto>>("api/roles")
               ?? new List<RolDto>();

        public async Task<RolDto?> ObtenerRolPorIdAsync(int id)
            => await _http.GetFromJsonAsync<RolDto>($"api/roles/{id}");

        public async Task<RolDto?> CrearRolAsync(RolDto dto)
        {
            var resp = await _http.PostAsJsonAsync("api/roles", dto);
            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadFromJsonAsync<RolDto>();
        }

        public async Task<RolDto?> ActualizarRolAsync(RolDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"api/roles/{dto.Id}", dto);
            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadFromJsonAsync<RolDto>();
        }

        public async Task<bool> EliminarRolAsync(int id)
        {
            var resp = await _http.DeleteAsync($"api/roles/{id}");
            return resp.IsSuccessStatusCode;
        }


        public async Task RefrescarPermisosSiAfectaUsuarioActual(int rolIdEditado)
        {
            // 1) Asegurarnos de que UsuarioActual tenga datos (por si se recargó la página)
            if (_usuarioActual.Usuario is null)
            {
                await _usuarioActual.InicializarDesdeLocalStorage(_js);
            }

            var usuarioActual = _usuarioActual.Usuario;
            if (usuarioActual is null)
                return; // no hay sesión cargada, no hacemos nada

            // 2) Si el rol que editamos no es el del usuario logueado, salimos
            if (usuarioActual.RolId != rolIdEditado)
                return;

            // 3) Pedimos al backend el usuario actualizado
            var usuarioRefrescado = await _http.GetFromJsonAsync<UsuarioDto>("api/auth/mi-usuario");
            if (usuarioRefrescado is null)
                return;

            // 4) Actualizamos el servicio de usuario actual
            _usuarioActual.SetUsuario(usuarioRefrescado);

            // 5) Actualizar permisos y nombre en localStorage
            await _js.InvokeVoidAsync("localStorage.setItem", "permisos",
                JsonSerializer.Serialize(usuarioRefrescado.Permisos));

            await _js.InvokeVoidAsync("localStorage.setItem", "usuario_nombre",
                usuarioRefrescado.NombreUsuario ?? "Usuario");
        }



        //Refrescar token
        public async Task RefrescarTokenAsync()
        {
            var response = await _http.PostAsync("api/auth/refresh-token", null);
            if (!response.IsSuccessStatusCode)
                return;

            var refresh = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (refresh is null)
                return;

            // Actualizar usuario actual en memoria
            if (refresh.Usuario is not null)
            {
                _usuarioActual.SetUsuario(refresh.Usuario);

                await _js.InvokeVoidAsync("localStorage.setItem", "usuario_json",
                    JsonSerializer.Serialize(refresh.Usuario));

                await _js.InvokeVoidAsync("localStorage.setItem", "permisos",
                    JsonSerializer.Serialize(refresh.Usuario.Permisos));

                await _js.InvokeVoidAsync("localStorage.setItem", "usuario_nombre",
                    refresh.Usuario.NombreUsuario ?? "Usuario");
            }

            // Actualizar token y expiración
            await _js.InvokeVoidAsync("localStorage.setItem", "token", refresh.Token);
            await _js.InvokeVoidAsync("localStorage.setItem", "token_expiracion",
                refresh.ExpiracionToken.ToString("o"));

            // Actualizar token en el servidor Blazor
            await _tokenStore.SetAsync(refresh.Token);
            ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserAuthentication(refresh.Token);
        }

    }

}
