using Microsoft.JSInterop;

namespace IurixBlazor.Shared.Helpers
{
    public static class DispositivoHelper
    {
        // Obtiene o genera un ID persistente de dispositivo
        public static async Task<string> ObtenerDispositivoIdAsync(IJSRuntime js)
        {
            const string clave = "dispositivo_id";
            var id = await js.InvokeAsync<string>("localStorage.getItem", clave);

            if (!string.IsNullOrEmpty(id))
                return id;

            var nuevoId = $"cliente_local_{Guid.NewGuid():N}";
            await js.InvokeVoidAsync("localStorage.setItem", clave, nuevoId);
            return nuevoId;
        }

        // Genera un ID único de sesión
        public static string GenerarSesionId()
        {
            return $"sesion_{Guid.NewGuid():N}";
        }

        // Obtiene el nombre del equipo (según plataforma Blazor)
        public static async Task<string> ObtenerNombreDelEquipoAsync(IJSRuntime js, bool esServidor)
        {
            if (esServidor)
            {
                // Blazor Server: se ejecuta en backend, podemos usar Environment
                return Environment.MachineName;
            }
            else
            {
                // Blazor WebAssembly: obtenemos User-Agent
                var userAgent = await js.InvokeAsync<string>("navigator.userAgent");
                return userAgent.Contains("Windows") ? "PC Windows" :
                       userAgent.Contains("Android") ? "Dispositivo Android" :
                       userAgent.Contains("iPhone") ? "iPhone" :
                       "Dispositivo Web";
            }
        }
    }
}

