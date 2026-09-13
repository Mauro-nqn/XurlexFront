using Microsoft.JSInterop;

namespace IurixBlazor.Services.Auth
{
    public static class SesionInterop
    {
        public static Func<Task>? OnCerrarSesionSolicitada;

        [JSInvokable("CerrarSesionPorInactividad")]
        public static async Task CerrarSesionPorInactividad()
        {
            if (OnCerrarSesionSolicitada is not null)
                await OnCerrarSesionSolicitada.Invoke();
        }
    }
}
