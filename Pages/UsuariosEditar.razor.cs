using IurixBlazor.Services;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using static Org.BouncyCastle.Math.EC.ECCurve;


    public class UsuariosEditarBase : ComponentBase, IAsyncDisposable
{
        [Inject] protected UsuarioService UsuarioService { get; set; } = default!;
        [Inject] protected CondicionIvaService? CondicionIvaService { get; set; }
        [Inject] protected NavigationManager? Navigation { get; set; }
        [Inject] protected IJSRuntime JS { get; set; } = default!;

        [Inject] protected ConfigService Config { get; set; } = default!;

    [Parameter] public int Id { get; set; }

        protected UsuarioDto usuario = new();
        protected List<RolDto> Roles = new();
        protected List<CondicionIvaDto> CondicionIvas = new();
        protected bool IsLoading = true;

        protected bool? GoogleVinculado { get; set; } = null; // null=desconocido, true/false
        protected string? GoogleEmail { get; set; } = null;

        protected EditContext? editContext;

    // En tu componente UsuariosEditarBase
    //private CancellationTokenSource? _linkCts;
    //protected bool IsLinking { get; set; } = false;

    private DotNetObjectReference<UsuariosEditarBase>? _objRef;


    protected bool esMovil = false;

    protected override async Task OnInitializedAsync()
        {


            IsLoading = true;
            
            // Obtener roles para el combo
            Roles = await UsuarioService!.ObtenerRolesAsync();
            CondicionIvas = await CondicionIvaService!.ObtenerTodasAsync();
            
            //Cargamos Host Dextra desde Config que esta definido ahi
            NuevaCred.Host = Config.DextraDefaultHost;
        // Cargar usuario por ID
        var usuarioExistente = await UsuarioService.ObtenerUsuarioPorIdAsync(Id);
            if (usuarioExistente != null)
            {
                usuario = usuarioExistente;
            // ⚠️ Crear EditContext DESPUÉS de asignar 'usuario'
            editContext = new EditContext(usuario);

        }
            if (usuario?.Id > 0)
            {
                var estado = await UsuarioService.ObtenerEstadoGoogleAsync(usuario.Id);
                GoogleEmail = estado?.Email;
                GoogleVinculado = !string.IsNullOrWhiteSpace(estado?.RefreshToken); // criterio real de vinculación
            }



        IsLoading = false;
        }



    //protected override async Task OnAfterRenderAsync(bool firstRender)
    //{
    //    if (firstRender)
    //    {
    //        _objRef = DotNetObjectReference.Create(this);
    //        var origin = new Uri(Navigation!.BaseUri).GetLeftPart(UriPartial.Authority).TrimEnd('/');
    //        // ej: http://192.168.1.100:7000

    //        await JS.InvokeVoidAsync("googleLink.startListening", _objRef, origin);
    //    }
    //}

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        
            // ⚠️ JSInterop después del primer render, así el script seguro ya está

            var width = await JS.InvokeAsync<int>("obtenerAnchoPantalla");
            esMovil = width < 768; // breakpoint Bootstrap sm/md

            StateHasChanged();
        

        _objRef = DotNetObjectReference.Create(this);

        // Origin del BACKEND (quien ENVÍA el postMessage)
        var backendOrigin =
            $"{(Config.UsaHttpsServidorBackend ? "https" : "http")}://{Config.IpServidor}:{Config.Puerto}".TrimEnd('/');

        // (opcional) retry suave por si el script aún no está
        for (var i = 0; i < 10; i++)
        {
            try
            {
                await JS.InvokeVoidAsync("googleLink.startListening", _objRef, new[] { backendOrigin });
                break;
            }
            catch
            {
                await Task.Delay(150);
            }
        }
    }




    protected void OnInvalidSubmit(EditContext ctx)
    {
        // Mostrar errores de validación/conversión
        var errores = string.Join(" | ", ctx.GetValidationMessages());
        JS.InvokeVoidAsync("console.warn", $"Formulario inválido: {errores}");
        JS.InvokeVoidAsync("mostrarToast", "Revisá los campos: " + errores, "error");
    }

    [JSInvokable]
    public async Task OnGoogleLinked(int userId)
    {
        if (usuario != null && usuario.Id == userId)
        {
            var est = await UsuarioService.ObtenerEstadoGoogleAsync(userId);
            GoogleEmail = est?.Email;
            // Vinculado si tenemos refresh token (o access + refresh previo guardado)
            GoogleVinculado = !string.IsNullOrWhiteSpace(est?.RefreshToken);

            await JS.InvokeVoidAsync("mostrarToast", $"✅ Vinculado: {GoogleEmail}", "success");
            StateHasChanged();
        }
    }


    public async ValueTask DisposeAsync()
    {
        try { await JS.InvokeVoidAsync("googleLink.stopListening"); } catch { }
        _objRef?.Dispose();
    }



    //protected async Task ActualizarUsuario()
    //    {
    //        var exito = await UsuarioService!.ActualizarUsuarioAsync(usuario);
    //        if (exito)
    //        {
    //            Navigation!.NavigateTo("/usuarios");
    //        }
    //    }

    protected async Task ActualizarUsuario()
    {
        try
        {
            // Ejemplo: si usás -1 como “Seleccione…”, normalizá a null:
            if (usuario.RolId <= 0)
            {
                await JS.InvokeVoidAsync("mostrarToast", "Seleccioná un Rol.", "error");
                return;
            }
            if (usuario.CondicionIvaId.HasValue && usuario.CondicionIvaId.Value <= 0) usuario.CondicionIvaId = null;

            // Si Password vacío NO debe cambiarse, podés omitir enviarlo:
            // if (string.IsNullOrWhiteSpace(usuario.Password)) usuario.Password = null;  // sólo si backend lo permite

            var exito = await UsuarioService.ActualizarUsuarioAsync(usuario);

            if (exito)
            {
                await JS.InvokeVoidAsync("mostrarToast", "✅ Usuario actualizado", "success");
                Navigation!.NavigateTo("/usuarios");
            }
            else
            {
                await JS.InvokeVoidAsync("mostrarToast", "No se pudo actualizar (respuesta false).", "error");
            }
        }
        catch (HttpRequestException ex)
        {
            await JS.InvokeVoidAsync("mostrarToast", $"Error HTTP: {ex.Message}", "error");
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("mostrarToast", $"Error: {ex.Message}", "error");
        }
    }





    protected async Task OnVincularGoogleClicked()
    {
        if (usuario == null || usuario.Id <= 0)
        {
            Console.WriteLine("❌ Error: Primero guardá el usuario.");
            return;
        }


        await JS.InvokeVoidAsync("console.log", "LLamando a vincular Google");

        // 1️ Verificar estado actual de vinculación
        var estado = await UsuarioService.ObtenerEstadoGoogleAsync(usuario.Id);

        await JS.InvokeVoidAsync("console.log", $"Estado vinculacion google {estado}");

        if (estado != null && !string.IsNullOrEmpty(estado.RefreshToken))
        {
            // Mostrar confirmación con la cuenta actual vinculada
            var mensaje = $"Este usuario ya está vinculado a la cuenta de Google:\n\n📧 {estado.Email}\n\n¿Deseás vincular otra cuenta?";
            bool confirmar = await JS.InvokeAsync<bool>("confirm", mensaje);

            if (!confirmar)
            {
                await JS.InvokeVoidAsync("mostrarToast", "Operación cancelada por el usuario.", "info");
                return;
            }
        }

        // 2️ Generar URL de vinculación
        var url = await UsuarioService.ObtenerUrlLoginGoogleAsync(usuario.Id);
        await JS.InvokeVoidAsync("console.log", $"Url de vinculacion {url}");

        if (!string.IsNullOrWhiteSpace(url))
        {
#if WINDOWS
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        };
        System.Diagnostics.Process.Start(psi);
#else
            await JS.InvokeVoidAsync("open", url, "_blank"); // Abre en una nueva pestaña en Blazor
#endif



            // 3) Empezar a chequear el estado por unos segundos
            //_linkCts?.Cancel();
            //_linkCts = new CancellationTokenSource();
            //IsLinking = true;
            //StateHasChanged();
            //_ = PollGoogleLinkAsync(_linkCts.Token); // no esperes acá; deja que siga la UI
        }
        else
        {
            Console.WriteLine("❌ No se pudo generar el enlace de Google.");
        }
    }






    //private async Task PollGoogleLinkAsync(CancellationToken ct)
    //{
    //    try
    //    {
    //        // intenta durante ~60s, cada 2s
    //        for (var i = 0; i < 30; i++)
    //        {
    //            await Task.Delay(2000, ct);

    //            var est = await UsuarioService.ObtenerEstadoGoogleAsync(usuario.Id);
    //            var vinculado = !string.IsNullOrWhiteSpace(est?.RefreshToken);
    //            if (vinculado)
    //            {
    //                GoogleVinculado = true;
    //                GoogleEmail = est?.Email;
    //                IsLinking = false;
    //                await JS.InvokeVoidAsync("mostrarToast", $"✅ Vinculado: {GoogleEmail}", "success");
    //                StateHasChanged();
    //                return;
    //            }
    //        }

    //        // si no se detectó en el tiempo dado
    //        await JS.InvokeVoidAsync("mostrarToast",
    //            "No se detectó la vinculación aún. Terminá el flujo en la pestaña abierta y volvé a probar “Verificar vinculación”.",
    //            "info");
    //    }
    //    catch (OperationCanceledException) { /* cancelado a propósito */ }
    //    finally
    //    {
    //        IsLinking = false;
    //        StateHasChanged();
    //    }
    //}

    //protected async Task OnVerificarVinculacionClicked()
    //{
    //    if (usuario == null || usuario.Id <= 0)
    //    {
    //        Console.WriteLine("❌ Error: Primero guardá el usuario.");
    //        return;
    //    }

    //    var estado = await UsuarioService.ObtenerEstadoGoogleAsync(usuario.Id);

    //    if (estado == null)
    //    {
    //        Console.WriteLine("❌ Error: No se pudo consultar el estado.");
    //    }
    //    else if (!string.IsNullOrEmpty(estado.AccessToken))
    //    {
    //        Console.WriteLine("✅ Google fue vinculado correctamente.");
    //    }
    //    else
    //    {
    //        Console.WriteLine("⚠ Pendiente: La cuenta aún no fue vinculada.");
    //    }
    //}














    //protected async Task OnVerificarVinculacionClicked()
    //{
    //    if (usuario == null || usuario.Id <= 0)
    //    {
    //        await JS.InvokeVoidAsync("mostrarToast", "Primero guardá el usuario.", "error");
    //        return;
    //    }

    //    var estado = await UsuarioService.ObtenerEstadoGoogleAsync(usuario.Id);

    //    if (estado == null)
    //    {
    //        await JS.InvokeVoidAsync("mostrarToast", "No se pudo consultar el estado.", "error");
    //    }
    //    else if (!string.IsNullOrEmpty(estado.AccessToken))
    //    {
    //        await JS.InvokeVoidAsync("mostrarToast", "Google fue vinculado correctamente.", "success");
    //    }
    //    else
    //    {
    //        await JS.InvokeVoidAsync("mostrarToast", "La cuenta aún no fue vinculada.", "info");
    //    }
    //}







    protected async Task OnVerificarVinculacionClicked()
    {
        if (usuario == null || usuario.Id <= 0)
        {
            await JS.InvokeVoidAsync("mostrarToast", "Primero guardá el usuario.", "error");
            return;
        }

        var estado = await UsuarioService.ObtenerEstadoGoogleAsync(usuario.Id);

        if (estado == null)
        {
            await JS.InvokeVoidAsync("mostrarToast", "No se pudo consultar el estado de vinculación.", "error");
            return;
        }

        if (!string.IsNullOrEmpty(estado.RefreshToken))
        {
            var mensaje = !string.IsNullOrEmpty(estado.Email)
                ? $"Google fue vinculado correctamente.\n📧 {estado.Email}"
                : "Google fue vinculado correctamente, pero no se pudo obtener el email.";

            await JS.InvokeVoidAsync("mostrarToast", mensaje, "success");
        }
        else
        {
            await JS.InvokeVoidAsync("mostrarToast", "La cuenta aún no fue vinculada.", "info");
        }
    }



    protected async Task OnDesvincularGoogleClicked()
    {
        if (usuario is null || usuario.Id <= 0)
        {
            await JS.InvokeVoidAsync("mostrarToast", "Primero guardá el usuario.", "error");
            return;
        }

        var ok = await JS.InvokeAsync<bool>("confirm",
            "¿Desvincular la cuenta de Google de este usuario?\nSe eliminarán los tokens almacenados.");
        if (!ok) return;

        try
        {
            await UsuarioService.DesvincularGoogleAsync(usuario.Id);

            // Limpio estado UI
            GoogleVinculado = false;
            GoogleEmail = null;

            // Este componente no necesita tocar 'UsuarioGoogleId'.
            // Si lo habías puesto, podés borrar esta línea:
            // if (UsuarioGoogleId == usuario.Id) UsuarioGoogleId = null;

            await JS.InvokeVoidAsync("mostrarToast", "🔌 Cuenta de Google desvinculada.", "success");
            StateHasChanged();
        }
        catch (HttpRequestException ex)
        {
            await JS.InvokeVoidAsync("mostrarToast", $"No se pudo desvincular: {ex.Message}", "error");
        }
    }






    //DEXTRA 

    // DTO local para crear
    protected class NuevaCredDto { public string Host { get; set; } = ""; public string UsuarioDextra { get; set; } = ""; public string PasswordPlain { get; set; } = ""; public bool IsDefault { get; set; } = true; }

    protected NuevaCredDto NuevaCred = new();
    protected List<DextraCredListItemDto> Creds = new();



    protected override async Task OnParametersSetAsync()
    {
        // cargar usuario y luego:
        await CargarCredsAsync();
    }

    private async Task CargarCredsAsync()
    {
        Creds = await UsuarioService.ListarCredsAsync(usuario.Id);
    }

    protected async Task CrearCredencialAsync()
    {
        if (usuario.Id <= 0) { await JS.InvokeVoidAsync("mostrarToast", "Guardá el usuario antes.", "error"); return; }

        var okId = await UsuarioService.CrearCredAsync(
           usuario.Id,
           new DextraCreateRequestDto
           {
               UsuarioId = usuario.Id, // opcional
               Host = string.IsNullOrWhiteSpace(NuevaCred.Host) ? "" : NuevaCred.Host,
               UsuarioDextra = NuevaCred.UsuarioDextra,
               PasswordPlain = NuevaCred.PasswordPlain,
               IsDefault = NuevaCred.IsDefault
           }
       );

        if (okId > 0)
        {
            await JS.InvokeVoidAsync("mostrarToast", "Credencial guardada", "success");
            NuevaCred.PasswordPlain = ""; // limpiar en UI
            await CargarCredsAsync();
            StateHasChanged();
        }
    }

    //protected async Task VerificarCredAsync(int credId)
    //{
    //    await JS.InvokeVoidAsync("console.log", $"Usuario ID: {usuario.Id} - Credencial ID: {credId}");

    //    var res = await UsuarioService.VerificarCredAsync(usuario.Id, credId);

    //    await JS.InvokeVoidAsync("console.log", $"Respuesta desde endpoint: {res}");
    //    var msg = res?.Ok == true ? "✅ Credencial válida" : $"❌ Credencial inválida: {res?.Note}";
    //    await JS.InvokeVoidAsync("mostrarToast", msg, res?.Ok == true ? "success" : "error");
    //    await CargarCredsAsync();
    //}


    protected async Task VerificarCredAsync(int credId)
    {
        try
        {
            await JS.InvokeVoidAsync("console.log", $"Usuario ID: {usuario.Id} - Credencial ID: {credId}");

            var res = await UsuarioService.VerificarCredAsync(usuario.Id, credId);

            if (res is null)
            {
                await JS.InvokeVoidAsync(
                    "mostrarToast",
                    "No se pudo verificar la credencial. El servidor no devolvió una respuesta válida.",
                    "error");

                return;
            }

            var msg = res.Ok
                ? "✅ Credencial válida"
                : $"❌ {res.Note}";

            await JS.InvokeVoidAsync("mostrarToast", msg, res.Ok ? "success" : "error");

            await CargarCredsAsync();
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync(
                "mostrarToast",
                $"Error al verificar credencial: {ex.Message}",
                "error");
        }
    }

    protected async Task SetDefaultAsync(int credId)
    {
        await UsuarioService.ActualizarCredAsync(usuario.Id, credId, new { IsDefault = true });
        await CargarCredsAsync();
    }
    protected async Task EliminarCredAsync(int credId)
    {
        var ok = await JS.InvokeAsync<bool>("confirm", "¿Eliminar credencial?");
        if (!ok) return;
        await UsuarioService.EliminarCredAsync(usuario.Id, credId);
        await CargarCredsAsync();
    }






    protected void Cancelar() => Navigation!.NavigateTo("/usuarios");
    }

