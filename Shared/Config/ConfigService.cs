using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IurixBlazor.Shared.Config
{
    //public class ConfigService
    //{
    //    public string IpServidor { get; set; } = "192.168.1.100";
    //    public int Puerto { get; set; } = 5000;

    //    public string IpLicenciaBackend { get; set; } = "192.168.1.100";
    //    public int PuertoLicenciaBackend { get; set; } = 7140;
    //    public bool UsaHttpsLicenciaBackend { get; set; } = false;
    //    public bool UsaHttpsServidorBackend { get; set; } = false;

    //    public string LicenciaBackendUrl =>
    //        $"{(UsaHttpsLicenciaBackend ? "https" : "http")}://{IpLicenciaBackend}:{PuertoLicenciaBackend}";

    //    public string ServidorBackendUrl =>
    //        $"{(UsaHttpsServidorBackend ? "https" : "http")}://{IpServidor}:{Puerto}";

    //    public bool EstaCargado { get; private set; } = false;
    //    public string ModoCliente { get; set; } = "local";
    //    public bool EsClienteRemoto => ModoCliente.Equals("remoto", StringComparison.OrdinalIgnoreCase);

    //    private string ObtenerRutaConfig()
    //    {
    //        var configDir = Path.Combine(AppContext.BaseDirectory, "Config");
    //        Directory.CreateDirectory(configDir);
    //        return Path.Combine(configDir, "config.ini");
    //    }

    //    public async Task InicializarAsync()
    //    {
    //        await Task.Run(() => LeerConfiguracion());
    //        EstaCargado = true;
    //    }

    //    public void LeerConfiguracion()
    //    {
    //        var ruta = ObtenerRutaConfig();
    //        if (!File.Exists(ruta))
    //            return;

    //        var secciones = new Dictionary<string, Dictionary<string, string>>();
    //        string? seccionActual = null;

    //        foreach (var linea in File.ReadAllLines(ruta))
    //        {
    //            var trimmed = linea.Trim();
    //            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith(";")) continue;

    //            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
    //            {
    //                seccionActual = trimmed[1..^1];
    //                secciones[seccionActual] = new Dictionary<string, string>();
    //            }
    //            else if (seccionActual != null && trimmed.Contains('='))
    //            {
    //                var partes = trimmed.Split('=', 2);
    //                var clave = partes[0].Trim().ToLower();
    //                var valor = partes[1].Trim();
    //                secciones[seccionActual][clave] = valor;
    //            }
    //        }

    //        if (secciones.TryGetValue("Servidor", out var servidor))
    //        {
    //            if (servidor.TryGetValue("ip_servidor", out var ip))
    //                IpServidor = ip;
    //            if (servidor.TryGetValue("puerto", out var puertoStr) && int.TryParse(puertoStr, out var puerto))
    //                Puerto = puerto;
    //        }

    //        if (secciones.TryGetValue("IpLicenciaBackend", out var backend))
    //        {
    //            if (backend.TryGetValue("ip", out var ip))
    //                IpLicenciaBackend = ip;
    //            if (backend.TryGetValue("puerto", out var puertoStr) && int.TryParse(puertoStr, out var puerto))
    //                PuertoLicenciaBackend = puerto;
    //            if (backend.TryGetValue("https", out var httpsStr))
    //                UsaHttpsLicenciaBackend = httpsStr.Equals("true", StringComparison.OrdinalIgnoreCase);
    //        }

    //        if (secciones.TryGetValue("Cliente", out var cliente))
    //        {
    //            if (cliente.TryGetValue("modo", out var modo))
    //                ModoCliente = modo;
    //        }
    //    }

    //    public void EstablecerServidorRemoto(string ip, int puerto, bool usarHttps)
    //    {
    //        var ruta = ObtenerRutaConfig();
    //        if (!File.Exists(ruta)) return;

    //        var lineas = File.ReadAllLines(ruta).ToList();
    //        var nuevaConfig = new List<string>();
    //        bool enSeccionServidor = false;
    //        bool ipSet = false, puertoSet = false, httpsSet = false;

    //        foreach (var linea in lineas)
    //        {
    //            var trimmed = linea.Trim();
    //            if (trimmed.StartsWith("[")) enSeccionServidor = trimmed.Equals("[Servidor]", StringComparison.OrdinalIgnoreCase);

    //            if (enSeccionServidor && trimmed.StartsWith("ip_servidor=", StringComparison.OrdinalIgnoreCase))
    //            {
    //                nuevaConfig.Add($"ip_servidor = {ip}");
    //                ipSet = true;
    //            }
    //            else if (enSeccionServidor && trimmed.StartsWith("puerto=", StringComparison.OrdinalIgnoreCase))
    //            {
    //                nuevaConfig.Add($"puerto = {puerto}");
    //                puertoSet = true;
    //            }
    //            else if (enSeccionServidor && trimmed.StartsWith("https=", StringComparison.OrdinalIgnoreCase))
    //            {
    //                nuevaConfig.Add($"https = {usarHttps.ToString().ToLower()}");
    //                httpsSet = true;
    //            }
    //            else
    //            {
    //                nuevaConfig.Add(linea);
    //            }
    //        }

    //        if (enSeccionServidor)
    //        {
    //            if (!ipSet) nuevaConfig.Add($"ip_servidor = {ip}");
    //            if (!puertoSet) nuevaConfig.Add($"puerto = {puerto}");
    //            if (!httpsSet) nuevaConfig.Add($"https = {usarHttps.ToString().ToLower()}");
    //        }

    //        File.WriteAllLines(ruta, nuevaConfig);
    //        LeerConfiguracion();
    //    }

    //    public void EstablecerServidorLocal(string ip, int puerto, bool usarHttps)
    //    {
    //        EstablecerServidorRemoto(ip, puerto, usarHttps);
    //    }
    //}



    public enum AuthMode { Off, Optional, Required }




    public class ConfigService
    {
        public IJSRuntime? JS { get; set; }

        public AuthMode AuthMode { get; private set; } = AuthMode.Optional;
        public string IpServidor { get; set; } = "192.168.1.254";
        public int Puerto { get; set; } = 5000;
        public bool UsaHttpsServidorBackend { get; set; } = false;

        public string IpLicenciaBackend { get; set; } = "192.168.1.254";
        public int PuertoLicenciaBackend { get; set; } = 7140;
        public bool UsaHttpsLicenciaBackend { get; set; } = false;

        public string ModoCliente { get; set; } = "local";

        // "remoto" o "nube" => EsClienteRemoto = true
        public bool EsClienteRemoto =>
            // si lo forzás por INI
            !ModoCliente.Equals("local", StringComparison.OrdinalIgnoreCase)
            // o si detectamos que corre en Azure (fallback)
            || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"))
            || string.Equals(Environment.GetEnvironmentVariable("IURIX_MODE"), "Azure", StringComparison.OrdinalIgnoreCase);

        public bool EstaCargado { get; private set; }




        private string? _urlServidorBackend;
        private string? _urlLicenciaBackend;



        //El host donde consultamos DEXTRA y verificamos credenciales
        public string DextraDefaultHost { get; } = "https://consultadextra.jusneuquen.gov.ar/";




        // ===== URLs calculadas (omiten 80/443) =====
        //public string ServidorBackendUrl => BuildUrl(IpServidor, Puerto, UsaHttpsServidorBackend);
        //public string LicenciaBackendUrl => BuildUrl(IpLicenciaBackend, PuertoLicenciaBackend, UsaHttpsLicenciaBackend);

        public string ServidorBackendUrl =>
    !string.IsNullOrWhiteSpace(_urlServidorBackend)
        ? _urlServidorBackend
        : BuildUrl(IpServidor, Puerto, UsaHttpsServidorBackend);

        //public string LicenciaBackendUrl =>
        //    !string.IsNullOrWhiteSpace(_urlLicenciaBackend)
        //        ? _urlLicenciaBackend
        //        : BuildUrl(IpLicenciaBackend, PuertoLicenciaBackend, UsaHttpsLicenciaBackend);

        public string LicenciaBackendUrl
        {
            get
            {
                var raw = !string.IsNullOrWhiteSpace(_urlLicenciaBackend)
                    ? _urlLicenciaBackend
                    : BuildUrl(IpLicenciaBackend, PuertoLicenciaBackend, UsaHttpsLicenciaBackend);

                return BuildSafeUrl(raw);
            }
        }




        private static string BuildSafeUrl(string rawUrl)
        {
            var cleaned = rawUrl.Trim()
                .Replace("\uFEFF", "") // BOM
                .Replace("\u200B", "") // zero-width space
                .Replace("https://https://", "https://")
                .Replace("https//", "https://")
                .Replace("http://http://", "http://")
                .Replace("http//", "http://");

            if (!cleaned.StartsWith("http://") && !cleaned.StartsWith("https://"))
                cleaned = "https://" + cleaned;

            return cleaned.TrimEnd('/') + "/";
        }










        //private static string BuildUrl(string host, int port, bool https)
        //{
        //    var scheme = https ? "https" : "http";
        //    var omitPort = (https && port == 443) || (!https && port == 80);
        //    return omitPort ? $"{scheme}://{host}" : $"{scheme}://{host}:{port}";
        //}



        private static string BuildUrl(string host, int port, bool https)
        {
            var scheme = https ? "https" : "http";
            var omit = (https && port == 443) || (!https && port == 80);
            var url = omit ? $"{scheme}://{host}" : $"{scheme}://{host}:{port}";
            if (!url.EndsWith("/")) url += "/";
            return url;
        }






        //private static string SanitizeBaseUrl(string? url)
        //{
        //    if (string.IsNullOrWhiteSpace(url)) return string.Empty;
        //    url = url.Trim();

        //    // Corrige esquemas sin ':'
        //    url = url.Replace("https//", "https://", StringComparison.OrdinalIgnoreCase)
        //             .Replace("http//", "http://", StringComparison.OrdinalIgnoreCase)
        //             .Replace("https:/", "https://", StringComparison.OrdinalIgnoreCase)
        //             .Replace("http:/", "http://", StringComparison.OrdinalIgnoreCase);

        //    // Si falta esquema, asumí https
        //    if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
        //        !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        //        url = "https://" + url;

        //    // Colapsa esquemas repetidos al inicio (https://https://...)
        //    // Nos quedamos con el último (normalmente https)
        //    var i = url.IndexOf("://", StringComparison.Ordinal);
        //    if (i > 0)
        //    {
        //        // Si hay otro “://” más adelante, recortá para quedarte con uno solo
        //        var j = url.IndexOf("://", i + 3);
        //        if (j > 0) url = url.Substring(0, j) + url.Substring(j + 3); // quita el segundo "://"
        //    }

        //    if (!url.EndsWith("/")) url += "/";
        //    return url;
        //}



        // Ejemplo de uso seguro
        private void LogJs(string mensaje)
        {
            if (JS != null)
            {
                JS.InvokeVoidAsync("console.log", mensaje);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] {mensaje}");
            }
        }





        // ===== Inicialización =====
        public async Task InicializarAsync()
        {
            await Task.Run(LeerConfiguracion);
            EstaCargado = true;
        }

        public void LeerConfiguracion()
        {
            var ruta = GetIniPath();
            System.Diagnostics.Debug.WriteLine($"[CFG] Leyendo INI: {ruta}");
            if (!File.Exists(ruta))
            {
                System.Diagnostics.Debug.WriteLine("[CFG] INI no encontrado. Usando defaults.");
                return;
            }

            var secciones = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            secciones = ParseIni(ruta);



            //if (secciones.TryGetValue("Servidor", out var servidor))
            //{


            //    if (servidor.TryGetValue("ip_servidor", out var ip)) IpServidor = ip;
            //    if (servidor.TryGetValue("puerto", out var pStr) && int.TryParse(pStr, out var p)) Puerto = p;
            //    if (servidor.TryGetValue("https", out var hStr))
            //        UsaHttpsServidorBackend = hStr.Equals("true", StringComparison.OrdinalIgnoreCase);


            //    // En Azure, si https=true y puerto no viene/está en 80, asumir 443
            //    if (IsAzure() && UsaHttpsServidorBackend && (Puerto == 0 || Puerto == 80)) Puerto = 443;
            //}

            //if (secciones.TryGetValue("IpLicenciaBackend", out var lic))
            //{
            //    if (lic.TryGetValue("ip", out var lip)) IpLicenciaBackend = lip;
            //    if (lic.TryGetValue("puerto", out var lpStr) && int.TryParse(lpStr, out var lp)) PuertoLicenciaBackend = lp;
            //    if (lic.TryGetValue("https", out var lhStr))
            //        UsaHttpsLicenciaBackend = lhStr.Equals("true", StringComparison.OrdinalIgnoreCase);

            //    if (IsAzure() && UsaHttpsLicenciaBackend && (PuertoLicenciaBackend == 0 || PuertoLicenciaBackend == 80))
            //        PuertoLicenciaBackend = 443;
            //}

            if (secciones.TryGetValue("Servidor", out var servidor))
            {
                if (servidor.TryGetValue("url", out var urlCompleta))
                {
                    // Si se especifica una URL completa, usala directamente
                    //_urlServidorBackend = urlCompleta.TrimEnd('/');
                    _urlServidorBackend = urlCompleta;
                }
                else
                {
                    if (servidor.TryGetValue("ip_servidor", out var ip)) IpServidor = ip;
                    if (servidor.TryGetValue("puerto", out var pStr) && int.TryParse(pStr, out var p)) Puerto = p;
                    if (servidor.TryGetValue("https", out var hStr))
                        UsaHttpsServidorBackend = hStr.Equals("true", StringComparison.OrdinalIgnoreCase);

                    // En Azure, si https=true y puerto no viene o es 80, asumir 443
                    if (IsAzure() && UsaHttpsServidorBackend && (Puerto == 0 || Puerto == 80))
                        Puerto = 443;

                    var protocolo = UsaHttpsServidorBackend ? "https" : "http";
                    var puertoParte = (UsaHttpsServidorBackend && Puerto == 443) || (!UsaHttpsServidorBackend && Puerto == 80)
                        ? ""
                        : $":{Puerto}";

                    //_urlServidorBackend = $"{protocolo}://{IpServidor}{puertoParte}";
                    _urlServidorBackend = BuildUrl(IpServidor, Puerto, UsaHttpsServidorBackend);
                }
            }

            if (secciones.TryGetValue("IpLicenciaBackend", out var lic))
            {
                System.Diagnostics.Debug.WriteLine("[CFG] Leyendo sección IpLicenciaBackend");

                if (lic.TryGetValue("urlLicencia", out var urlLicenciaCompleta))
                {
                    // Si se especifica una URL completa, usala directamente
                    //_urlLicenciaBackend = urlLicenciaCompleta.TrimEnd('/');
                    //_urlLicenciaBackend = urlLicenciaCompleta;
                    //System.Diagnostics.Debug.WriteLine($"[CFG] URL completa de licencia: {_urlLicenciaBackend}");
                    System.Diagnostics.Debug.WriteLine($"[CFG] Valor crudo de urlLicencia: '{urlLicenciaCompleta}'");
                    LogJs($"[DEBUG] Valor crudo de urlLicencia: '{urlLicenciaCompleta}'");

                    _urlLicenciaBackend = BuildSafeUrl(urlLicenciaCompleta);
                    System.Diagnostics.Debug.WriteLine($"[CFG] URL limpia de licencia: {_urlLicenciaBackend}");
                    LogJs($"[DEBUG] URL limpia de licencia: {_urlLicenciaBackend}");

                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[CFG] Intentando leer sección IpLicenciaBackend");
                    LogJs("[DEBUG] Intentando leer sección IpLicenciaBackend");

                    if (lic.TryGetValue("ip", out var lip)) IpLicenciaBackend = lip;
                    if (lic.TryGetValue("puerto", out var lpStr) && int.TryParse(lpStr, out var lp)) PuertoLicenciaBackend = lp;
                    if (lic.TryGetValue("https", out var lhStr))
                        UsaHttpsLicenciaBackend = lhStr.Equals("true", StringComparison.OrdinalIgnoreCase);

                    System.Diagnostics.Debug.WriteLine($"[CFG] IP Licencia: {IpLicenciaBackend}, Puerto: {PuertoLicenciaBackend}, HTTPS: {UsaHttpsLicenciaBackend}");

                    if (IsAzure() && UsaHttpsLicenciaBackend && (PuertoLicenciaBackend == 0 || PuertoLicenciaBackend == 80))
                        PuertoLicenciaBackend = 443;

                    var protocoloLicencia = UsaHttpsLicenciaBackend ? "https" : "http";
                    var omitPort = (UsaHttpsLicenciaBackend && PuertoLicenciaBackend == 443) || (!UsaHttpsLicenciaBackend && PuertoLicenciaBackend == 80);
                    var puertoLicenciaParte = omitPort ? "" : $":{PuertoLicenciaBackend}";

                    //_urlLicenciaBackend = $"{protocoloLicencia}://{IpLicenciaBackend}{puertoLicenciaParte}";

                    _urlLicenciaBackend = BuildUrl(IpLicenciaBackend, PuertoLicenciaBackend, UsaHttpsLicenciaBackend);
                    System.Diagnostics.Debug.WriteLine($"[CFG] URL construida de licencia: {_urlLicenciaBackend}");
                }
            }



            if (secciones.TryGetValue("Cliente", out var cliente))
            {
                if (cliente.TryGetValue("modo", out var modo))
                    ModoCliente = modo; // "local" | "remoto" | "nube"
            }

            //if (secciones.TryGetValue("Cliente", out var cli))
            //{
            //    if (cli.TryGetValue("modo", out var modo)) ModoCliente = modo;
            //}



            // configuramos si enviamos en el heather jwt o no
            if (secciones.TryGetValue("Auth", out var auth))
            {
                if (auth.TryGetValue("mode", out var modeStr))
                {
                    AuthMode = modeStr?.Trim().ToLower() switch
                    {
                        "off" => AuthMode.Off,
                        "required" => AuthMode.Required,
                        _ => AuthMode.Optional
                    };
                }
            }



        }







        // ===== Helpers de archivo/entorno =====
        //private static string GetIniPath()
        //{
        //    var folder = Path.Combine(AppContext.BaseDirectory, "Config");
        //    Directory.CreateDirectory(folder);
        //    var azure = Path.Combine(folder, "config.azure.ini");
        //    var local = Path.Combine(folder, "config.ini");
        //    return IsAzure() && File.Exists(azure) ? azure : local;
        //}


        //cuando no existe config.ini usa config.azure.ini
        private static string GetIniPath()
        {
            var folder = Path.Combine(AppContext.BaseDirectory, "Config");
            Directory.CreateDirectory(folder);

            var azure = Path.Combine(folder, "config.azure.ini");
            var local = Path.Combine(folder, "config.ini");

            // Si está en Azure y existe config.azure.ini  usarlo
            if (IsAzure() && File.Exists(azure))
                return azure;

            // Si config.ini existe  usarlo
            if (File.Exists(local))
                return local;

            // Si no existe ninguno  usar config.azure.ini como último recurso
            return azure;
        }


        private static bool IsAzure() =>
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")) ||
            string.Equals(Environment.GetEnvironmentVariable("IURIX_MODE"), "Azure", StringComparison.OrdinalIgnoreCase);

        private static Dictionary<string, Dictionary<string, string>> ParseIni(string ruta)
        {
            var map = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            string? current = null;
            //foreach (var raw in File.ReadAllLines(ruta))
            foreach (var raw in File.ReadAllLines(ruta, Encoding.UTF8))
            {
                var s = raw.Trim();
                if (string.IsNullOrWhiteSpace(s)) continue;
                if (s.StartsWith(";") || s.StartsWith("#") || s.StartsWith("//")) continue;

                if (s.StartsWith("[") && s.EndsWith("]"))
                {
                    current = s[1..^1].Trim();
                    map[current] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    continue;
                }
                var eq = s.IndexOf('=');
                if (eq <= 0 || current == null) continue;

                var key = s[..eq].Trim();
                var val = s[(eq + 1)..].Trim();
                map[current][key] = val;

                foreach (var seccion in map)
                {
                    foreach (var kvp in seccion.Value)
                    {                       

                        System.Diagnostics.Debug.WriteLine($"[INI] Sección '{seccion.Key}' clave '{kvp.Key}' = '{kvp.Value}'");
                    }
                }
            }
            return map;
        }





    }

}
