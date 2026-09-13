using IurixBlazor.Shared.Dtos;
using Microsoft.JSInterop;
using System.Text.Json;
using static System.Net.WebRequestMethods;


namespace IurixBlazor.Services
{
    public class UsuarioActualService
    {
        private UsuarioDto? _usuario;
        private readonly IJSRuntime _js;


        public UsuarioActualService(IJSRuntime js)
        {            
            _js = js;
        }

        public void SetUsuario(UsuarioDto usuario)
            => _usuario = usuario;



        public UsuarioDto? Usuario => _usuario;

        public bool TienePermiso(string permiso)
            => _usuario?.Permisos?.Contains(permiso) == true;

        public async Task InicializarDesdeLocalStorage(IJSRuntime js)
        {
            try
            {
                var usuarioJson = await _js.InvokeAsync<string>("localStorage.getItem", "usuario_json");
                if (string.IsNullOrWhiteSpace(usuarioJson))
                    return;

                var usuario = JsonSerializer.Deserialize<UsuarioDto>(usuarioJson);
                if (usuario != null)
                    _usuario = usuario;
            }
            catch
            {
                // ignorar errores
            }
        }


    }
}

