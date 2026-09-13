using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using System.Drawing;
using System.Globalization;
using System.Security.Cryptography.Xml;
using System.Text;
using static System.Net.WebRequestMethods;


public class PresupuestoVerBase : ComponentBase
{
    [Parameter] public int Id { get; set; }

    [Inject] protected PresupuestoService PresupuestoService { get; set; } = default!;
    [Inject] protected PersonaService PersonaService { get; set; } = default!;
    [Inject] protected UsuarioService UsuarioService { get; set; } = default!;
    [Inject] protected GoogleService GoogleService { get; set; } = default!;
    [Inject] protected EstudioConfigService EstudioConfigService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;


    [Inject] protected AjusteVariableService AjusteVariableSrv { get; set; } = default!;

    protected PresupuestoDto Presupuesto { get; set; } = new();
    protected List<PresupuestoDetalleDto> Detalles { get; set; } = new();
    protected PersonaDto? Persona { get; set; }
    protected EstudioConfigDto? Estudio { get; set; }

    protected bool _closingModal;

    // Branding
    protected string LogoUrl { get; set; } = "";
    protected string NombreEstudio { get; set; } = "Estudio Jurídico";
    protected string PieEstudio { get; set; } = "Dirección • Teléfono • www.tusitio.com";
    protected string ClienteLinea1 => Persona is null
        ? $"Cliente #{Presupuesto.PersonaId}"
        : (!string.IsNullOrWhiteSpace(Persona.RazonSocial) ? Persona.RazonSocial : $"{Persona.Apellido}, {Persona.Nombre}");
    protected string NotaAlPie { get; set; } = "Los valores incluyen IVA. Vigencia: 15 días.";


    // Modal enviar
    protected bool MostrarEnviar { get; set; } = false;
    protected bool AdjuntarPdf { get; set; } = true;
    protected List<UsuarioDto> Usuarios { get; set; } = new();
    protected int UsuarioSeleccionadoId { get; set; }
    protected string EmailVinculado { get; set; } = "";
    protected string Destinatario { get; set; } = "";
    protected string AsuntoCorreo { get; set; } = "";
    protected string CuerpoCorreo { get; set; } = "";
    protected bool Enviando { get; set; } = false;




    protected int Tab { get; set; } = 0;
    protected string IntroHtml { get; set; } = "";
    protected string OutroHtml { get; set; } = "";
    protected bool GenerandoPreview { get; set; } = false;
    protected string? PreviewPdfBase64 { get; set; } // cache en base64 para adjuntar sin regenerar


    protected static string ToBase64(string s) => Convert.ToBase64String(Encoding.UTF8.GetBytes(s ?? ""));


    protected IJSObjectReference? _editorModule;
    protected bool _editorsReady;
    protected bool _pendingInitEditors;
    protected Guid _editorDomKey = Guid.NewGuid(); // para forzar DOM fresco



    protected string? HtmlVistaPresupuesto;




    protected string? PdfPreviewUrl { get; set; }
    protected bool MostrarPdfPreview { get; set; }
    private byte[]? _pdfBytesCache;




    public class EquivalenciaVm
    {
        public string Etiqueta { get; set; } = "";
        public decimal Unidades { get; set; }
        // Valor con el que se valuó el presupuesto (snapshot)
        public decimal ValorUsado { get; set; }

        // Opcional: valor actual de la variable, por si querés compararlo o mostrarlo después
        public decimal? ValorActual { get; set; }
        public decimal TotalAsignado { get; set; }


    }

    protected List<EquivalenciaVm> Equivalencias { get; set; } = new();
    private Dictionary<int, AjusteVariableDto> _vars = new();





    protected bool esMovil = false;



    protected override async Task OnInitializedAsync()
    {
        Presupuesto = await PresupuestoService.ObtenerPorIdAsync(Id) ?? new PresupuestoDto();
        Detalles = await PresupuestoService.ObtenerDetallesAsync(Id) ?? new List<PresupuestoDetalleDto>();
        Persona = await PersonaService.ObtenerPersonaPorIdAsync(Presupuesto.PersonaId);
        Estudio = await EstudioConfigService.ObtenerAsync();

        Destinatario = Persona?.Email ?? "";
        AsuntoCorreo = $"Presupuesto #{Presupuesto.Id} - {ClienteLinea1}";
        CuerpoCorreo = "En adjunto enviamos presupuesto por honorarios en base a lo conversado.\n" +
                       "Quedamos a su disposición por cualquier consulta o comentario.\n" +
                       "Por favor confirmar recepción.\n" +
                       "Saludos cordiales.\n\n"  +
                       $"{Estudio!.Nombre}\n" +
                       $"{Estudio.Domicilio} - {Estudio.Localidad} \n" +
                       $"{Estudio.Email}\n" +
                       $"{Estudio.Web}";


        NotaAlPie = $"Validez del presupuesto {Presupuesto.PlazoValidezDias} días";

        // 2) Branding desde EstudioConfig
        await CargarEstudioConfigAsync();

        Usuarios = await UsuarioService.ObtenerUsuariosAsync();

        // Abrir modal si viene ?enviar=1
        var uri = new Uri(Nav.Uri);
        var qs = QueryHelpers.ParseQuery(uri.Query);
        if (qs.TryGetValue("enviar", out var v) && v.ToString() == "1")
            MostrarEnviar = true;

        // Token (si usás auth en GoogleService)
        var token = await JS.InvokeAsync<string>("localStorage.getItem", "token");
        if (!string.IsNullOrEmpty(token))
            GoogleService.ConfigurarToken(token);


        // Cargar variables vigentes (para mostrar Clave/Nombre/ValorActual)
        var vars = await AjusteVariableSrv.ObtenerTodosAsync() ?? new();
        _vars = vars.ToDictionary(v => v.Id, v => v);

        CalcularEquivalencias();
    }




    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // importa el módulo (cacheado por el browser)
            _editorModule = await JS.InvokeAsync<IJSObjectReference>("import", "/editor/editorInterop.js?v=1");


            var width = await JS.InvokeAsync<int>("obtenerAnchoPantalla");
            esMovil = width < 768; // breakpoint Bootstrap sm/md

            StateHasChanged();
        }

        // inicializar editores cuando el modal ya se renderizó y estamos en la pestaña Redacción
        if (_pendingInitEditors && MostrarEnviar && Tab == 0 && _editorModule is not null)
        {
            var base64Intro = Convert.ToBase64String(Encoding.UTF8.GetBytes(IntroHtml ?? ""));
            var base64Outro = Convert.ToBase64String(Encoding.UTF8.GetBytes(OutroHtml ?? ""));

            await _editorModule.InvokeVoidAsync("initMiniEditor", "editorIntro", base64Intro);
            await _editorModule.InvokeVoidAsync("initMiniEditor", "editorOutro", base64Outro);

            _editorsReady = true;
            _pendingInitEditors = false;
            // No hace falta StateHasChanged aquí
        }
    }



    protected async Task VerAdjuntoPantallaCompleta()
    {
        var html = BuildPresupuestoAdjuntoHtmlCompleto();
        await JS.InvokeVoidAsync("iurixAbrirAdjuntoHtml", html);
    }

    //private void CalcularEquivalencias()
    //{
    //    Equivalencias.Clear();

    //    // Tomar solo renglones con variable asociada (por DTO tenés solo el Id)
    //    var conVar = Detalles.Where(d => d.AjusteVariableId.HasValue).ToList();
    //    if (!conVar.Any()) return;

    //    var netoTotal = Detalles.Sum(d => d.Subtotal);
    //    if (netoTotal <= 0m) return;

    //    var grupos = conVar.GroupBy(d => d.AjusteVariableId!.Value);

    //    foreach (var g in grupos)
    //    {
    //        var netoGrupo = g.Sum(d => d.Subtotal);
    //        var proporcion = netoGrupo / netoTotal;
    //        var totalGrupo = Math.Round(Presupuesto.Total * proporcion, 2, MidpointRounding.AwayFromZero);

    //        // Valor actual desde catálogo; si no está, caemos al snapshot guardado en el detalle
    //        var varSel = _vars.TryGetValue(g.Key, out var vdto) ? vdto : null;
    //        var valorAct = varSel?.ValorActual ?? g.First().VariableValorUsado ?? 0m;
    //        if (valorAct <= 0m) continue;

    //        var etiqueta = varSel is null
    //            ? $"Variable #{g.Key}"
    //            : $"{varSel.Nombre} ({varSel.Clave})";

    //        var unidades = Math.Round(totalGrupo / valorAct, 4, MidpointRounding.AwayFromZero);

    //        Equivalencias.Add(new EquivalenciaVm
    //        {
    //            Etiqueta = etiqueta,
    //            Unidades = unidades,
    //            ValorActual = valorAct,
    //            TotalAsignado = totalGrupo
    //        });
    //    }
    //}

    private void CalcularEquivalencias()
    {
        Equivalencias.Clear();

        // Sólo renglones con variable asociada
        var conVar = Detalles.Where(d => d.AjusteVariableId.HasValue).ToList();
        if (!conVar.Any()) return;

        var netoTotal = Detalles.Sum(d => d.Subtotal);
        if (netoTotal <= 0m) return;

        var grupos = conVar.GroupBy(d => d.AjusteVariableId!.Value);

        foreach (var g in grupos)
        {
            var netoGrupo = g.Sum(d => d.Subtotal);
            var proporcion = netoGrupo / netoTotal;
            var totalGrupo = Math.Round(Presupuesto.Total * proporcion, 2, MidpointRounding.AwayFromZero);

            // Variable asociada (catálogo)
            _vars.TryGetValue(g.Key, out var varSel);

            // 👉 Valor usado en el presupuesto (snapshot por renglón)
            var valorUsado = g.First().VariableValorUsado
                             ?? varSel?.ValorActual
                             ?? 0m;

            if (valorUsado <= 0m) continue;

            // Valor actual de la variable (solo para info, opcional)
            var valorActual = varSel?.ValorActual;

            var etiqueta = varSel is null
                ? $"Variable #{g.Key}"
                : $"{varSel.Nombre} ({varSel.Clave})";

            // Las unidades se calculan con el valor USADO, no el actual
            var unidades = Math.Round(totalGrupo / valorUsado, 4, MidpointRounding.AwayFromZero);

            Equivalencias.Add(new EquivalenciaVm
            {
                Etiqueta = etiqueta,
                Unidades = unidades,
                ValorUsado = valorUsado,
                ValorActual = valorActual,
                TotalAsignado = totalGrupo
            });
        }
    }



    private async Task CargarEstudioConfigAsync()
    {
        var cfg = await EstudioConfigService.ObtenerAsync(); // puede ser null si aún no creaste el registro
        if (cfg == null) return;

        // Nombre
        if (!string.IsNullOrWhiteSpace(cfg.Nombre))
            NombreEstudio = cfg.Nombre;

        // Logo
        if (!string.IsNullOrWhiteSpace(cfg.LogoBase64))
            LogoUrl = $"data:image/png;base64,{cfg.LogoBase64}";

        // Pie / datos de contacto
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(cfg.Domicilio)) partes.Add(cfg.Domicilio);
        if (!string.IsNullOrWhiteSpace(cfg.Telefono)) partes.Add(cfg.Telefono);
        if (!string.IsNullOrWhiteSpace(cfg.Web)) partes.Add(cfg.Web);
        if (!string.IsNullOrWhiteSpace(cfg.Email)) partes.Add(cfg.Email);

        if (partes.Count > 0)
            PieEstudio = string.Join(" • ", partes);

        // Nota opcional para PDF/email desde Observaciones
        if (!string.IsNullOrWhiteSpace(cfg.Observaciones))
            NotaAlPie = cfg.Observaciones;
    }


    protected void Volver() => Nav.NavigateTo("/presupuestos");

    protected async Task Imprimir() => await JS.InvokeVoidAsync("window.print");

    //protected void AbrirEnviar() => MostrarEnviar = true;
    //protected void CerrarEnviar() => MostrarEnviar = false;





    private async Task SyncEditorsToState()
    {
        if (_editorModule is null) return;
        IntroHtml = await _editorModule.InvokeAsync<string>("getEditorContent", "editorIntro") ?? "";
        OutroHtml = await _editorModule.InvokeAsync<string>("getEditorContent", "editorOutro") ?? "";
    }



    //protected void AbrirEnviar()
    //{
    //    MostrarEnviar = true;
    //    _ = JS.InvokeVoidAsync("eval", "document.documentElement.classList.add('no-scroll');document.body.classList.add('no-scroll');");
    //}

    //protected void CerrarEnviar()
    //{
    //    MostrarEnviar = false;
    //    _ = JS.InvokeVoidAsync("eval", "document.documentElement.classList.remove('no-scroll');document.body.classList.remove('no-scroll');");
    //}

    //protected void AbrirEnviar()
    //{
    //    MostrarEnviar = true;
    //    _pendingInitEditors = true;      // pedimos init luego del render
    //    _editorDomKey = Guid.NewGuid();  // fuerza DOM nuevo para los textareas
    //    _ = JS.InvokeVoidAsync("eval", "document.documentElement.classList.add('no-scroll');document.body.classList.add('no-scroll');");
    //}

    //protected async void CerrarEnviar()
    //{
    //    if (_editorsReady && _editorModule is not null)
    //    {
    //        await _editorModule.InvokeVoidAsync("destroyEditor", "editorIntro");
    //        await _editorModule.InvokeVoidAsync("destroyEditor", "editorOutro");
    //    }
    //    MostrarEnviar = false;
    //    _editorsReady = false;
    //    _pendingInitEditors = false;
    //    _ = JS.InvokeVoidAsync("eval", "document.documentElement.classList.remove('no-scroll');document.body.classList.remove('no-scroll');");
    //}

    //protected async Task CerrarEnviar()
    //{
    //    if (_closingModal) return;
    //    _closingModal = true;

    //    // 1) Ocultar YA el modal (sin esperar nada)
    //    MostrarEnviar = false;
    //    _editorsReady = false;
    //    _pendingInitEditors = false;
    //    StateHasChanged();             // fuerza re-render y oculta el modal

    //    // 2) Soltá el hilo para que el render se aplique
    //    await Task.Yield();

    //    // 3) Desbloquear scroll del fondo
    //    _ = JS.InvokeVoidAsync("eval",
    //        "document.documentElement.classList.remove('no-scroll');document.body.classList.remove('no-scroll');");

    //    // 4) Recién ahora destruí CKEditor (si todavía existe)
    //    try
    //    {
    //        if (_editorModule is not null)
    //        {
    //            await _editorModule.InvokeVoidAsync("destroyEditor", "editorIntro");
    //            await _editorModule.InvokeVoidAsync("destroyEditor", "editorOutro");
    //        }
    //    }
    //    catch { /* no romper el cierre si ya no existe */ }
    //    finally
    //    {
    //        _closingModal = false;
    //    }
    //}




    protected async Task AbrirEnviar()
    {
        MostrarEnviar = true;

        // forzar re-render (aparece el modal pero también ya está #print-root)
        StateHasChanged();
        await Task.Yield();

        // bloquear scroll del fondo
        _ = JS.InvokeVoidAsync("eval",
            "document.documentElement.classList.add('no-scroll');document.body.classList.add('no-scroll');");

        try
        {
            // 🔹 leer el HTML de la vista imprimible
            HtmlVistaPresupuesto = await JS.InvokeAsync<string>("presupuesto.getPrintHtml");

            // si vino null o vacío, caemos a la versión simple
            if (string.IsNullOrWhiteSpace(HtmlVistaPresupuesto))
            {
                HtmlVistaPresupuesto = BuildPresupuestoHtml();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error JS getPrintHtml: {ex.Message}");
            HtmlVistaPresupuesto = BuildPresupuestoHtml();
        }
    }




    protected async Task CerrarEnviar()
    {
        MostrarEnviar = false;
        _ = JS.InvokeVoidAsync("eval",
            "document.documentElement.classList.remove('no-scroll');document.body.classList.remove('no-scroll');");
        await Task.CompletedTask;
    }




    protected async Task SetTab(int tab)
    {
        if (Tab == tab) return;

        if (tab == 1 && _editorsReady && _editorModule is not null)
        {
            // vamos a preview: destruí instancias para evitar fugas y conflictos de DOM
            await _editorModule.InvokeVoidAsync("destroyEditor", "editorIntro");
            await _editorModule.InvokeVoidAsync("destroyEditor", "editorOutro");
            _editorsReady = false;
        }

        Tab = tab;

        if (tab == 0 && MostrarEnviar)
        {
            _editorDomKey = Guid.NewGuid(); // fuerza textareas nuevos
            _pendingInitEditors = true;     // reinit en próximo render
            StateHasChanged();
        }
    }




    protected void OnUsuarioSeleccionado(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var id))
            UsuarioSeleccionadoId = id;
    }

    protected async Task ConsultarEmailVinculado()
    {
        if (UsuarioSeleccionadoId == 0) { await Toast("Seleccioná un usuario.", "warning"); return; }
        var estado = await GoogleService.ObtenerEmailGoogleAsync(UsuarioSeleccionadoId);
        EmailVinculado = string.IsNullOrEmpty(estado?.Email) ? "Sin cuenta vinculada" : estado.Email;
    }


    //protected async Task GenerarPreviewPdf()
    //{
    //    if (GenerandoPreview) return;
    //    GenerandoPreview = true;
    //    try
    //    {
    //        var bytes = await PresupuestoService.ObtenerPdfPreviewAsync(Id, IntroHtml, OutroHtml);
    //        if (bytes is null || bytes.Length == 0)
    //        {
    //            await Toast("No se pudo generar la vista previa.", "error");
    //            return;
    //        }
    //        PreviewPdfBase64 = Convert.ToBase64String(bytes);
    //        Tab = 1; // cambiar a pestaña Vista previa
    //    }
    //    catch (Exception ex)
    //    {
    //        await Toast($"Error generando preview: {ex.Message}", "error");
    //    }
    //    finally
    //    {
    //        GenerandoPreview = false;
    //    }
    //}

    protected async Task GenerarPreviewPdf()
    {
        if (GenerandoPreview) return;
        GenerandoPreview = true;
        try
        {
            await SyncEditorsToState(); // 🔹 toma el HTML de CKEditor

            var bytes = await PresupuestoService.ObtenerPdfPreviewAsync(Id, IntroHtml, OutroHtml);
            if (bytes is null || bytes.Length == 0)
            {
                await Toast("No se pudo generar la vista previa.", "error");
                return;
            }
            PreviewPdfBase64 = Convert.ToBase64String(bytes);
            Tab = 1;
        }
        catch (Exception ex)
        {
            await Toast($"Error generando preview: {ex.Message}", "error");
        }
        finally
        {
            GenerandoPreview = false;
        }
    }



    //protected async Task EnviarCorreo()
    //{
    //    if (UsuarioSeleccionadoId == 0) { await Toast("Seleccioná usuario remitente.", "warning"); return; }
    //    if (string.IsNullOrWhiteSpace(Destinatario)) { await Toast("Ingresá el destinatario.", "warning"); return; }

    //    if (!await GoogleService.ValidarYRefrescarTokenAsync(UsuarioSeleccionadoId))
    //    {
    //        await Toast("Debes vincular/refrescar tu cuenta de Google.", "error");
    //        return;
    //    }

    //    Enviando = true;
    //    try
    //    {


    //        if (AdjuntarPdf)
    //        {
    //            // 1) Si ya generaste preview, usá ese
    //            byte[] pdfBytes;
    //            if (!string.IsNullOrEmpty(PreviewPdfBase64))
    //            {
    //                pdfBytes = Convert.FromBase64String(PreviewPdfBase64);
    //            }
    //            else
    //            {
    //                // si no hay preview, generá rápido (con intro/outro actuales)
    //                var b = await PresupuestoService.ObtenerPdfPreviewAsync(Id, IntroHtml, OutroHtml);
    //                if (b is null || b.Length == 0)
    //                {
    //                    await Toast("No se pudo generar el PDF a adjuntar.", "error");
    //                    return;
    //                }
    //                pdfBytes = b;
    //                PreviewPdfBase64 = Convert.ToBase64String(pdfBytes); // cache por si hace falta
    //            }

    //            var dto = new CrearEmailConAdjuntoDto
    //            {
    //                Destinatario = Destinatario,
    //                Asunto = AsuntoCorreo,
    //                CuerpoHtml = CuerpoCorreo.Replace("\n", "<br/>"),
    //                Adjuntos = new List<AdjuntoDto>
    //    {
    //        new AdjuntoDto
    //        {
    //            FileName = $"Presupuesto_{Id}.pdf",
    //            ContentType = "application/pdf",
    //            Base64 = Convert.ToBase64String(pdfBytes)
    //        }
    //    }
    //            };

    //            var result = await GoogleService.EnviarCorreoConAdjuntoAsync(UsuarioSeleccionadoId, dto);

    //            if (result?.Ok == true)
    //            {
    //                await PresupuestoService.MarcarEnviadoAsync(
    //                    Presupuesto.Id,
    //                    new MarcarEnviadoDto { Ok = true, MessageId = result.MessageId }
    //                );
    //                await Toast("📤 Enviado", "success");
    //            }
    //            else
    //            {
    //                await Toast("❌ Error al enviar", "error");
    //            }
    //        }

    //        else
    //        {
    //            // Enviar solo HTML del detalle (si preferís, usá BuildEmailHtml())
    //            var dto = new CrearEmailDto
    //            {
    //                Destinatario = Destinatario,
    //                Asunto = AsuntoCorreo,
    //                Cuerpo = BuildEmailHtml()
    //            };
    //            var msg = await GoogleService.EnviarCorreoAsync(UsuarioSeleccionadoId, dto);
    //            await Toast($"Enviado: {msg}", "success");
    //        }

    //        MostrarEnviar = false;
    //    }
    //    catch (Exception ex)
    //    {
    //        await Toast($"Error al enviar: {ex.Message}", "error");
    //    }
    //    finally
    //    {
    //        Enviando = false;
    //    }
    //}




    //protected async Task EnviarCorreo()
    //{
    //    if (UsuarioSeleccionadoId == 0) { await Toast("Seleccioná usuario remitente.", "warning"); return; }
    //    if (string.IsNullOrWhiteSpace(Destinatario)) { await Toast("Ingresá el destinatario.", "warning"); return; }

    //    if (!await GoogleService.ValidarYRefrescarTokenAsync(UsuarioSeleccionadoId))
    //    {
    //        await Toast("Debes vincular/refrescar tu cuenta de Google.", "error");
    //        return;
    //    }

    //    Enviando = true;
    //    try
    //    {
    //        // 🔹 si el usuario editó y no generó preview, igual volcamos el HTML
    //        await SyncEditorsToState();

    //        if (AdjuntarPdf)
    //        {
    //            byte[] pdfBytes;
    //            if (!string.IsNullOrEmpty(PreviewPdfBase64))
    //            {
    //                pdfBytes = Convert.FromBase64String(PreviewPdfBase64);
    //            }
    //            else
    //            {
    //                var b = await PresupuestoService.ObtenerPdfPreviewAsync(Id, IntroHtml, OutroHtml);
    //                if (b is null || b.Length == 0)
    //                {
    //                    await Toast("No se pudo generar el PDF a adjuntar.", "error");
    //                    return;
    //                }
    //                pdfBytes = b;
    //                PreviewPdfBase64 = Convert.ToBase64String(pdfBytes);
    //            }

    //            var dto = new CrearEmailConAdjuntoDto
    //            {
    //                Destinatario = Destinatario,
    //                Asunto = AsuntoCorreo,
    //                CuerpoHtml = CuerpoCorreo.Replace("\n", "<br/>"),
    //                Adjuntos = new List<AdjuntoDto>
    //            {
    //                new AdjuntoDto
    //                {
    //                    FileName = $"Presupuesto_{Id}.pdf",
    //                    ContentType = "application/pdf",
    //                    Base64 = Convert.ToBase64String(pdfBytes)
    //                }
    //            }
    //            };

    //            var result = await GoogleService.EnviarCorreoConAdjuntoAsync(UsuarioSeleccionadoId, dto);

    //            if (result?.Ok == true)
    //            {
    //                await PresupuestoService.MarcarEnviadoAsync(
    //                    Presupuesto.Id,
    //                    new MarcarEnviadoDto { Ok = true, MessageId = result.MessageId }
    //                );

    //                // 🔄 refrescar datos del presupuesto
    //                Presupuesto = await PresupuestoService.ObtenerPorIdAsync(Presupuesto.Id) ?? Presupuesto;

    //                await Toast("📤 Enviado", "success");
    //            }
    //            else
    //            {
    //                await Toast("❌ Error al enviar", "error");
    //            }
    //        }
    //        else
    //        {
    //            var dto = new CrearEmailDto
    //            {
    //                Destinatario = Destinatario,
    //                Asunto = AsuntoCorreo,
    //                Cuerpo = BuildEmailHtml()
    //            };
    //            var msg = await GoogleService.EnviarCorreoAsync(UsuarioSeleccionadoId, dto);
    //            await Toast($"Enviado: {msg}", "success");
    //        }

    //        // Limpiar editores y cerrar
    //        await JS.InvokeVoidAsync("destroyEditor", "editorIntro");
    //        await JS.InvokeVoidAsync("destroyEditor", "editorOutro");
    //        MostrarEnviar = false;
    //    }
    //    catch (Exception ex)
    //    {
    //        await Toast($"Error al enviar: {ex.Message}", "error");
    //    }
    //    finally
    //    {
    //        Enviando = false;
    //    }
    //}


    //protected async Task EnviarCorreo()
    //{
    //    if (UsuarioSeleccionadoId == 0)
    //    {
    //        await Toast("Seleccioná usuario remitente.", "warning");
    //        return;
    //    }

    //    if (string.IsNullOrWhiteSpace(Destinatario))
    //    {
    //        await Toast("Ingresá el destinatario.", "warning");
    //        return;
    //    }

    //    if (!await GoogleService.ValidarYRefrescarTokenAsync(UsuarioSeleccionadoId))
    //    {
    //        await Toast("Debes vincular/refrescar tu cuenta de Google.", "error");
    //        return;
    //    }

    //    Enviando = true;

    //    try
    //    {
    //        // Traer el contenido de los editores (intro / outro) al estado
    //        await SyncEditorsToState();

    //        // Armamos el cuerpo HTML final:
    //        // 1) Texto libre que escribiste en "Mensaje" (CuerpoCorreo)
    //        // 2) El presupuesto formateado (BuildEmailHtml)
    //        var sb = new StringBuilder();

    //        if (!string.IsNullOrWhiteSpace(CuerpoCorreo))
    //        {
    //            sb.Append(CuerpoCorreo.Replace("\n", "<br/>"));
    //            sb.Append("<br/><br/>");
    //        }

    //        sb.Append(BuildPresupuestoHtml());   // acá va el HTML del presupuesto

    //        var dto = new CrearEmailDto
    //        {
    //            Destinatario = Destinatario,
    //            Asunto = AsuntoCorreo,
    //            Cuerpo = sb.ToString()
    //        };

    //        var msg = await GoogleService.EnviarCorreoAsync(UsuarioSeleccionadoId, dto);
    //        await Toast($"📤 Enviado: {msg}", "success");

    //        // Limpiar editores y cerrar modal
    //        await JS.InvokeVoidAsync("destroyEditor", "editorIntro");
    //        await JS.InvokeVoidAsync("destroyEditor", "editorOutro");
    //        MostrarEnviar = false;
    //    }
    //    catch (Exception ex)
    //    {
    //        await Toast($"Error al enviar: {ex.Message}", "error");
    //    }
    //    finally
    //    {
    //        Enviando = false;
    //    }
    //}



    //    protected string BuildEmailHtml()
    //    {
    //        var ci = new CultureInfo("es-AR");
    //        var sb = new StringBuilder();
    //        sb.Append($@"
    //<h3>Presupuesto #{Presupuesto.Id}</h3>
    //<div><b>Fecha:</b> {Presupuesto.Fecha:dd/MM/yyyy}</div>
    //<div><b>Cliente:</b> {ClienteLinea1}</div>
    //<hr/>
    //<table style=""border-collapse:collapse;width:100%;"">
    //  <thead>
    //    <tr>
    //      <th style=""border:1px solid #ddd;padding:6px;width:80px;"">Cant.</th>
    //      <th style=""border:1px solid #ddd;padding:6px;"">Descripción</th>
    //      <th style=""border:1px solid #ddd;padding:6px;width:140px;text-align:right;"">P. Unitario</th>
    //      <th style=""border:1px solid #ddd;padding:6px;width:140px;text-align:right;"">Subtotal</th>
    //    </tr>
    //  </thead>
    //  <tbody>");
    //        foreach (var d in Detalles)
    //        {
    //            sb.Append($@"
    //    <tr>
    //      <td style=""border:1px solid #ddd;padding:6px;"">{d.Cantidad}</td>
    //      <td style=""border:1px solid #ddd;padding:6px;"">{d.Descripcion}</td>
    //      <td style=""border:1px solid #ddd;padding:6px;text-align:right;"">{d.PrecioUnitario.ToString("C", ci)}</td>
    //      <td style=""border:1px solid #ddd;padding:6px;text-align:right;"">{d.Subtotal.ToString("C", ci)}</td>
    //    </tr>");
    //        }
    //        sb.Append($@"
    //  </tbody>
    //</table>
    //<p style=""text-align:right;""><b>Neto:</b> {Presupuesto.Neto.ToString("C", ci)}</p>
    //<p style=""text-align:right;""><b>IVA:</b> {Presupuesto.Iva.ToString("C", ci)}</p>
    //<h4 style=""text-align:right;""><b>Total:</b> {Presupuesto.Total.ToString("C", ci)}</h4>
    //<p><i>{NotaAlPie}</i></p>");
    //        return sb.ToString();
    //    }








    protected async Task EnviarCorreo()
    {
        if (UsuarioSeleccionadoId == 0)
        {
            await Toast("Seleccioná usuario remitente.", "warning");
            return;
        }

        if (string.IsNullOrWhiteSpace(Destinatario))
        {
            await Toast("Ingresá el destinatario.", "warning");
            return;
        }

        if (!await GoogleService.ValidarYRefrescarTokenAsync(UsuarioSeleccionadoId))
        {
            await Toast("Debes vincular/refrescar tu cuenta de Google.", "error");
            return;
        }

        Enviando = true;

        try
        {
            // 1) Cuerpo del mail: saludo + versión simple del presupuesto
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(CuerpoCorreo))
            {
                sb.Append(CuerpoCorreo.Replace("\n", "<br/>"));
                sb.Append("<br/><br/>");
            }

            /*sb.Append(BuildPresupuestoHtml());*/ // tu versión "simple" que ya probaste

            sb.Append(BuildPresupuestoAdjuntoHtmlCompleto());

            // 2) HTML completo para adjuntar
            var htmlAdjunto = BuildPresupuestoAdjuntoHtmlCompleto();
            var htmlBytes = Encoding.UTF8.GetBytes(htmlAdjunto);

            var dto = new CrearEmailConAdjuntoDto
            {
                Destinatario = Destinatario,
                Asunto = AsuntoCorreo,
                CuerpoHtml = sb.ToString(),
                Adjuntos = new List<AdjuntoDto>
            {
                new AdjuntoDto
                {
                    FileName = $"Presupuesto_{Presupuesto.Id}.html",
                    ContentType = "text/html",
                    Base64 = Convert.ToBase64String(htmlBytes)
                }
            }
            };

            var result = await GoogleService.EnviarCorreoConAdjuntoAsync(UsuarioSeleccionadoId, dto);

            if (result?.Ok == true)
            {
                await PresupuestoService.MarcarEnviadoAsync(
                    Presupuesto.Id,
                    new MarcarEnviadoDto { Ok = true, MessageId = result.MessageId }
                );

                Presupuesto = await PresupuestoService.ObtenerPorIdAsync(Presupuesto.Id) ?? Presupuesto;

                await Toast("📤 Enviado", "success");
                MostrarEnviar = false;
            }
            else
            {
                await Toast("❌ Error al enviar", "error");
            }
        }
        catch (Exception ex)
        {
            await Toast($"Error al enviar: {ex.Message}", "error");
        }
        finally
        {
            Enviando = false;
        }
    }








    protected string BuildPresupuestoHtml()
    {
        var ci = new CultureInfo("es-AR");
        var sb = new StringBuilder();

        sb.Append($@"
<h3>Presupuesto #{Presupuesto.Id}</h3>
<div><b>Fecha:</b> {Presupuesto.Fecha:dd/MM/yyyy}</div>
<div><b>Cliente:</b> {ClienteLinea1}</div>
<hr/>

<table style=""border-collapse:collapse;width:100%;font-size:14px;"">
<thead>
<tr>
  <th style=""border:1px solid #ccc;padding:6px;width:70px;text-align:center"">Cant.</th>
  <th style=""border:1px solid #ccc;padding:6px"">Descripción</th>
  <th style=""border:1px solid #ccc;padding:6px;text-align:right;width:120px"">P. Unitario</th>
  <th style=""border:1px solid #ccc;padding:6px;text-align:right;width:120px"">Subtotal</th>
</tr>
</thead>
<tbody>");

        foreach (var d in Detalles)
        {
            sb.Append($@"
<tr>
  <td style=""border:1px solid #ccc;padding:6px;text-align:center"">{d.Cantidad:N2}</td>
  <td style=""border:1px solid #ccc;padding:6px"">{d.Descripcion}</td>
  <td style=""border:1px solid #ccc;padding:6px;text-align:right"">{d.PrecioUnitario.ToString("C", ci)}</td>
  <td style=""border:1px solid #ccc;padding:6px;text-align:right"">{d.Subtotal.ToString("C", ci)}</td>
</tr>");
        }

        sb.Append("</tbody></table>");

        sb.Append($@"
<p style=""text-align:right;font-size:15px""><b>Neto:</b> {Presupuesto.Neto.ToString("C", ci)}</p>
<p style=""text-align:right;font-size:15px""><b>IVA:</b> {Presupuesto.Iva.ToString("C", ci)}</p>
<h3 style=""text-align:right""><b>Total:</b> {Presupuesto.Total.ToString("C", ci)}</h3>
");

        if (!string.IsNullOrWhiteSpace(NotaAlPie))
            sb.Append($"<p><i>{NotaAlPie}</i></p>");

        return sb.ToString();
    }












    protected string BuildPresupuestoAdjuntoHtmlCompleto()
    {
        var ci = new CultureInfo("es-AR");
        var sb = new StringBuilder();

        sb.Append(@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <title>Presupuesto #" + Presupuesto.Id + @"</title>
    <style>
        body { font-family: Arial, Helvetica, sans-serif; background-color:#f8f9fa; margin:0; padding:20px; }
        .card { background:#fff; border:1px solid #ddd; border-radius:6px; padding:20px; max-width:900px; margin:0 auto; }
        .muted { color:#6c757d; font-size:0.9rem; }
        .header-row { display:flex; align-items:center; gap:20px; }
        .logo { max-height:80px; max-width:150px; object-fit:contain; }
        .cliente-estado { display:block; margin-top:20px;}
        .totales { text-align:right; margin-top:15px; }
        .totales div { margin-bottom:4px; }
        .equivalencia { margin-top:20px; padding:10px; border-radius:4px; background:#f1f3f5; font-size:0.95rem; }
        .firma { margin-top:30px; border-top:1px solid #dee2e6; padding-top:10px; font-size:0.95rem; }
        table.detalle { width:100%; border-collapse:collapse; margin-top:20px; font-size:0.95rem; }
        table.detalle th, table.detalle td {
            border:1px solid #dee2e6;
            padding:6px 8px;
        }
        table.detalle th { background:#f8f9fa; }
        table.detalle td.num { text-align:right; white-space:nowrap; }
        table.detalle td.cant { text-align:center; white-space:nowrap; }
        h2 { margin:0; }
        h3 { margin:0; }

    </style>
</head>
<body>
    <div class=""card"">");

        // Encabezado
        sb.Append(@"
        <div class=""header-row"">");

        if (!string.IsNullOrWhiteSpace(LogoUrl))
        {
            // Ojo: en adjunto local puede que la URL externa no cargue, pero no rompe nada
            sb.Append($@"<div><img src=""{LogoUrl}"" alt=""Logo"" class=""logo"" /></div>");
        }

        sb.Append(@"
            <div>
                <h2>Presupuesto</h2>
                <div class=""muted"">N.º " + Presupuesto.Id + " • " + Presupuesto.Fecha.ToString("dd/MM/yyyy") + @"</div>
                <div class=""muted"">" + NombreEstudio + @"</div>
            </div>
        </div>
        <hr />");

        // Cliente + estado
        sb.Append(@"
        <div class=""cliente-estado"">
            <div>
                <h3>Cliente</h3>
                <div>" + ClienteLinea1 + @"</div>");

        if (!string.IsNullOrWhiteSpace(Persona?.Email))
            sb.Append("<div><b>Email:</b> " + Persona!.Email + "</div>");
        if (!string.IsNullOrWhiteSpace(Persona?.Domicilio))
            sb.Append("<div><b>Domicilio:</b> " + Persona!.Domicilio + "</div>");

        sb.Append(@"</div>
            <div style=""text-align:left;"">
                <div><b>Estado:</b> " + Presupuesto.Estado + @"</div>
                <div><b>Total:</b> " + Presupuesto.Total.ToString("C", ci) + @"</div>
            </div>
        </div>");

        // Detalle
        sb.Append(@"
        <h3 style=""margin-top:25px;"">Detalle</h3>
        <table class=""detalle"">
            <thead>
                <tr>
                    <th style=""width:90px;"">Cant.</th>
                    <th>Descripción</th>
                    <th style=""width:140px; text-align:right;"">P. Unitario</th>
                    <th style=""width:140px; text-align:right;"">Subtotal</th>
                </tr>
            </thead>
            <tbody>");

        foreach (var d in Detalles)
        {
            sb.Append(@"
                <tr>
                    <td class=""cant"">" + d.Cantidad.ToString("N2") + @"</td>
                    <td>" + d.Descripcion + @"</td>
                    <td class=""num"">" + d.PrecioUnitario.ToString("C", ci) + @"</td>
                    <td class=""num"">" + d.Subtotal.ToString("C", ci) + @"</td>
                </tr>");
        }

        sb.Append(@"
            </tbody>
        </table>");

        // Totales
        sb.Append(@"
        <div class=""totales"">
            <div><b>Neto:</b> " + Presupuesto.Neto.ToString("C", ci) + @"</div>
            <div><b>IVA:</b> " + Presupuesto.Iva.ToString("C", ci) + @"</div>
            <div style=""font-size:1.1rem;""><b>Total:</b> " + Presupuesto.Total.ToString("C", ci) + @"</div>
        </div>");

        // Nota
        if (!string.IsNullOrWhiteSpace(NotaAlPie))
        {
            sb.Append(@"
        <div style=""margin-top:15px;font-style:italic;color:#555;"">" + NotaAlPie + @"</div>");
        }

        // Equivalencias (versión resumida, si querés)
        if (Equivalencias?.Any() == true)
        {
            sb.Append(@"<div class=""equivalencia"">");

            if (Equivalencias.Count == 1)
            {
                var e = Equivalencias[0];
                sb.Append($@"
                <div>
                    <b>Importante:</b> Este presupuesto equivale a <b>{e.Unidades:N2}</b> {e.Etiqueta} tomando como referencia el valor vigente 
                        a la fecha de su emisión {e.ValorUsado.ToString("C")}.  
                    <strong> El monto final a abonar quedará sujeto a actualización conforme al valor del {e.Etiqueta} vigente al momento del pago efectivo.</strong>
                </div>");
                  
            }
            else
            {
                sb.Append("<b>Equivalencias por variable:</b><ul>");
                foreach (var e in Equivalencias)
                {
                    sb.Append($@"<li><b>{e.Unidades:N2}</b> {e.Etiqueta}</li>");
                }
                sb.Append("</ul>");
            }

            sb.Append("<small class=\"muted\">Cálculo proporcional sobre el total del presupuesto.</small></div>");
        }

        // Firma
        sb.Append(@"
        <div class=""firma"">
            <div>" + NombreEstudio + @"</div>
            <div class=""muted"">" + PieEstudio + @"</div>
        </div>
    </div>
</body>
</html>");

        return sb.ToString();
    }







    protected async Task Toast(string msg, string tipo = "info")
        => await JS.InvokeVoidAsync("mostrarToast", msg, tipo);







    protected async Task PrevisualizarPdf()
    {
        _pdfBytesCache ??= await PresupuestoService.ObtenerPdf2Async(Presupuesto.Id);

        PdfPreviewUrl = await JS.InvokeAsync<string>("pdfPreview.createObjectUrl", _pdfBytesCache);
        MostrarPdfPreview = true;
    }



    protected async Task CerrarPdfPreview()
    {
        MostrarPdfPreview = false;

        if (!string.IsNullOrWhiteSpace(PdfPreviewUrl))
            await JS.InvokeVoidAsync("pdfPreview.revokeObjectUrl", PdfPreviewUrl);

        PdfPreviewUrl = null;
    }



    protected async Task ImprimirPdf()
    {
        // Si no abriste preview, lo abrimos primero para tener el iframe
        if (!MostrarPdfPreview)
            await PrevisualizarPdf();

        await JS.InvokeVoidAsync("pdfPreview.printFrame", "pdfPresupuestoFrame");
    }




    protected async Task DescargarPdf()
    {
        _pdfBytesCache ??= await PresupuestoService.ObtenerPdfAsync(Presupuesto.Id);

        await JS.InvokeVoidAsync("pdfPreview.downloadBytes",
            _pdfBytesCache,
            "application/pdf",
            $"presupuesto_{Presupuesto.Id}.pdf");
    }


    protected async Task RedactarMailLocal()
    {
        var to = Destinatario ?? "";
        var subject = Uri.EscapeDataString(AsuntoCorreo ?? $"Presupuesto #{Presupuesto.Id}");
        var body = Uri.EscapeDataString(CuerpoCorreo ?? "Adjunto presupuesto en PDF.");
        var mailto = $"mailto:{to}?subject={subject}&body={body}";

        await JS.InvokeVoidAsync("pdfPreview.openMailto", mailto);
    }





}


