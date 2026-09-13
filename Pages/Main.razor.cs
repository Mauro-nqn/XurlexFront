using IurixBlazor.Services;
using IurixBlazor.Services.Auth;
using IurixBlazor.Services.Interfaces;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Timers;




namespace IurixBlazor.Pages
{
    public partial class Main : ComponentBase, IDisposable
    {


        [Inject] public IJSRuntime? JS { get; set; }
        [Inject] private NavigationManager? Nav { get; set; }
        

        [Inject] private IApiSesionService? SesionService { get; set; }
        [Inject] private AgendaService? AgendaService { get; set; }

        //  Bootstrap del token para el handler
        [Inject] 
        public ITokenStore? TokenStore { get; set; }

        [Inject] 
        public AuthenticationStateProvider? AuthStateProvider { get; set; } = default!;


        private string UsuarioNombre = "Usuario";
        //private string FechaHora = DateTime.Now.ToString("dddd dd 'de' MMMM yyyy HH:mm:ss");

        private string FechaHora = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time")
        ).ToString("dddd dd 'de' MMMM yyyy HH:mm:ss");


        private System.Timers.Timer? relojTimer;

      


        protected override async Task OnInitializedAsync()
        {
            // 1) Recuperar usuario desde localStorage
            UsuarioNombre = await JS!.InvokeAsync<string>("localStorage.getItem", "usuario_nombre") ?? "Usuario";

            // 2)  Bootstrap del token desde localStorage → TokenStore (circuito)
            var token = await JS!.InvokeAsync<string>("localStorage.getItem", "token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                await TokenStore!.SetAsync(token);
                Console.WriteLine("[Main] Token cargado en TokenStore desde localStorage.");
            }
            else
            {
                Console.WriteLine("[Main] No hay token en localStorage todavía.");
            }

            // Inicializar reloj en tiempo real

            //relojTimer = new System.Timers.Timer(1000);
            //relojTimer.Elapsed += (s, e) =>
            //{
            //    FechaHora = DateTime.Now.ToString("dddd dd 'de' MMMM yyyy HH:mm:ss");
            //    InvokeAsync(StateHasChanged);
            //};
            //relojTimer.Start();

            relojTimer = new System.Timers.Timer(1000);
            relojTimer.Elapsed += (s, e) =>
            {
                var ahoraAR = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time")
                );

                FechaHora = ahoraAR.ToString("dddd dd 'de' MMMM yyyy HH:mm:ss");
                InvokeAsync(StateHasChanged);
            };
            relojTimer.Start();



        }



        //protected override async Task OnAfterRenderAsync(bool firstRender)
        //{
        //    if (firstRender)
        //    {
        //        await JS!.InvokeVoidAsync("mostrarModalAgenda");

        //    }
        //}


        //protected override async Task OnAfterRenderAsync(bool firstRender)
        //{
        //    if (!firstRender) return;

        //    // ¿ya se mostró en esta pestaña?
        //    var yaMostrado = await JS!.InvokeAsync<string>("sessionStorage.getItem", "agenda_shown");

        //    if (string.IsNullOrEmpty(yaMostrado))
        //    {
        //        await JS!.InvokeVoidAsync("sessionStorage.setItem", "agenda_shown", "1");
        //        /*await JS!.InvokeVoidAsync("mostrarModalAgenda");*/ // <- tu función existente

        //        // ✅ da tiempo a que el layout/bootstrp termine de acomodar
        //        await Task.Delay(150);

        //        await OpenAgendaAsync();
        //    }
        //}

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            var yaMostrado = await JS!.InvokeAsync<string>("sessionStorage.getItem", "agenda_shown");
            if (!string.IsNullOrEmpty(yaMostrado)) return;

            await JS!.InvokeVoidAsync("sessionStorage.setItem", "agenda_shown", "1");

            // ✅ cede el frame y deja que Blazor termine renders pendientes
            await Task.Yield();
            await Task.Yield();

            await JS!.InvokeVoidAsync("mostrarModalAgenda");
        }




        private async Task OpenAgendaAsync()
        {
            await Task.Delay(50);
            await JS!.InvokeVoidAsync("mostrarModalAgenda");
        }

        private async Task CloseAgendaAsync()
        {
            await JS!.InvokeVoidAsync("cerrarModalAgenda");
        }





        protected async Task CerrarSesion()
        {
            try
            {
                // Obtener datos necesarios de localStorage
                var claveLicencia = await JS!.InvokeAsync<string>("localStorage.getItem", "clave_licencia");
                var dispositivoSesionId = await JS!.InvokeAsync<string>("localStorage.getItem", "dispositivo_sesion_id");
                var token = await JS!.InvokeAsync<string>("localStorage.getItem", "token");
                System.Diagnostics.Debug.WriteLine($"⚠️ Datos para cerrar sesión - clave {claveLicencia}, dispositivo {dispositivoSesionId}, token {token}");
                // Validar que existan
                if (string.IsNullOrEmpty(claveLicencia) || string.IsNullOrEmpty(dispositivoSesionId) || string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Datos insuficientes para cerrar sesión en backend. Procediendo con limpieza local.");
                }
                else
                {
                    // Llamar API de cierre de sesión
                    bool cerrado = await SesionService!.CerrarSesionAsync(claveLicencia, dispositivoSesionId, token);

                    if (!cerrado)
                    {
                        Console.WriteLine("⚠️ No se pudo cerrar la sesión en el backend.");
                    }
                    else
                    {
                        token = null;

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

            
 

            Nav!.NavigateTo("/login", forceLoad: true);
        }

        private void NavegarA(string ruta)
        {
            Nav!.NavigateTo($"{ruta}?origen=main");
        }

        public void Dispose()
        {
            relojTimer?.Stop();
            relojTimer?.Dispose();
        }
    }
}
