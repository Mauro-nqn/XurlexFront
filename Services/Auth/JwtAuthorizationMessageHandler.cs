//using System;
//using System.Diagnostics;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Threading;
//using System.Threading.Tasks;

//namespace IurixBlazor.Services.Auth
//{
//    public class JwtAuthorizationMessageHandler : DelegatingHandler
//    {
//        private readonly ITokenStore _tokenStore;

//        public JwtAuthorizationMessageHandler(ITokenStore tokenStore)
//        {
//            _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
//        }

//        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//        {
//            // 👇 Logs para comprobar que es el MISMO TokenStore que usás en el login
//            Debug.WriteLine($"[JWT] Handler usa TokenStore#{_tokenStore.GetHashCode()}");
//            var token = await _tokenStore.GetAsync();

//            var url = request.RequestUri?.ToString() ?? "(sin URL)";
//            if (string.IsNullOrWhiteSpace(token))
//            {
//                Debug.WriteLine($"[JWT] (SIN TOKEN) → {request.Method} {url}");
//            }
//            else
//            {
//                // Adjunta el token
//                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
//                Debug.WriteLine($"[JWT] (CON TOKEN) → {request.Method} {url}");
//            }

//            var resp = await base.SendAsync(request, cancellationToken);
//            Debug.WriteLine($"[JWT] ← {(int)resp.StatusCode} {resp.ReasonPhrase}");

//            return resp;
//        }
//    }
//}


using IurixBlazor.Services.Auth;
using IurixBlazor.Shared.Config;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;

public class JwtAuthorizationMessageHandler : DelegatingHandler
{
    private readonly ITokenStore _tokenStore;
    private readonly ConfigService _config;

    public JwtAuthorizationMessageHandler(ITokenStore tokenStore, ConfigService config)
    {
        _tokenStore = tokenStore;
        _config = config;

        System.Diagnostics.Debug.WriteLine($"[JWT-CTOR] Config #{_config.GetHashCode()}  AuthMode={_config.AuthMode}");
        Console.WriteLine($"[JWT-CTOR] Config #{_config.GetHashCode()}  AuthMode={_config.AuthMode}");
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var path = request.RequestUri?.AbsolutePath?.ToLowerInvariant() ?? "";

        // No tocar login, refresh, o endpoints públicos que vos definas
        if (path.Contains("/api/auth/login") || path.Contains("/api/licencia/clave"))
            return await base.SendAsync(request, ct);

        var token = await _tokenStore.GetAsync();

        switch (_config.AuthMode)
        {
            case AuthMode.Off:
                // No adjuntar nada
                // Garantizá que NO salga Authorization de ningún lado
                request.Headers.Authorization = null;
                System.Diagnostics.Debug.WriteLine($"[JWT] OFF → NO envío Authorization a {request.RequestUri}");
                break;

            case AuthMode.Optional:
                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    System.Diagnostics.Debug.WriteLine($"[JWT] OPTIONAL → envío Bearer (len={token.Length}) a {request.RequestUri}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[JWT] OPTIONAL (SIN TOKEN) → NO envío Authorization a {request.RequestUri}");
                }
                break;


            case AuthMode.Required:
                if (string.IsNullOrWhiteSpace(token))
                {
                    System.Diagnostics.Debug.WriteLine($"[JWT] REQUIRED (SIN TOKEN) → 401 local para {request.RequestUri}");
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        RequestMessage = request,
                        ReasonPhrase = "Token requerido"
                    };
                }
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                System.Diagnostics.Debug.WriteLine($"[JWT] REQUIRED → envío Bearer (len={token.Length}) a {request.RequestUri}");
                break;
        }

        return await base.SendAsync(request, ct);
    }
}
