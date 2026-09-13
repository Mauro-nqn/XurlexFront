using IurixBlazor.Data;
using IurixBlazor.Services;
using IurixBlazor.Services.Auth;
using IurixBlazor.Services.Interfaces;
using IurixBlazor.Services.Windowing;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;



var builder = WebApplication.CreateBuilder(args);



// Crear y registrar config
//var configService = new ConfigService();
//configService.InicializarAsync().GetAwaiter().GetResult();
//builder.Services.AddSingleton(configService);

var configService = new ConfigService();
configService.LeerConfiguracion();                         // <-- LEE YA
System.Diagnostics.Debug.WriteLine($"[CFG-BOOT] AuthMode={configService.AuthMode}  URL={configService.ServidorBackendUrl}");
builder.Services.AddSingleton(configService);

// Si estás en local y querés que Blazor escuche en una IP/puerto fijos:
bool isAzure = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
if (!isAzure)
{
    // si querés forzar por ini: p.ej., puerto 7000
    builder.WebHost.UseUrls($"http://{configService.IpServidor}:7000");
}


// Add services to the container.
builder.Services.AddRazorPages();
//builder.Services.AddServerSideBlazor();

//builder.Services.AddServerSideBlazor()
//    .AddHubOptions(options =>
//    {
//        options.ClientTimeoutInterval = TimeSpan.FromMinutes(30);
//        options.KeepAliveInterval = TimeSpan.FromMinutes(1);
//        options.HandshakeTimeout = TimeSpan.FromSeconds(15);
//    });

//builder.Services.AddServerSideBlazor()
//    .AddHubOptions(options =>
//    {
//        options.ClientTimeoutInterval = TimeSpan.FromMinutes(30);
//        options.KeepAliveInterval = TimeSpan.FromMinutes(1);
//        options.HandshakeTimeout = TimeSpan.FromSeconds(15);
//    })
//    .AddCircuitOptions(options =>
//    {
//        // Cuánto tiempo mantener el circuito "esperando" reconexión
//        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(20);

//        // Timeout razonable para llamadas JS largas (por ejemplo, cosas con CKEditor, etc.)
//        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(60);

//        // Para evitar buffer infinito si el cliente se cuelga
//        options.MaxBufferedUnacknowledgedRenderBatches = 10;
//    });


builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(30);
        options.KeepAliveInterval = TimeSpan.FromMinutes(1);
        options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    })
    .AddCircuitOptions(options =>
    {
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(20);
        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(60);
        options.MaxBufferedUnacknowledgedRenderBatches = 10;
    });





//builder.Services.AddSingleton<WeatherForecastService>();


//Esto para usar en cada consulta http el token
//  Cambiá a Singleton y eliminá cualquier registro previo de ITokenStore
builder.Services.AddSingleton<ITokenStore, MemoryTokenStore>();

// El handler puede ser transient o scoped; transient está bien
builder.Services.AddTransient<JwtAuthorizationMessageHandler>();




//ESTO USABAMOS ANTES SIN TOKEN

//builder.Services.AddScoped<IApiSesionService, ApiSesionService>();








// Registrar ConfigService como Singleton
// Inicializar config.ini
//var configService = new ConfigService();
//configService.InicializarAsync().GetAwaiter().GetResult();
//builder.Services.AddSingleton(configService);

//// Forzar Blazor a escuchar en la IP del config.ini pero en puerto 7000
//builder.WebHost.UseUrls($"http://{configService.IpServidor}:7000");

//// HttpClient para API principal
//builder.Services.AddHttpClient<ApiSesionService>((provider, client) =>
//{
//    var config = provider.GetRequiredService<ConfigService>();
//    client.BaseAddress = new Uri(config.ServidorBackendUrl);
//});

//// HttpClient para Licencia Backend
////builder.Services.AddHttpClient("LicenciaBackend", (provider, client) =>
////{
////    var config = provider.GetRequiredService<ConfigService>();
////    client.BaseAddress = new Uri(config.LicenciaBackendUrl);
////});

//builder.Services.AddHttpClient<IApiSesionService, ApiSesionService>((provider, client) =>
//{
//    var config = provider.GetRequiredService<ConfigService>();
//    client.BaseAddress = new Uri(config.ServidorBackendUrl); // <- Aquí debe ser algo como http://192.168.1.100:5000/
//});












//// HttpClient para API principal (tipado o no)
//builder.Services.AddHttpClient<ApiSesionService>((provider, client) =>
//{
//    var cfg = provider.GetRequiredService<ConfigService>();
//    client.BaseAddress = new Uri(cfg.ServidorBackendUrl);
//});


//// HttpClient para API principal (tipado o no)
//builder.Services.AddHttpClient<ApiSesionService>((provider, client) =>
//{
//    var cfg = provider.GetRequiredService<ConfigService>();
//    client.BaseAddress = new Uri(cfg.ServidorBackendUrl);
//})
//    .AddHttpMessageHandler<JwtAuthorizationMessageHandler>();


///////CAMBIOS////


//ESTO USABAMOS ANTES SIN TOKEN
// Variante con interfaz
//builder.Services.AddHttpClient<IApiSesionService, ApiSesionService>((provider, client) =>
//{
//    var cfg = provider.GetRequiredService<ConfigService>();
//    client.BaseAddress = new Uri(cfg.ServidorBackendUrl);
//});






// (Opcional) Si más adelante necesitás llamar directo al backend de licencias desde el front:
// builder.Services.AddHttpClient("LicenciaBackend", (provider, client) =>
// {
//     var cfg = provider.GetRequiredService<ConfigService>();
//     client.BaseAddress = new Uri(cfg.LicenciaBackendUrl);
// });



//ESTO USAMOS CON TOKEN AHORA

// Este ya existe, solo agregá el handler:

// HttpClient para AgendaService (tipado) con BaseAddress y handler JWT
//builder.Services.AddHttpClient<AgendaService>((sp, client) =>
//{
//    var cfg = sp.GetRequiredService<ConfigService>();
//    client.BaseAddress = new Uri(cfg.ServidorBackendUrl);
//})
//.AddHttpMessageHandler(sp => sp.GetRequiredService<JwtAuthorizationMessageHandler>());




builder.Services.AddHttpClient("Api", (sp, client) =>
{
    var c = sp.GetRequiredService<ConfigService>();
    Console.WriteLine($"[ApiClient] BaseAddress={c.ServidorBackendUrl}  AuthMode={c.AuthMode}");
    client.BaseAddress = new Uri(c.ServidorBackendUrl);

    //  AUMENTAR TIMEOUT PARA LLAMADAS LARGAS COMO LA SINCRONIZACIÓN DEXTRA
    client.Timeout = TimeSpan.FromMinutes(5); // o el valor que prefieras
})
.AddHttpMessageHandler<JwtAuthorizationMessageHandler>();


// Igual para IApiSesionService (si querés que también lleve JWT cuando corresponde)
builder.Services.AddHttpClient<IApiSesionService, ApiSesionService>((sp, client) =>
{
    var cfg = sp.GetRequiredService<ConfigService>();
    client.BaseAddress = new Uri(cfg.ServidorBackendUrl);
});

//builder.Services.AddHttpClient<ApiSesionService>((sp, client) =>
//{
//    var cfg = sp.GetRequiredService<ConfigService>();
//    client.BaseAddress = new Uri(cfg.ServidorBackendUrl);
//});
//  No agregues handler acá














// UsuarioService inyectando ConfigService y IJSRuntime
builder.Services.AddScoped<UsuarioService>();


// HttpClient para AgendaService (tipado) con BaseAddress y handler JWT
//builder.Services.AddHttpClient<UsuarioService>((sp, client) =>
//{
//    var cfg = sp.GetRequiredService<ConfigService>();
//    client.BaseAddress = new Uri(cfg.ServidorBackendUrl);
//})
//.AddHttpMessageHandler(sp => sp.GetRequiredService<JwtAuthorizationMessageHandler>());


// EscritoService inyectando ConfigService y IJSRuntime
builder.Services.AddScoped<EscritoService>();

builder.Services.AddScoped<GoogleService>();

builder.Services.AddScoped<CondicionIvaService>();

builder.Services.AddScoped<PersonaService>();

builder.Services.AddScoped<DistribucionHonorarioService>();

builder.Services.AddScoped<PresupuestoService>();

builder.Services.AddScoped<IvaAlicuotaService>();

builder.Services.AddScoped<CertService>();

builder.Services.AddScoped<AfipAuthService>();

builder.Services.AddScoped<PuntoVentaService>();

builder.Services.AddScoped<TipoComprobanteService>();

builder.Services.AddScoped<AfipFacturaService>();

builder.Services.AddScoped<FacturaService>();

builder.Services.AddScoped<ComprobanteAfipService>();

builder.Services.AddScoped<FacturaService>();

builder.Services.AddScoped<DomicilioService>();

builder.Services.AddScoped<JurisdiccionService>();

builder.Services.AddScoped<CircunscripcionService>();

builder.Services.AddScoped<SecretariaService>();

builder.Services.AddScoped<JuzgadoService>();

builder.Services.AddScoped<CaracterIntervencionService>();

builder.Services.AddScoped<TipoEstadoProcesoService>();

builder.Services.AddScoped<GrupoGestionService>();

builder.Services.AddScoped<TipoProcesoService>();

builder.Services.AddScoped<GestionService>();

builder.Services.AddScoped<AgendaService>();


builder.Services.AddScoped<TipoAgendamientoService>();

builder.Services.AddScoped<MovimientoService>();

builder.Services.AddScoped<ArchivoMovimientoService>();

builder.Services.AddScoped<RubroService>();
builder.Services.AddScoped<CentroCostoService>();
builder.Services.AddScoped<MedioPagoService>();

builder.Services.AddScoped<CuentaCorrienteService>();
builder.Services.AddScoped<ReciboService>();

builder.Services.AddScoped<EstudioConfigService>();

builder.Services.AddScoped<FacturaPresupuestoService>();

builder.Services.AddScoped<AjusteVariableService>();

builder.Services.AddScoped<EscritoMovimientoService>();


builder.Services.AddScoped<ProcuracionService>();

builder.Services.AddScoped<DextraService>();


// Registrar servicio de manejo de archivos
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

//Para mantener info de editor entre actualizaciones e info del movimiento - mejorar
//No resiste F5
builder.Services.AddSingleton<EditorInfoService>();
builder.Services.AddSingleton<MovimientoTemporalService>();


//Si hacemos esto se crea una unica instancia para todas las terminales
//builder.Services.AddSingleton<WindowService>();

// La forma correcta de registrarlo
builder.Services.AddScoped<WindowService>();

builder.Services.AddScoped<DeviceService>();


builder.Services.AddSingleton<PersonaEventService>();

builder.Services.AddScoped<InformeHonorariosService>();

builder.Services.AddScoped<ParteProcesoService>();

builder.Services.AddScoped<CertificadoApremioService>();

builder.Services.AddScoped<ProcesoJudicialService>();

builder.Services.AddScoped<RolService>();

builder.Services.AddScoped<UsuarioActualService>();

builder.Services.AddScoped<ProcesoListadoService>();




builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();








//builder.Services.AddServerSideBlazor()
//    .AddHubOptions(options =>
//    {
//        options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
//    });

builder.Services
    .AddServerSideBlazor()
    .AddHubOptions(o =>
    {
        o.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB
    })
    .AddCircuitOptions(o =>
    {
        o.DetailedErrors = true; //  muestra el stack real en la consola del navegador
        // o.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3); // opcional
    });

// si usás HttpClient hacia tus APIs, registralo con factory acá (opcional)
// builder.Services.AddHttpClient("Api", c => c.BaseAddress = new Uri("http://192.168.1.100:5000"));



var supportedCultures = new[] { new CultureInfo("es-AR") };
CultureInfo.DefaultThreadCurrentCulture = supportedCultures[0];
CultureInfo.DefaultThreadCurrentUICulture = supportedCultures[0];








//builder.Services.AddAuthorization();
//builder.Services.AddAuthentication();
builder.Services.AddControllers();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}





//  Si estás sirviendo TODO por HTTP en LAN (192.168.x.x:7000),
// podés omitir UseHttpsRedirection() para no redirigir a un endpoint HTTPS inexistente.

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

//app.UseAuthentication();        //  si usás auth
//app.UseAuthorization();

app.MapControllers();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host"); // Ya existe
// Redirigir automáticamente si la raíz "/" es accedida
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/login");
        return;
    }
    await next();
});





app.Run();

Console.WriteLine($"?? Entorno actual: {builder.Environment.EnvironmentName}");
Console.WriteLine($"?? ContentRootPath: {app.Environment.ContentRootPath}");
Console.WriteLine($"?? WebRootPath: {app.Environment.WebRootPath}");