using IurixBlazor.Services;
using IurixBlazor.Services.Auth;
using IurixBlazor.Services.Interfaces;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;




namespace IurixBlazor.Pages

{

    public partial class Login


    {



        [Inject] AuthenticationStateProvider? AuthStateProvider { get; set; } = default!;


        [Inject] UsuarioActualService? UsuarioActualService { get; set; } = default!;



        protected string Usuario = "";
        protected string Password = "";
        protected string Mensaje = "";


        protected bool ShowPassword = false;
        protected string PasswordInputType => ShowPassword ? "text" : "password";
        protected string EyeIconClass => ShowPassword ? "bi bi-eye-slash" : "bi bi-eye";



        protected string FondoSeleccionado = "";

        protected string LicenciaEstudioTexto = "";
        protected string LicenciaInfoTexto = "";
        protected string LicenciaEstado = "";
        protected string LicenciaInfoColor = "green";
        protected bool LicenciaEstudioVisible = false;
        protected bool LicenciaInfoVisible = false;

        protected LicenciaDto? LicenciaActual;

        protected bool DebeGuardarLogin = false;
        protected LoginResponseDto? LoginPendiente;

        protected bool LicenciaVerificada = false;

        protected bool IntentosAgotados = false;

        protected string errorMessage = "";

        protected string SesionId = "";



        private string LicenciaUploadMensaje = "";


        protected bool PuedeIniciarSesion =>
    LicenciaEstado == "Vigente" || LicenciaEstado == "Por vencer";

        protected bool MostrarUploaderLicencia =>
            LicenciaEstado == "Sin licencia" || LicenciaEstado == "Vencida" || IntentosAgotados;


        private bool LicenciaCargando = true; // al inicio estamos verificando


        private bool MostrarUploaderManual { get; set; } = false;


        //protected override async Task OnInitializedAsync()
        //{
        //    SeleccionarFondoAleatorio();
        //    await VerificarLicenciaAlIniciar();
        //}

        //protected override async Task OnInitializedAsync()
        //{
        //    try
        //    {
        //        SeleccionarFondoAleatorio();

        //        if (!LicenciaVerificada)
        //        {
        //            LicenciaVerificada = true; // ?? Evita que se ejecute más de una vez
        //            await VerificarLicenciaAlIniciar();
        //        }


        //    }


        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"?? Error en login: {ex}");
        //        errorMessage = ex.Message;
        //    }
        //}


        protected override async Task OnInitializedAsync()
        {
            try
            {
                SeleccionarFondoAleatorio();

                if (!LicenciaVerificada)
                {
                    LicenciaVerificada = true;   // evita que se ejecute más de una vez
                    LicenciaCargando = true;     //  empezamos la verificación

                    await VerificarLicenciaAlIniciar();

                    LicenciaCargando = false;    //  terminamos, recién acá mostramos cosas
                }
                else
                {
                    // Si por algún motivo ya estaba verificada,
                    // igual dejamos Cargando = false para no bloquear la UI
                    LicenciaCargando = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error en login: {ex}");
                errorMessage = ex.Message;
                LicenciaCargando = false;
            }
        }



        private void TogglePasswordVisibility()
        {
            ShowPassword = !ShowPassword;
        }

        private void SeleccionarFondoAleatorio()
        {
            var fondos = new List<string>
            {
                "login.webp",
                "login2.webp",
                "login3.webp",
                "login4.webp",
                "login5.webp",
                "login6.webp"
            };
            FondoSeleccionado = fondos[new Random().Next(fondos.Count)];
        }

        //private async Task VerificarLicenciaAlIniciar()
        //{
        //    var licencia = await ObtenerLicenciaDesdeServidor();
        //    if (licencia == null)
        //    {
        //        Mensaje = "? No se pudo conectar con el servidor de licencia.";
        //        return;
        //    }

        //    LicenciaActual = licencia;
        //    LicenciaEstudioTexto = $"Licencia de: {licencia.EstudioNombre}";
        //    LicenciaEstudioVisible = true;

        //    var estado = LicenciaHelper.CalcularEstado(licencia.ValidoHasta);
        //    LicenciaInfoTexto = estado == "Vencida"
        //        ? $"Licencia vencida el: {licencia.ValidoHasta:dd/MM/yyyy}. Contactar a soporte."
        //        : $"Licencia válida hasta: {licencia.ValidoHasta:dd/MM/yyyy}";
        //    LicenciaInfoColor = estado == "Vencida" ? "red" : (estado == "Por vencer" ? "orange" : "green");
        //    LicenciaInfoVisible = true;
        //}





        //private async Task VerificarLicenciaAlIniciar()
        //{
        //    int intentos = 0;
        //    const int maxIntentos = 3; // Cantidad máxima de intentos
        //    const int delayEntreIntentos = 5000; // 5 segundos de espera entre intentos

        //    while (intentos < maxIntentos)
        //    {
        //        var licencia = await ObtenerLicenciaDesdeServidor();

        //        if (licencia != null)
        //        {
        //            // ? Si conecta y obtiene licencia, salir del bucle
        //            LicenciaActual = licencia;
        //            LicenciaEstudioTexto = $"Licencia de: {licencia.EstudioNombre}";
        //            LicenciaEstudioVisible = true;

        //            var estado = LicenciaHelper.CalcularEstado(licencia.ValidoHasta);
        //            LicenciaInfoTexto = estado == "Vencida"
        //                ? $"Licencia vencida el: {licencia.ValidoHasta:dd/MM/yyyy}. Contactar a soporte."
        //                : $"Licencia válida hasta: {licencia.ValidoHasta:dd/MM/yyyy}";
        //            LicenciaInfoColor = estado == "Vencida" ? "red" : (estado == "Por vencer" ? "orange" : "green");
        //            LicenciaInfoVisible = true;
        //            return;
        //        }

        //        // ? Si no conecta, mostrar mensaje y esperar antes de reintentar
        //        intentos++;
        //        Mensaje = $"? No se pudo conectar con el servidor. Reintentando ({intentos}/{maxIntentos})...";
        //        StateHasChanged();

        //        if (intentos < maxIntentos)
        //            await Task.Delay(delayEntreIntentos); // Espera antes de reintentar
        //    }

        //    // ? Si después de todos los intentos no conecta
        //    Mensaje = "? No se pudo conectar con el servidor tras varios intentos. Verifique que el backend esté en ejecución.";
        //}




        //private async Task VerificarLicenciaAlIniciar()
        //{
        //    int intentos = 0;
        //    const int maxIntentos = 3;
        //    const int delayEntreIntentos = 5000;

        //    while (intentos < maxIntentos)
        //    {
        //        var licencia = await ObtenerLicenciaDesdeServidor();

        //        if (licencia != null)
        //        {
        //            //  Limpiar mensaje y estado de error cuando conecta
        //            Mensaje = "";
        //            IntentosAgotados = false;

        //            LicenciaActual = licencia;
        //            LicenciaEstudioTexto = $"Licencia de: {licencia.EstudioNombre}";
        //            LicenciaEstado = licencia.Estado;
        //            LicenciaEstudioVisible = true;

        //            var estado = LicenciaHelper.CalcularEstado(licencia.ValidoHasta);
        //            LicenciaInfoTexto = estado == "Vencida"
        //                ? $"Licencia vencida el: {licencia.ValidoHasta:dd/MM/yyyy}. Contactar a soporte."
        //                : $"Licencia válida hasta: {licencia.ValidoHasta:dd/MM/yyyy}";
        //            LicenciaInfoColor = estado == "Vencida" ? "red" : (estado == "Por vencer" ? "orange" : "green");
        //            LicenciaInfoVisible = true;
        //            return;
        //        }

        //        intentos++;
        //        Mensaje = $"->No se pudo conectar con el servidor. Reintentando ({intentos}/{maxIntentos})...";
        //        StateHasChanged();


        //        if (intentos < maxIntentos)
        //            await Task.Delay(delayEntreIntentos);
        //    }

        //    IntentosAgotados = true;
        //    Mensaje = "->No se pudo conectar tras varios intentos. Verifique que el backend esté en ejecución.";
        //}


        private async Task VerificarLicenciaAlIniciar()
        {
            int intentos = 0;
            const int maxIntentos = 3;
            const int delayEntreIntentos = 5000;

            while (intentos < maxIntentos)
            {
                var licencia = await ObtenerLicenciaDesdeServidor();

                if (licencia != null)
                {
                    Mensaje = "";
                    IntentosAgotados = false;

                    LicenciaActual = licencia;
                    LicenciaEstudioTexto = $"Licencia de: {licencia.EstudioNombre}";
                    LicenciaEstudioVisible = true;

                    var estado = LicenciaHelper.CalcularEstado(licencia.ValidoHasta);
                    LicenciaEstado = estado;

                    LicenciaInfoTexto = estado switch
                    {
                        "Vencida" => $"Licencia vencida el: {licencia.ValidoHasta:dd/MM/yyyy}. Contactar a soporte.",
                        "Por vencer" => $"Licencia válida hasta: {licencia.ValidoHasta:dd/MM/yyyy}. (Por vencer)",
                        "Vigente" => $"Licencia válida hasta: {licencia.ValidoHasta:dd/MM/yyyy}",
                        _ => "Sin licencia registrada."
                    };

                    LicenciaInfoColor = estado switch
                    {
                        "Vencida" => "red",
                        "Por vencer" => "orange",
                        "Vigente" => "green",
                        _ => "red"
                    };

                    LicenciaInfoVisible = true;
                    return;
                }

                intentos++;
                Mensaje = $"->No se pudo conectar con el servidor. Reintentando ({intentos}/{maxIntentos})...";
                StateHasChanged();

                if (intentos < maxIntentos)
                    await Task.Delay(delayEntreIntentos);
            }

            // si llegó acá, no hubo licencia
            IntentosAgotados = true;
            LicenciaEstado = "Sin licencia";
            LicenciaInfoTexto = "No se encontró una licencia registrada o no se pudo verificar con el servidor.";
            LicenciaInfoColor = "red";
            LicenciaInfoVisible = true;
        }



        //private async Task<LicenciaDto?> ObtenerLicenciaDesdeServidor()
        //{
        //    try

        //    {

        //        //await JS.InvokeVoidAsync("console.log", $"[DEBUG] ServidorBackendUrl: {Config.ServidorBackendUrl}");
        //        //await JS.InvokeVoidAsync("console.log", $"[DEBUG] LicenciaBackendUrl: {Config.LicenciaBackendUrl}");


        //        var http = HttpFactory.CreateClient();
        //        http.BaseAddress = new Uri(Config.ServidorBackendUrl);

        //        //await JS.InvokeVoidAsync("console.log", $"[DEBUG] http.BaseAddress: {http.BaseAddress}");
        //        http.DefaultRequestHeaders.Add("X-Internal-Key", "iurix-internal-2024");






        //        var claveDto = await http.GetFromJsonAsync<ClaveLicenciaDto>("api/licencia/clave");

        //        //await JS.InvokeVoidAsync("console.log", $"[DEBUG] Clave obtenida: {claveDto?.Id}");
        //        if (claveDto == null || claveDto.Id == 0) return null;



        //        var httpLic = HttpFactory.CreateClient();
        //        httpLic.BaseAddress = new Uri(Config.LicenciaBackendUrl);


        //        //await JS.InvokeVoidAsync("console.log", $"[DEBUG] httpLic.BaseAddress: {httpLic.BaseAddress}");
        //        httpLic.DefaultRequestHeaders.Add("X-Internal-Key", "iurix-internal-2024");





        //        //await JS.InvokeVoidAsync("console.log", $"[Licencia] Conectando al backend de licencia: {httpLic.BaseAddress}api/licencia/obtener/{claveDto.Id}");


        //        //Esta OPCION FUNCIONA LOCAL
        //        //var absLic = new Uri(httpLic.BaseAddress, $"api/licencia/obtener/{claveDto.Id}");
        //        //var licencia = await httpLic.GetFromJsonAsync<LicenciaDto>(absLic);


        //        //ESTA OPCION PASANDO URL AZURE LITERAL - FUNCIONA EN AZURE
        //        //var urlLicencia = $"https://api-licencias-iurix-bgaea7bybdadhkca.canadacentral-01.azurewebsites.net/api/licencia/obtener/{claveDto.Id}";
        //        //var licencia = await httpLic.GetFromJsonAsync<LicenciaDto>(urlLicencia);


        //        //await JS.InvokeVoidAsync("console.log", $"[DEBUG] URL final de licencia: {Config.LicenciaBackendUrl}");


        //        var urlLicencia = $"{Config.LicenciaBackendUrl}api/licencia/obtener/{claveDto.Id}";
        //        var licencia = await httpLic.GetFromJsonAsync<LicenciaDto>(urlLicencia);



        //        //var licencia = await httpLic.GetFromJsonAsync<LicenciaDto>($"api/licencia/obtener/{claveDto.Id}");
        //        LicenciaActual = licencia;
        //        return licencia;
        //    }
        //    catch (Exception ex)
        //    {
        //        await JS.InvokeVoidAsync("console.log", $"-> Error obteniendo licencia: {ex.Message}");
        //        return null;
        //    }
        //}




        private async Task<LicenciaDto?> ObtenerLicenciaDesdeServidor()
        {
            try
            {
                // Cliente hacia el backend LOCAL del cliente
                var httpLocal = HttpFactory.CreateClient();
                httpLocal.BaseAddress = new Uri(Config.ServidorBackendUrl);
                httpLocal.DefaultRequestHeaders.Add("X-Internal-Key", "iurix-internal-2024");

                // 1) Obtener la clave / id de licencia desde el backend local
                var claveDto = await httpLocal.GetFromJsonAsync<ClaveLicenciaDto>("api/licencia/clave");
                if (claveDto == null || claveDto.Id == 0)
                    return null;

                // 2) Intentar obtener datos “online” desde el LicenciaBackend remoto
                try
                {
                    var httpLic = HttpFactory.CreateClient();
                    httpLic.BaseAddress = new Uri(Config.LicenciaBackendUrl);
                    httpLic.DefaultRequestHeaders.Add("X-Licencia-Key", "iurix-go-2024@ADMIN#");

                    var urlLicencia = $"{Config.LicenciaBackendUrl}api/Licencia/obtener/{claveDto.Id}";
                    var licenciaOnline = await httpLic.GetFromJsonAsync<LicenciaDto>(urlLicencia);

                    if (licenciaOnline != null)
                    {
                        LicenciaActual = licenciaOnline;
                        return licenciaOnline;
                    }
                }
                catch (Exception exRemoto)
                {
                    await JS.InvokeVoidAsync("console.log", $"-> Error consultando LicenciaBackend remoto: {exRemoto.Message}");
                    // caemos al fallback local
                }

                // 3) Fallback LOCAL: usar /api/licencia/datos, que lee licencia.lic del disco
                try
                {
                    var datosLocal = await httpLocal.GetFromJsonAsync<LicenciaDatosLocalDto>("api/licencia/datos");
                    if (datosLocal == null)
                        return null;

                    var licenciaLocal = new LicenciaDto
                    {
                        IdLicencia = datosLocal.Id,
                        Clave = datosLocal.Clave,
                        EstudioNombre = datosLocal.EstudioNombre,
                        DispositivoId = datosLocal.DispositivoId,
                        ValidoHasta = datosLocal.ValidoHasta,
                        //MaxUsuariosLocales = datosLocal.MaxUsuariosLocales,
                        //MaxDispositivos = datosLocal.MaxDispositivos,
                        Estado = datosLocal.Estado
                    };

                    LicenciaActual = licenciaLocal;
                    return licenciaLocal;
                }
                catch (Exception exLocal)
                {
                    await JS.InvokeVoidAsync("console.log", $"-> Error obteniendo datos de licencia local: {exLocal.Message}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("console.log", $"-> Error general obteniendo licencia: {ex.Message}");
                return null;
            }
        }








        private async Task OnLoginClicked()
        {
            if (LicenciaActual == null)
            {
                Mensaje = "No hay licencia válida.";
                return;
            }

            // Generar ID único de sesión
            var sesionId = DispositivoHelper.GenerarSesionId();

            SesionId = sesionId;

            // Guardar en localStorage
            //await JS.InvokeVoidAsync("localStorage.setItem", "dispositivo_sesion_id", sesionId);

            var dto = new UsuarioLoginDto
            {
                NombreUsuario = Usuario,
                Password = Password,
                IdLicencia = LicenciaActual.IdLicencia,
                DispositivoId = LicenciaActual.DispositivoId,           // Se obtiene de backend/licencia
                ClaveLicencia = LicenciaActual.Clave,
                DispositivoSesionId = sesionId,                         // El ID recién generado
                NombreDispositivo = Environment.MachineName,            // Si es Blazor Server
                TipoConexion = Config.EsClienteRemoto ? "Remoto" : "Local"
            };


            //PARA VER LOGIN EN CONSOLA QUE ENVIAMOS
            var jsonDto = JsonSerializer.Serialize(dto, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            //await JS.InvokeVoidAsync("console.log", $"Enviando Login:\n{jsonDto}");

            try
            {
                // Intento de login
                var loginResponse = await SesionService.LoginAsync(dto);
                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    await ProcesarLoginExitoso(loginResponse);
                    Nav.NavigateTo("/"); // Ir a la página principal
                }
                else
                {
                    Mensaje = "Credenciales inválidas o error de licencia.";
                }
            }
            catch (Exception ex)
            {
                Mensaje = $" Error: {ex.Message}";
            }
        }

        //private async Task ProcesarLoginExitoso(LoginResponseDto loginResponse)
        //{
        //    // Guardar token y permisos en localStorage
        //    if (!string.IsNullOrEmpty(loginResponse.Token))
        //        await JS.InvokeVoidAsync("localStorage.setItem", "token", loginResponse.Token);

        //    if (loginResponse.ExpiracionToken != default)
        //        await JS.InvokeVoidAsync("localStorage.setItem", "token_expiracion", loginResponse.ExpiracionToken.ToString("o"));

        //    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        //    var jwt = handler.ReadJwtToken(loginResponse.Token);
        //    var permisos = jwt.Claims.Where(c => c.Type == "Permiso").Select(c => c.Value).ToList();

        //    await JS.InvokeVoidAsync("localStorage.setItem", "permisos", JsonSerializer.Serialize(permisos));
        //    await JS.InvokeVoidAsync("localStorage.setItem", "usuario_nombre", loginResponse.Usuario?.NombreUsuario ?? "Usuario");
        //    await JS.InvokeVoidAsync("localStorage.setItem", "clave_licencia", loginResponse.Licencia?.Clave ?? "");
        //    await JS.InvokeVoidAsync("localStorage.setItem", "dispositivo_id", loginResponse.Licencia?.DispositivoId ?? "");
        //}

        private async Task HandleEnter(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await OnLoginClicked();
            }
        }



        private async Task ProcesarLoginExitoso(LoginResponseDto loginResponse)
        {

            // Guardar temporalmente la respuesta del login
            LoginPendiente = loginResponse;
            DebeGuardarLogin = true;
            await JS.InvokeVoidAsync("localStorage.setItem", "token", LoginPendiente.Token);
            await JS.InvokeVoidAsync("localStorage.setItem", "token_expiracion", LoginPendiente.ExpiracionToken.ToString("o"));



            //Iniciamos la funcion de control de inactividad por tema cierre de sesion, para mostrar aviso de cierre tambien
            await JS.InvokeVoidAsync("iniciarInactividad");

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(LoginPendiente.Token);      
            var permisos = jwt.Claims.Where(c => c.Type == "Permiso").Select(c => c.Value).ToList();

            if (loginResponse.Usuario is not null)
            {
                // Forzamos permisos desde el token por las dudas
                loginResponse.Usuario.Permisos = permisos;
                UsuarioActualService.SetUsuario(loginResponse.Usuario);
            }


            await JS.InvokeVoidAsync("localStorage.setItem", "permisos", JsonSerializer.Serialize(permisos));
            await JS.InvokeVoidAsync("localStorage.setItem", "usuario_nombre", LoginPendiente.Usuario?.NombreUsuario ?? "Usuario");
            await JS.InvokeVoidAsync("localStorage.setItem", "usuario_id", LoginPendiente.Usuario?.Id.ToString() ?? ""); // Nuevo
            await JS.InvokeVoidAsync("localStorage.setItem", "clave_licencia", LoginPendiente.Licencia?.Clave ?? "");
            await JS.InvokeVoidAsync("localStorage.setItem", "dispositivo_id", LoginPendiente.Licencia?.DispositivoId ?? "");
            await JS.InvokeVoidAsync("localStorage.setItem", "dispositivo_sesion_id", SesionId ?? "");

            //Guardamos el usuario completo
            await JS.InvokeVoidAsync("localStorage.setItem", "usuario_json",
                JsonSerializer.Serialize(loginResponse.Usuario));

            //await JS.InvokeVoidAsync("console.log", $"La sesion del dispositivo es: {SesionId}");

            var token = loginResponse.Token;
            // Cargar el token en el TokenStore del servidor Blazor
            await TokenStore.SetAsync(loginResponse.Token);

            //Deshabilitar en Prod
            //await JS.InvokeVoidAsync("console.log", $"{token}");

            //await JS.InvokeVoidAsync("console.log", $"{LoginPendiente.Usuario?.Id.ToString()}");


            //System.Diagnostics.Debug.WriteLine($"[LOGIN] Set Token en TokenStore#{TokenStore.GetHashCode()}");


            //Notificar que tenemos token y cambia el estado autorizado para poder acceder a los modulos
            //((CustomAuthenticationStateProvider)AuthStateProvider).NotifyUserAuthentication(loginResponse.Token);
            await ((CustomAuthenticationStateProvider)AuthStateProvider)
                .NotifyUserAuthentication(loginResponse.Token);

            //Aca borramos el agenda shown set item para mostrar cada vez que se ingresa; si sacamos esto solo muestra al usuario la primera vez que ingresa
            //  IMPORTANTE: resetear agenda para este nuevo login
            await JS.InvokeVoidAsync("sessionStorage.removeItem", "agenda_shown");

            // ?? Usar NavigateTo con "forceLoad: true" para asegurar que se recargue el contexto
            Nav.NavigateTo("/main");

            /* Nav.NavigateTo("/main", forceLoad: true); */// evita renders previos sin token
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (DebeGuardarLogin && LoginPendiente != null)
            {
                DebeGuardarLogin = false;

                try
                {
                    // Guardar token y datos en localStorage
                    await JS.InvokeVoidAsync("localStorage.setItem", "token", LoginPendiente.Token);
                    await JS.InvokeVoidAsync("localStorage.setItem", "token_expiracion", LoginPendiente.ExpiracionToken.ToString("o"));

                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(LoginPendiente.Token);
                    var permisos = jwt.Claims.Where(c => c.Type == "Permiso").Select(c => c.Value).ToList();

                    await JS.InvokeVoidAsync("localStorage.setItem", "permisos", JsonSerializer.Serialize(permisos));
                    await JS.InvokeVoidAsync("localStorage.setItem", "usuario_nombre", LoginPendiente.Usuario?.NombreUsuario ?? "Usuario");
                    await JS.InvokeVoidAsync("localStorage.setItem", "usuario_id", LoginPendiente.Usuario?.Id.ToString() ?? ""); // Nuevo
                    await JS.InvokeVoidAsync("localStorage.setItem", "clave_licencia", LoginPendiente.Licencia?.Clave ?? "");
                    await JS.InvokeVoidAsync("localStorage.setItem", "dispositivo_id", LoginPendiente.Licencia?.DispositivoId ?? "");
                    await JS.InvokeVoidAsync("localStorage.setItem", "dispositivo_sesion_id", LoginPendiente.Licencia?.DispositivoSesionId ?? "");

                    //Console.WriteLine("? Datos guardados en localStorage correctamente.");


                }
                catch (Exception ex)
                {
                    Console.WriteLine($"? Error guardando datos en localStorage: {ex.Message}");
                }
            }
        }





        private async Task OnLicenciaSeleccionada(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file is null)
                return;

            try
            {
                using var ms = new MemoryStream();
                await file.OpenReadStream(maxAllowedSize: 1024 * 1024).CopyToAsync(ms); // 1MB por ejemplo

                var base64 = Convert.ToBase64String(ms.ToArray());

                var http = HttpFactory.CreateClient();
                http.BaseAddress = new Uri(Config.ServidorBackendUrl);

                var resp = await http.PostAsJsonAsync("api/licencia/registrar-licencia", new
                {
                    ArchivoBase64 = base64
                });

                var content = await resp.Content.ReadAsStringAsync();

                if (resp.IsSuccessStatusCode)
                {
                    LicenciaUploadMensaje = "Licencia registrada correctamente. Vuelva a intentar iniciar sesión.";
                    // Podrías incluso llamar a VerificarLicenciaAlIniciar() de nuevo
                    await VerificarLicenciaAlIniciar();
                }
                else
                {
                    LicenciaUploadMensaje = $"Error al registrar licencia: {content}";
                }
            }
            catch (Exception ex)
            {
                LicenciaUploadMensaje = $"Error al cargar licencia: {ex.Message}";
            }
        }







    }

}