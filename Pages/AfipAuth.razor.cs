using IurixBlazor.Services;
using IurixBlazor.Services.Interfaces;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

public class AfipAuthBase : ComponentBase
{
    [Inject] public UsuarioService UsuarioService { get; set; } = default!;
    [Inject] public AfipAuthService AfipAuthService { get; set; } = default!;
    [Inject] public CertService CertService { get; set; } = default!;
    [Inject] public IFileUploadService FileUploadService { get; set; } = default!;

    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected List<UsuarioDto> Usuarios { get; set; } = new();
    protected UsuarioDto? UsuarioSeleccionado { get; set; }
    protected AfipAuthDto AfipAuth { get; set; } = new();

    protected string EstadoCertificadoHomologacion = "No cargado";
    protected string EstadoRsaHomologacion = "No generada";
    protected string EstadoCertificadoProduccion = "No cargado";
    protected string EstadoRsaProduccion = "No generada";


    protected string CnHomologacion { get; set; } = string.Empty;
    protected string CnProduccion { get; set; } = string.Empty;



    // Helpers de estado (Homologación)
    protected bool TieneClaveHomol => !string.IsNullOrWhiteSpace(AfipAuth?.RSA_PKEY);
    protected bool TieneCertHomol => !string.IsNullOrWhiteSpace(AfipAuth?.CERT_X509);

    // Helpers de estado (Producción)
    protected bool TieneClaveProd => !string.IsNullOrWhiteSpace(AfipAuth?.RSA_PKEY_PROD);
    protected bool TieneCertProd => !string.IsNullOrWhiteSpace(AfipAuth?.CERT_X509_PROD);

    // Reglas UI (Homologación)
    protected bool PuedeGenerarSolicitudHomol => !TieneClaveHomol && !TieneCertHomol;    // genera clave+CSR
    protected bool PuedeCargarCertHomol => TieneClaveHomol && !TieneCertHomol;     // requiere clave
    protected bool PuedeRecargarClaveHomol => TieneClaveHomol && !TieneCertHomol;     // NO si ya hay cert
    protected bool PuedeEliminarHomol => TieneClaveHomol || TieneCertHomol;

    // Reglas UI (Producción)
    protected bool PuedeGenerarSolicitudProd => !TieneClaveProd && !TieneCertProd;
    protected bool PuedeCargarCertProd => TieneClaveProd && !TieneCertProd;
    protected bool PuedeRecargarClaveProd => TieneClaveProd && !TieneCertProd;
    protected bool PuedeEliminarProd => TieneClaveProd || TieneCertProd;



    protected override async Task OnInitializedAsync()
    {
        Usuarios = await UsuarioService.ObtenerUsuariosAsync();
    }

    //protected async Task OnUsuarioSeleccionado(ChangeEventArgs e)
    //{
    //    var usuarioId = int.Parse(e.Value?.ToString() ?? "0");
    //    UsuarioSeleccionado = Usuarios.FirstOrDefault(u => u.Id == usuarioId);

    //    if (UsuarioSeleccionado != null)
    //    {
    //        AfipAuth = await AfipAuthService.ObtenerPorUsuarioId(usuarioId) ?? new AfipAuthDto { UsuarioId = usuarioId };
    //        ActualizarEstados();
    //    }
    //}

    protected async Task OnUsuarioSeleccionado(ChangeEventArgs e)
    {
        //var usuarioId = int.Parse(e.Value?.ToString() ?? "0");
        var value = e.Value?.ToString();
        var usuarioId = int.TryParse(value, out var id) ? id : 0;

        if (usuarioId == 0)
        {
            UsuarioSeleccionado = null;
            AfipAuth = null!;
            return;
        }

        // 🔥 Traemos el usuario completo desde backend
        UsuarioSeleccionado = await UsuarioService.ObtenerUsuarioPorIdAsync(usuarioId);

        if (UsuarioSeleccionado != null)
        {
            AfipAuth = await AfipAuthService.ObtenerPorUsuarioId(usuarioId)
                       ?? new AfipAuthDto { UsuarioId = usuarioId };
            ActualizarEstados();
        }
    }



    private void ActualizarEstados()
    {
        EstadoCertificadoHomologacion = string.IsNullOrEmpty(AfipAuth.CERT_X509) ? "No cargado" : "Cargado";
        EstadoRsaHomologacion = string.IsNullOrEmpty(AfipAuth.RSA_PKEY) ? "No generada" : "Generada";
        EstadoCertificadoProduccion = string.IsNullOrEmpty(AfipAuth.CERT_X509_PROD) ? "No cargado" : "Cargado";
        EstadoRsaProduccion = string.IsNullOrEmpty(AfipAuth.RSA_PKEY_PROD) ? "No generada" : "Generada";
    }

    //protected async Task GenerarSolicitudHomologacion()
    //{
    //    var rsa = await CertService.GenerarClavePrivadaAsync();
    //    //var csr = await CertService.GenerarCSRAsync(rsa, UsuarioSeleccionado!.CUIT, UsuarioSeleccionado.Nombre);
    //    if (string.IsNullOrWhiteSpace(UsuarioSeleccionado?.CUIT))
    //    {
    //        throw new InvalidOperationException("El usuario seleccionado no tiene CUIT configurado.");
    //    }

    //    var csr = await CertService.GenerarCSRAsync(
    //        rsa,
    //        UsuarioSeleccionado.CUIT!,
    //        UsuarioSeleccionado.Nombre ?? string.Empty
    //    );
    //    AfipAuth.RSA_PKEY = rsa;
    //    await AfipAuthService.GuardarAsync(AfipAuth);
    //    EstadoRsaHomologacion = "Generada";
    //}


    //protected async Task GenerarSolicitudHomologacion()
    //{
    //    //System.Diagnostics.Debug.WriteLine($"{UsuarioSeleccionado.CUIT}");

    //    if (UsuarioSeleccionado == null)
    //    {
    //        // Mostramos un mensaje de advertencia en UI
    //        await JS.InvokeVoidAsync("alert", "Debe seleccionar un usuario antes de generar el certificado.");
    //        return;
    //    }

    //    if (string.IsNullOrWhiteSpace(UsuarioSeleccionado.CUIT))
    //    {

    //        await JS.InvokeVoidAsync("alert", "El usuario seleccionado no tiene CUIT configurado.");
    //        return;
    //    }

    //    if (string.IsNullOrWhiteSpace(UsuarioSeleccionado.RazonSocial))
    //    {

    //        await JS.InvokeVoidAsync("alert", "El usuario seleccionado no tiene RAZON SOCIAL configurada.");
    //        return;
    //    }

    //    if (string.IsNullOrWhiteSpace(CnHomologacion))
    //    {
    //        await JS.InvokeVoidAsync("alert", "Debe ingresar el nombre del sistema (CN) declarado en AFIP.");
    //        return;
    //    }

    //    // Generar clave privada y CSR
    //    var rsa = await CertService.GenerarClavePrivadaAsync();
    //    var csr = await CertService.GenerarCSRAsync(
    //        rsa,
    //        UsuarioSeleccionado.CUIT!,
    //        UsuarioSeleccionado.RazonSocial ?? string.Empty,
    //        CnHomologacion

    //    );

    //    AfipAuth.RSA_PKEY = rsa;
    //    await AfipAuthService.GuardarAsync(AfipAuth);
    //    EstadoRsaHomologacion = "Generada";
    //    //// (Opcional) Descargar CSR para subir a AFIP
    //    //await FileUploadService.DownloadFileAsync($"{UsuarioSeleccionado.Nombre}_AFIP_HOMOL.csr", csr);
    //    // Generar CSR en PEM estándar
    //    var csrPem = await CertService.GenerarCSRAsync(rsa, UsuarioSeleccionado.CUIT!, UsuarioSeleccionado.RazonSocial!, CnHomologacion);
    //    await FileUploadService.DownloadFileAsync($"{UsuarioSeleccionado.Nombre}_AFIP.csr", csrPem);

    //    // Generar CSR solo Base64 (sin cabeceras)
    //    var csrBase64 = await CertService.GenerarCSRBase64Async(rsa, UsuarioSeleccionado.CUIT!, UsuarioSeleccionado.RazonSocial!, CnHomologacion);
    //    await FileUploadService.DownloadFileAsync($"{UsuarioSeleccionado.Nombre}_AFIP_BASE64.txt", csrBase64);
    //}


    protected async Task GenerarSolicitudHomologacion()
    {
        if (!PuedeGenerarSolicitudHomol) { await JS.InvokeVoidAsync("alert", "No se puede generar la solicitud en el estado actual."); return; }

        if (UsuarioSeleccionado == null)
        {
            await JS.InvokeVoidAsync("alert", "Debe seleccionar un usuario antes de generar el certificado.");
            return;
        }

        if (string.IsNullOrWhiteSpace(UsuarioSeleccionado.CUIT))
        {
            await JS.InvokeVoidAsync("alert", "El usuario seleccionado no tiene CUIT configurado.");
            return;
        }

        if (string.IsNullOrWhiteSpace(UsuarioSeleccionado.RazonSocial))
        {
            await JS.InvokeVoidAsync("alert", "El usuario seleccionado no tiene RAZÓN SOCIAL configurada.");
            return;
        }

        if (string.IsNullOrWhiteSpace(CnHomologacion))
        {
            await JS.InvokeVoidAsync("alert", "Debe ingresar el nombre del sistema (CN) declarado en AFIP.");
            return;
        }

        // ✅ Generar clave privada y CSR en un único paso
        var rsa = await CertService.GenerarClavePrivadaAsync();

        // Guardar clave privada en AFIP Auth
        AfipAuth.RSA_PKEY = rsa;
        await AfipAuthService.GuardarAsync(AfipAuth);
        EstadoRsaHomologacion = "Generada";

        // ✅ Generar CSR PEM estándar
        var csrPem = await CertService.GenerarCSRAsync(
            rsa,
            UsuarioSeleccionado.CUIT!,
            UsuarioSeleccionado.RazonSocial!,
            CnHomologacion
        );
        await FileUploadService.DownloadFileAsync($"{UsuarioSeleccionado.Nombre}_AFIP.csr", csrPem);

        // ✅ Generar CSR solo Base64 (para copiar en AFIP sin cabeceras)
        //var csrBase64 = await CertService.GenerarCSRBase64Async(
        //    rsa,
        //    UsuarioSeleccionado.CUIT!,
        //    UsuarioSeleccionado.RazonSocial!,
        //    CnHomologacion
        //);
        //await FileUploadService.DownloadFileAsync($"{UsuarioSeleccionado.Nombre}_AFIP_BASE64.txt", csrBase64);
    }







// Extrae la primera X.509 de un PEM/base64 .crt/.cer/.pem/.txt
protected static X509Certificate2? ParseCertFromPem(string pem)
{
    if (string.IsNullOrWhiteSpace(pem)) return null;

    try
    {
        const string begin = "-----BEGIN CERTIFICATE-----";
        const string end = "-----END CERTIFICATE-----";

        string base64;
        if (pem.Contains(begin))
        {
            var i = pem.IndexOf(begin, StringComparison.Ordinal);
            var j = pem.IndexOf(end, i, StringComparison.Ordinal);
            if (i < 0 || j <= i) return null;
            var inner = pem.Substring(i + begin.Length, j - (i + begin.Length));
            base64 = new string(inner.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }
        else
        {
            base64 = new string(pem.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        var raw = Convert.FromBase64String(base64);
        return new X509Certificate2(raw);
    }
    catch { return null; }
}

protected static string? GetCertCN(X509Certificate2 cert)
    => cert.GetNameInfo(X509NameType.SimpleName, false);

// (Opcional) chequea que cert publique la misma clave que la privada PEM guardada
protected static bool CertMatchesPrivateKey(string pemPrivateKey, X509Certificate2 cert)
{
    try
    {
        using var rsaPriv = RSA.Create();
        rsaPriv.ImportFromPem(pemPrivateKey);

        var priv = rsaPriv.ExportParameters(false);
        using var rsaPub = cert.GetRSAPublicKey();
        if (rsaPub is null) return false;
        var pub = rsaPub.ExportParameters(false);

        // Comparo solo el Módulo (N). Si coincide, es el mismo par.
        return priv.Modulus.AsSpan().SequenceEqual(pub.Modulus);
    }
    catch { return false; }
}



    //protected async Task CargarCertificadoHomologacion()
    //    {
    //        if (!PuedeCargarCertHomol) { await JS.InvokeVoidAsync("alert", "Primero generá la clave y la solicitud (CSR)."); return; }

    //        var file = await FileUploadService.PickFileAsync();
    //        AfipAuth.CERT_X509 = file.Content;
    //        await AfipAuthService.GuardarAsync(AfipAuth);
    //        EstadoCertificadoHomologacion = "Cargado";
    //    }
    protected async Task CargarCertificadoHomologacion()
    {
        if (!PuedeCargarCertHomol)
        {
            await JS.InvokeVoidAsync("alert", "Primero generá la clave y la solicitud (CSR).");
            return;
        }

        var (fileName, content) = await FileUploadService.PickFileAsync(".crt,.cer,.pem,.txt");
        if (string.IsNullOrWhiteSpace(content))
        {
            await JS.InvokeVoidAsync("alert", "Archivo inválido o vacío.");
            return;
        }

        // 1) Parsear certificado
        var cert = ParseCertFromPem(content);
        if (cert is null)
        {
            await JS.InvokeVoidAsync("alert", "No se pudo leer el certificado X.509.");
            return;
        }

        // 2) (Opcional) Validar que el cert matchee la clave privada cargada
        if (!string.IsNullOrWhiteSpace(AfipAuth.RSA_PKEY) &&
            !CertMatchesPrivateKey(AfipAuth.RSA_PKEY, cert))
        {
            await JS.InvokeVoidAsync("alert", "El certificado no corresponde a la clave privada actual. Revisá la clave/CSR.");
            return;
        }

        // 3) CN: autocompletar o advertir si difiere
        var cnFromCert = GetCertCN(cert);
        if (string.IsNullOrWhiteSpace(CnHomologacion) && !string.IsNullOrWhiteSpace(cnFromCert))
        {
            CnHomologacion = cnFromCert; // autocompleta CN
        }
        else if (!string.IsNullOrWhiteSpace(CnHomologacion) && !string.IsNullOrWhiteSpace(cnFromCert))
        {
            var equal = string.Equals(CnHomologacion.Trim(), cnFromCert.Trim(), StringComparison.OrdinalIgnoreCase);
            if (!equal)
            {
                var msg = $"El CN del certificado es \"{cnFromCert}\", diferente a \"{CnHomologacion}\". ¿Deseás continuar?";
                var ok = await JS.InvokeAsync<bool>("confirm", msg);
                if (!ok) return;
            }
        }

        // 4) Guardar (el backend calculará emisión/vencimiento si ya implementaste el parseo)
        AfipAuth.CERT_X509 = content;
        AfipAuth = await AfipAuthService.GuardarAsync(AfipAuth);

        EstadoCertificadoHomologacion = "Cargado";

        var desde = AfipAuth.CertValidFrom?.ToString("dd/MM/yyyy") ?? "-";
        var hasta = AfipAuth.CertValidTo?.ToString("dd/MM/yyyy") ?? "-";
        await JS.InvokeVoidAsync("alert", $"Certificado de Homologación cargado.\nVigencia: {desde} – {hasta}");
        StateHasChanged();
    }




    //protected async Task RecargarCertificadoHomologacion()
    //{
    //    if (!TieneClaveHomol)
    //    {
    //        await JS.InvokeVoidAsync("alert", "Primero generá o cargá la clave privada de Homologación.");
    //        return;
    //    }

    //    try
    //    {
    //        var (fileName, content) = await FileUploadService.PickFileAsync(".crt,.cer,.pem,.txt");
    //        if (string.IsNullOrWhiteSpace(content))
    //        {
    //            await JS.InvokeVoidAsync("alert", "Archivo inválido o vacío.");
    //            return;
    //        }

    //        AfipAuth.CERT_X509 = content;

    //        AfipAuth = await AfipAuthService.GuardarAsync(AfipAuth);

    //        // Fallback si GuardarAsync no retorna DTO:
    //        // await AfipAuthService.GuardarAsync(AfipAuth);
    //        // AfipAuth = await AfipAuthService.ObtenerPorUsuarioId(UsuarioSeleccionado!.Id) ?? AfipAuth;

    //        EstadoCertificadoHomologacion = "Cargado";

    //        var desde = AfipAuth.CertValidFrom?.ToString("dd/MM/yyyy") ?? "-";
    //        var hasta = AfipAuth.CertValidTo?.ToString("dd/MM/yyyy") ?? "-";
    //        await JS.InvokeVoidAsync("alert", $"Certificado de Homologación actualizado.\nVigencia: {desde} – {hasta}");
    //        StateHasChanged();
    //    }
    //    catch (OperationCanceledException) { }
    //}

    protected async Task RecargarCertificadoHomologacion()
    {
        if (!TieneClaveHomol)
        {
            await JS.InvokeVoidAsync("alert", "Primero generá o cargá la clave privada de Homologación.");
            return;
        }

        try
        {
            var (fileName, content) = await FileUploadService.PickFileAsync(".crt,.cer,.pem,.txt");
            if (string.IsNullOrWhiteSpace(content))
            {
                await JS.InvokeVoidAsync("alert", "Archivo inválido o vacío.");
                return;
            }

            // 1) Parsear el X.509
            var cert = ParseCertFromPem(content);
            if (cert is null)
            {
                await JS.InvokeVoidAsync("alert", "No se pudo leer el certificado X.509.");
                return;
            }

            // 2) Verificar que el cert corresponda a la clave privada (opcional, recomendado)
            if (!string.IsNullOrWhiteSpace(AfipAuth.RSA_PKEY) && !CertMatchesPrivateKey(AfipAuth.RSA_PKEY, cert))
            {
                await JS.InvokeVoidAsync("alert", "El certificado no corresponde a la clave privada actual. Revisá la clave/CSR.");
                return;
            }

            // 3) CN: autocompletar si está vacío o advertir si difiere
            var cnFromCert = cert.GetNameInfo(X509NameType.SimpleName, false);
            if (string.IsNullOrWhiteSpace(CnHomologacion) && !string.IsNullOrWhiteSpace(cnFromCert))
            {
                CnHomologacion = cnFromCert; // autocompletar
            }
            else if (!string.IsNullOrWhiteSpace(CnHomologacion) && !string.IsNullOrWhiteSpace(cnFromCert))
            {
                var equal = string.Equals(CnHomologacion.Trim(), cnFromCert.Trim(), StringComparison.OrdinalIgnoreCase);
                if (!equal)
                {
                    var ok = await JS.InvokeAsync<bool>("confirm",
                        $"El CN del certificado es \"{cnFromCert}\", distinto del actual \"{CnHomologacion}\". ¿Deseás continuar?");
                    if (!ok) return;
                }
            }

            // 4) Guardar y refrescar (el backend debería setear CertValidFrom/To)
            AfipAuth.CERT_X509 = content;
            AfipAuth = await AfipAuthService.GuardarAsync(AfipAuth);

            EstadoCertificadoHomologacion = "Cargado";

            var desde = AfipAuth.CertValidFrom?.ToString("dd/MM/yyyy") ?? "-";
            var hasta = AfipAuth.CertValidTo?.ToString("dd/MM/yyyy") ?? "-";
            await JS.InvokeVoidAsync("alert", $"Certificado de Homologación actualizado.\nVigencia: {desde} – {hasta}");
            StateHasChanged();
        }
        catch (OperationCanceledException)
        {
            // usuario canceló el diálogo
        }

        // --------- helpers locales ---------
        static X509Certificate2? ParseCertFromPem(string pem)
        {
            try
            {
                const string begin = "-----BEGIN CERTIFICATE-----";
                const string end = "-----END CERTIFICATE-----";

                string base64;
                if (pem.Contains(begin))
                {
                    var i = pem.IndexOf(begin, StringComparison.Ordinal);
                    var j = pem.IndexOf(end, i, StringComparison.Ordinal);
                    if (i < 0 || j <= i) return null;
                    var inner = pem.Substring(i + begin.Length, j - (i + begin.Length));
                    base64 = new string(inner.Where(c => !char.IsWhiteSpace(c)).ToArray());
                }
                else
                {
                    base64 = new string(pem.Where(c => !char.IsWhiteSpace(c)).ToArray());
                }

                var raw = Convert.FromBase64String(base64);
                return new X509Certificate2(raw);
            }
            catch { return null; }
        }

        static bool CertMatchesPrivateKey(string pemPrivateKey, X509Certificate2 cert)
        {
            try
            {
                using var rsaPriv = RSA.Create();
                rsaPriv.ImportFromPem(pemPrivateKey);

                var priv = rsaPriv.ExportParameters(false);               // N, e
                using var rsaPub = cert.GetRSAPublicKey();
                if (rsaPub is null) return false;
                var pub = rsaPub.ExportParameters(false);

                // comparar Módulo y Exponente
                return priv.Modulus.AsSpan().SequenceEqual(pub.Modulus)
                    && priv.Exponent.AsSpan().SequenceEqual(pub.Exponent);
            }
            catch { return false; }
        }
    }





    protected async Task RecargarClavePrivadaHomologacion()
    {
        if (!PuedeRecargarClaveHomol) { await JS.InvokeVoidAsync("alert", "No se puede recargar la clave si ya hay certificado. Eliminá primero."); return; }

        var file = await FileUploadService.PickFileAsync(".key");
        AfipAuth.RSA_PKEY = file.Content;
        await AfipAuthService.GuardarAsync(AfipAuth);
        EstadoRsaHomologacion = "Generada";
    }

    protected async Task EliminarCertificadosHomologacion()
    {
        if (!PuedeEliminarHomol) { return; }

        AfipAuth.RSA_PKEY = string.Empty;
        AfipAuth.CERT_X509 = string.Empty;
        AfipAuth.TA = string.Empty;
        AfipAuth.Sign = string.Empty;
        AfipAuth.TokenExpiration = null;
        await AfipAuthService.GuardarAsync(AfipAuth);
        ActualizarEstados();
    }

    // Métodos de Producción similares...


    //protected async Task GenerarSolicitudProduccion()
    //{
    //    var rsa = await CertService.GenerarClavePrivadaAsync();
    //    //var csr = await CertService.GenerarCSRAsync(rsa, UsuarioSeleccionado!.CUIT, UsuarioSeleccionado.Nombre);
    //    if (string.IsNullOrWhiteSpace(UsuarioSeleccionado?.CUIT))
    //    {
    //        throw new InvalidOperationException("El usuario seleccionado no tiene CUIT configurado.");
    //    }

    //    var csr = await CertService.GenerarCSRAsync(
    //        rsa,
    //        UsuarioSeleccionado.CUIT!,
    //        UsuarioSeleccionado.Nombre ?? string.Empty
    //    );
    //    AfipAuth.RSA_PKEY_PROD = rsa;
    //    await AfipAuthService.GuardarAsync(AfipAuth);
    //    EstadoRsaProduccion = "Generada";

    //    // (Opcional) Descargar CSR para subir a AFIP
    //    await FileUploadService.DownloadFileAsync($"{UsuarioSeleccionado.Nombre}_AFIP_PROD.csr", csr);
    //}

    protected async Task GenerarSolicitudProduccion()
    {
        if (!PuedeGenerarSolicitudProd) { await JS.InvokeVoidAsync("alert", "No se puede generar la solicitud en el estado actual."); return; }

        if (UsuarioSeleccionado == null)
        {
            // Mostramos un mensaje de advertencia en UI
            await JS.InvokeVoidAsync("alert", "Debe seleccionar un usuario antes de generar el certificado.");
            return;
        }

        if (string.IsNullOrWhiteSpace(UsuarioSeleccionado.CUIT))
        {
            await JS.InvokeVoidAsync("alert", "El usuario seleccionado no tiene CUIT configurado.");
            return;
        }

        if (string.IsNullOrWhiteSpace(UsuarioSeleccionado.RazonSocial))
        {

            await JS.InvokeVoidAsync("alert", "El usuario seleccionado no tiene RAZON SOCIAL configurada.");
            return;
        }
        if (string.IsNullOrWhiteSpace(CnProduccion))
        {
            await JS.InvokeVoidAsync("alert", "Debe ingresar el nombre del sistema (CN) declarado en AFIP.");
            return;
        }

        // Generar clave privada y CSR
        var rsa = await CertService.GenerarClavePrivadaAsync();
        var csr = await CertService.GenerarCSRAsync(
           rsa,
           UsuarioSeleccionado.CUIT!,
           UsuarioSeleccionado.RazonSocial ?? string.Empty,
           CnProduccion
       );
        AfipAuth.RSA_PKEY_PROD = rsa;
        await AfipAuthService.GuardarAsync(AfipAuth);
        EstadoRsaProduccion = "Generada";

        // (Opcional) Descargar CSR para subir a AFIP
        //await FileUploadService.DownloadFileAsync($"{UsuarioSeleccionado.Nombre}_AFIP_PROD.csr", csr);

        // Generar CSR en PEM estándar
        var csrPem = await CertService.GenerarCSRAsync(rsa, UsuarioSeleccionado.CUIT!, UsuarioSeleccionado.RazonSocial!, CnProduccion);
        await FileUploadService.DownloadFileAsync($"{UsuarioSeleccionado.Nombre}_AFIP.csr", csrPem);

        //// Generar CSR solo Base64 (sin cabeceras)
        //var csrBase64 = await CertService.GenerarCSRBase64Async(rsa, UsuarioSeleccionado.CUIT!, UsuarioSeleccionado.RazonSocial!, CnProduccion);
        //await FileUploadService.DownloadFileAsync($"{UsuarioSeleccionado.Nombre}_AFIP_BASE64.txt", csrBase64);

    }





    //protected async Task CargarCertificadoProduccion()
    //{
    //    if (!PuedeCargarCertProd) { await JS.InvokeVoidAsync("alert", "Primero generá la clave y la solicitud (CSR)."); return; }

    //    var file = await FileUploadService.PickFileAsync(".crt");
    //    AfipAuth.CERT_X509_PROD = file.Content;
    //    await AfipAuthService.GuardarAsync(AfipAuth);
    //    EstadoCertificadoProduccion = "Cargado";
    //}

    //protected async Task CargarCertificadoProduccion()
    //{
    //    if (!PuedeCargarCertProd)
    //    {
    //        await JS.InvokeVoidAsync("alert", "Primero generá la clave y la solicitud (CSR).");
    //        return;
    //    }

    //    try
    //    {
    //        var (fileName, content) = await FileUploadService.PickFileAsync(".crt,.cer,.pem,.txt");
    //        if (string.IsNullOrWhiteSpace(content))
    //        {
    //            await JS.InvokeVoidAsync("alert", "Archivo inválido o vacío.");
    //            return;
    //        }

    //        AfipAuth.CERT_X509_PROD = content;

    //        // GuardarAsync debe devolver el DTO actualizado (con fechas si las calculás en backend)
    //        AfipAuth = await AfipAuthService.GuardarAsync(AfipAuth);

    //        EstadoCertificadoProduccion = "Cargado";
    //        StateHasChanged();
    //    }
    //    catch (OperationCanceledException)
    //    {
    //        // usuario canceló el diálogo -> no hacemos nada
    //    }
    //}


    protected async Task CargarCertificadoProduccion()
    {
        if (!PuedeCargarCertProd)
        {
            await JS.InvokeVoidAsync("alert", "Primero generá la clave y la solicitud (CSR).");
            return;
        }

        try
        {
            var (fileName, content) = await FileUploadService.PickFileAsync(".crt,.cer,.pem,.txt");
            if (string.IsNullOrWhiteSpace(content))
            {
                await JS.InvokeVoidAsync("alert", "Archivo inválido o vacío.");
                return;
            }

            // 1) Parsear X.509
            var cert = ParseCertFromPem(content);
            if (cert is null)
            {
                await JS.InvokeVoidAsync("alert", "No se pudo leer el certificado X.509.");
                return;
            }

            // 2) Validar que matchee la clave privada de PRODUCCIÓN
            if (!string.IsNullOrWhiteSpace(AfipAuth.RSA_PKEY_PROD) &&
                !CertMatchesPrivateKey(AfipAuth.RSA_PKEY_PROD, cert))
            {
                await JS.InvokeVoidAsync("alert", "El certificado no corresponde a la clave privada de Producción.");
                return;
            }

            // 3) CN: autocompletar o advertir si difiere
            var cnFromCert = cert.GetNameInfo(X509NameType.SimpleName, false);
            if (string.IsNullOrWhiteSpace(CnProduccion) && !string.IsNullOrWhiteSpace(cnFromCert))
            {
                CnProduccion = cnFromCert;
            }
            else if (!string.IsNullOrWhiteSpace(CnProduccion) && !string.IsNullOrWhiteSpace(cnFromCert))
            {
                var equal = string.Equals(CnProduccion.Trim(), cnFromCert.Trim(), StringComparison.OrdinalIgnoreCase);
                if (!equal)
                {
                    var ok = await JS.InvokeAsync<bool>("confirm",
                        $"El CN del certificado es \"{cnFromCert}\", distinto de \"{CnProduccion}\". ¿Deseás continuar?");
                    if (!ok) return;
                }
            }

            // 4) Guardar y refrescar (backend setea CertValidFrom_PROD/CertValidTo_PROD)
            AfipAuth.CERT_X509_PROD = content;
            AfipAuth = await AfipAuthService.GuardarAsync(AfipAuth);

            EstadoCertificadoProduccion = "Cargado";

            var desde = AfipAuth.CertValidFrom_PROD?.ToString("dd/MM/yyyy") ?? "-";
            var hasta = AfipAuth.CertValidTo_PROD?.ToString("dd/MM/yyyy") ?? "-";
            await JS.InvokeVoidAsync("alert", $"Certificado de Producción cargado.\nVigencia: {desde} – {hasta}");
            StateHasChanged();
        }
        catch (OperationCanceledException) { /* usuario canceló */ }
    }



    //protected async Task RecargarCertificadoProduccion()
    //{
    //    // Reglas mínimas: debe existir clave privada para este entorno
    //    if (!TieneClaveProd)
    //    {
    //        await JS.InvokeVoidAsync("alert", "Primero generá o cargá la clave privada de Producción.");
    //        return;
    //    }

    //    try
    //    {
    //        var (fileName, content) = await FileUploadService.PickFileAsync(".crt,.cer,.pem,.txt");
    //        if (string.IsNullOrWhiteSpace(content))
    //        {
    //            await JS.InvokeVoidAsync("alert", "Archivo inválido o vacío.");
    //            return;
    //        }

    //        // Reemplaza el certificado (recarga)
    //        AfipAuth.CERT_X509_PROD = content;

    //        // Guardar y recibir DTO ya actualizado con fechas (si tu servicio retorna el DTO)
    //        AfipAuth = await AfipAuthService.GuardarAsync(AfipAuth);

    //        // Si tu GuardarAsync aún NO retorna DTO, usá este fallback:
    //        // await AfipAuthService.GuardarAsync(AfipAuth);
    //        // AfipAuth = await AfipAuthService.ObtenerPorUsuarioId(UsuarioSeleccionado!.Id) ?? AfipAuth;

    //        EstadoCertificadoProduccion = "Cargado";

    //        var desde = AfipAuth.CertValidFrom_PROD?.ToString("dd/MM/yyyy") ?? "-";
    //        var hasta = AfipAuth.CertValidTo_PROD?.ToString("dd/MM/yyyy") ?? "-";
    //        await JS.InvokeVoidAsync("alert", $"Certificado de Producción actualizado.\nVigencia: {desde} – {hasta}");
    //        StateHasChanged();
    //    }
    //    catch (OperationCanceledException)
    //    {
    //        // El usuario canceló el diálogo -> no hacemos nada
    //    }
    //}


    protected async Task RecargarCertificadoProduccion()
    {
        if (!TieneClaveProd)
        {
            await JS.InvokeVoidAsync("alert", "Primero generá o cargá la clave privada de Producción.");
            return;
        }

        // Misma lógica que cargar (podés llamar directamente al método anterior)
        //    await CargarCertificadoProduccion();
        //}

        try
        {
            var (fileName, content) = await FileUploadService.PickFileAsync(".crt,.cer,.pem,.txt");
            if (string.IsNullOrWhiteSpace(content))
            {
                await JS.InvokeVoidAsync("alert", "Archivo inválido o vacío.");
                return;
            }

            // 1) Parsear X.509
            var cert = ParseCertFromPem(content);
            if (cert is null)
            {
                await JS.InvokeVoidAsync("alert", "No se pudo leer el certificado X.509.");
                return;
            }

            // 2) Validar que matchee la clave privada de PRODUCCIÓN
            if (!string.IsNullOrWhiteSpace(AfipAuth.RSA_PKEY_PROD) &&
                !CertMatchesPrivateKey(AfipAuth.RSA_PKEY_PROD, cert))
            {
                await JS.InvokeVoidAsync("alert", "El certificado no corresponde a la clave privada de Producción.");
                return;
            }

            // 3) CN: autocompletar o advertir si difiere
            var cnFromCert = cert.GetNameInfo(X509NameType.SimpleName, false);
            if (string.IsNullOrWhiteSpace(CnProduccion) && !string.IsNullOrWhiteSpace(cnFromCert))
            {
                CnProduccion = cnFromCert;
            }
            else if (!string.IsNullOrWhiteSpace(CnProduccion) && !string.IsNullOrWhiteSpace(cnFromCert))
            {
                var equal = string.Equals(CnProduccion.Trim(), cnFromCert.Trim(), StringComparison.OrdinalIgnoreCase);
                if (!equal)
                {
                    var ok = await JS.InvokeAsync<bool>("confirm",
                        $"El CN del certificado es \"{cnFromCert}\", distinto de \"{CnProduccion}\". ¿Deseás continuar?");
                    if (!ok) return;
                }
            }

            // 4) Guardar y refrescar (backend setea CertValidFrom_PROD/CertValidTo_PROD)
            AfipAuth.CERT_X509_PROD = content;
            AfipAuth = await AfipAuthService.GuardarAsync(AfipAuth);

            EstadoCertificadoProduccion = "Cargado";

            var desde = AfipAuth.CertValidFrom_PROD?.ToString("dd/MM/yyyy") ?? "-";
            var hasta = AfipAuth.CertValidTo_PROD?.ToString("dd/MM/yyyy") ?? "-";
            await JS.InvokeVoidAsync("alert", $"Certificado de Producción cargado.\nVigencia: {desde} – {hasta}");
            StateHasChanged();
        }
        catch (OperationCanceledException) { /* usuario canceló */ }
    }


    protected async Task RecargarClavePrivadaProduccion()
    {
        if (!PuedeRecargarClaveProd) { await JS.InvokeVoidAsync("alert", "No se puede recargar la clave si ya hay certificado. Eliminá primero."); return; }

        var file = await FileUploadService.PickFileAsync(".key");
        AfipAuth.RSA_PKEY_PROD = file.Content;
        await AfipAuthService.GuardarAsync(AfipAuth);
        EstadoRsaProduccion = "Generada";
    }

    protected async Task EliminarCertificadosProduccion()
    {
        if (!PuedeEliminarProd) { return; }

        AfipAuth.RSA_PKEY_PROD = string.Empty;
        AfipAuth.CERT_X509_PROD = string.Empty;
        AfipAuth.TA_PROD = string.Empty;
        AfipAuth.Sign_PROD = string.Empty;
        AfipAuth.TokenExpiration_PROD = null;
        await AfipAuthService.GuardarAsync(AfipAuth);
        ActualizarEstados();
    }




}
