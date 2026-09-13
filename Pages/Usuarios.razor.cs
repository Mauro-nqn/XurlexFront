using IurixBlazor.Services;
using IurixBlazor.Services.Auth;
using IurixBlazor.Services.Interfaces;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Text.Json;



namespace IurixBlazor.Pages
{
   
    public partial class UsuariosBase : ComponentBase
    {
        [Inject] protected UsuarioService UsuarioService { get; set; } = default!;
        [Inject] protected IJSRuntime JS { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;

        [Inject] protected IApiSesionService SesionService { get; set; } = default!;


        [Inject]
        public ITokenStore? TokenStore { get; set; }

        [Inject]
        public AuthenticationStateProvider? AuthStateProvider { get; set; } = default!;

        protected List<UsuarioDto> Usuarios = new();
        protected bool IsBusy = false;
        protected string CantidadUsuariosLabel = "";
        protected bool PuedeCrear = false;

        protected string UsuarioNombre = "Usuario";

        protected override async Task OnInitializedAsync()
        {
            // Recuperar usuario desde localStorage
            UsuarioNombre = await JS.InvokeAsync<string>("localStorage.getItem", "usuario_nombre") ?? "Usuario";

            await CargarUsuarios();
        }

        protected async Task CargarUsuarios()
        {
            IsBusy = true;
            try
            {
                var permisosJson = await JS.InvokeAsync<string>("localStorage.getItem", "permisos");
                var permisos = string.IsNullOrEmpty(permisosJson)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(permisosJson) ?? new();

                PuedeCrear = permisos.Contains("Usuarios.Crear");
                var puedeEditar = permisos.Contains("Usuarios.Editar");
                var puedeEliminar = permisos.Contains("Usuarios.Eliminar");

                var lista = await UsuarioService.ObtenerUsuariosAsync();
                foreach (var u in lista)
                {
                    u.PuedeEditar = puedeEditar;
                    u.PuedeEliminar = puedeEliminar;
                }

                Usuarios = lista;
                CantidadUsuariosLabel = $"Usuarios encontrados: {Usuarios.Count}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected void NuevoUsuario() => Nav.NavigateTo("/usuarios/nuevo");
        protected void EditarUsuario(int id) => Nav.NavigateTo($"/usuarios/editar/{id}");

        protected async Task EliminarUsuario(int id, string nombreUsuario)
        {
            var confirmar = await JS.InvokeAsync<bool>("confirm", $"¿Eliminar usuario {nombreUsuario}?");
            if (!confirmar) return;

            if (await UsuarioService.EliminarUsuarioAsync(id))
            {
                await JS.InvokeVoidAsync("alert", "Usuario eliminado correctamente");
                await CargarUsuarios();
            }
            else
            {
                await JS.InvokeVoidAsync("alert", "No se pudo eliminar el usuario");
            }
        }
    



    protected async Task CerrarSesion()
        {
            try
            {
                // Obtener datos necesarios de localStorage
                var claveLicencia = await JS.InvokeAsync<string>("localStorage.getItem", "clave_licencia");
                var dispositivoSesionId = await JS.InvokeAsync<string>("localStorage.getItem", "dispositivo_sesion_id");
                var token = await JS.InvokeAsync<string>("localStorage.getItem", "token");
                System.Diagnostics.Debug.WriteLine($"⚠️ Datos para cerrar sesión - clave {claveLicencia}, dispositivo {dispositivoSesionId}, token {token}");
                // Validar que existan
                if (string.IsNullOrEmpty(claveLicencia) || string.IsNullOrEmpty(dispositivoSesionId) || string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Datos insuficientes para cerrar sesión en backend. Procediendo con limpieza local.");
                }
                else
                {
                    // Llamar API de cierre de sesión
                    bool cerrado = await SesionService.CerrarSesionAsync(claveLicencia, dispositivoSesionId, token);

                    if (!cerrado)
                    {
                        Console.WriteLine("⚠️ No se pudo cerrar la sesión en el backend.");
                    }
                    else
                    {
                        Console.WriteLine("✅ Sesión cerrada en el backend correctamente.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al cerrar sesión: {ex.Message}");
            }

            // Limpiar datos locales y redirigir
           

            await JS.InvokeVoidAsync("localStorage.clear");
            await JS.InvokeVoidAsync("localStorage.removeItem", "token");


            if (TokenStore is not null)
                await TokenStore.ClearAsync();

            if (AuthStateProvider is CustomAuthenticationStateProvider customProvider)
                customProvider.NotifyUserLogout();


            Nav.NavigateTo("/login", forceLoad: true);
        }

    }

}
