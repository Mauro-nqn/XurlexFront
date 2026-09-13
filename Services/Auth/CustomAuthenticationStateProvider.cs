//using IurixBlazor.Services.Auth;
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.JSInterop;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;

//public class CustomAuthenticationStateProvider : AuthenticationStateProvider
//{
//    private readonly IJSRuntime _js;
//    private readonly ITokenStore _tokenStore;

//    public CustomAuthenticationStateProvider(IJSRuntime js, ITokenStore tokenStore)
//    {
//        _js = js;
//        _tokenStore = tokenStore;
//    }

//    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
//    {
//        //var token = await _js.InvokeAsync<string>("localStorage.getItem", "token");

//        var token = await _tokenStore.GetAsync();

//        if (string.IsNullOrWhiteSpace(token))
//            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

//        var handler = new JwtSecurityTokenHandler();
//        JwtSecurityToken jwt;

//        try
//        {
//            jwt = handler.ReadJwtToken(token);
//        }
//        catch
//        {
//            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
//        }

//        if (jwt.ValidTo < DateTime.UtcNow)
//        {
//            await _js.InvokeVoidAsync("localStorage.removeItem", "token");
//            await _tokenStore.ClearAsync();

//            if (this is CustomAuthenticationStateProvider provider)
//                provider.NotifyUserLogout();

//            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
//        }

//        var claims = jwt.Claims;
//        var identity = new ClaimsIdentity(claims, "jwt");
//        var user = new ClaimsPrincipal(identity);

//        return new AuthenticationState(user);
//    }

//    public void NotifyUserAuthentication(string token)
//    {
//        var handler = new JwtSecurityTokenHandler();
//        var jwt = handler.ReadJwtToken(token);
//        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
//        var user = new ClaimsPrincipal(identity);

//        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
//    }

//    public void NotifyUserLogout()
//    {
//        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
//        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
//    }
//}


//using IurixBlazor.Services.Auth;
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.JSInterop;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;

//public class CustomAuthenticationStateProvider : AuthenticationStateProvider
//{
//    private readonly IJSRuntime _js;
//    private readonly ITokenStore _tokenStore;

//    private static readonly AuthenticationState AnonymousState =
//        new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

//    public CustomAuthenticationStateProvider(IJSRuntime js, ITokenStore tokenStore)
//    {
//        _js = js;
//        _tokenStore = tokenStore;
//    }

//    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
//    {
//        string? token = null;

//        try
//        {
//            // leer desde localStorage
//            token = await _js.InvokeAsync<string>("localStorage.getItem", "token");
//        }
//        catch
//        {
//            // en Blazor Server puede fallar en prerender → anónimo
//            return AnonymousState;
//        }

//        if (string.IsNullOrWhiteSpace(token))
//            return AnonymousState;

//        var handler = new JwtSecurityTokenHandler();
//        JwtSecurityToken jwt;

//        try
//        {
//            jwt = handler.ReadJwtToken(token);
//        }
//        catch
//        {
//            await LimpiarSesionAsync();
//            return AnonymousState;
//        }

//        // jwt.ValidTo ya viene en UTC
//        if (jwt.ValidTo < DateTime.UtcNow)
//        {
//            await LimpiarSesionAsync();
//            return AnonymousState;
//        }

//        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
//        var user = new ClaimsPrincipal(identity);

//        await _tokenStore.SetAsync(token); // mantener sincronizado

//        return new AuthenticationState(user);
//    }

//    // ✅ Ahora Task, no void
//    public async Task NotifyUserAuthenticationAsync(string token)
//    {
//        var handler = new JwtSecurityTokenHandler();
//        var jwt = handler.ReadJwtToken(token);
//        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
//        var user = new ClaimsPrincipal(identity);

//        await _tokenStore.SetAsync(token);

//        // avisar a Blazor que el usuario cambió
//        NotifyAuthenticationStateChanged(
//            Task.FromResult(new AuthenticationState(user)));
//    }

//    // ✅ También Task
//    public async Task NotifyUserLogoutAsync()
//    {
//        await LimpiarSesionAsync();
//        NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState));
//    }

//    private async Task LimpiarSesionAsync()
//    {
//        try
//        {
//            await _js.InvokeVoidAsync("localStorage.removeItem", "token");
//            await _js.InvokeVoidAsync("localStorage.removeItem", "token_expiracion");
//            // si querés, podés borrar más cosas acá,
//            // pero lo mínimo crítico para auth es el token
//        }
//        catch
//        {
//            // ignorar errores de JSInterop
//        }

//        await _tokenStore.ClearAsync();
//    }
//}


using IurixBlazor.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private readonly ITokenStore _tokenStore;

    private static readonly AuthenticationState AnonymousState =
        new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

    public CustomAuthenticationStateProvider(IJSRuntime js, ITokenStore tokenStore)
    {
        _js = js;
        _tokenStore = tokenStore;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = null;

        try
        {
            // leer desde localStorage
            token = await _js.InvokeAsync<string>("localStorage.getItem", "token");
        }
        catch
        {
            // en Blazor Server puede fallar en prerender → anónimo
            return AnonymousState;
        }

        if (string.IsNullOrWhiteSpace(token))
            return AnonymousState;

        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwt;

        try
        {
            jwt = handler.ReadJwtToken(token);
        }
        catch
        {
            await LimpiarSesionAsync();
            return AnonymousState;
        }

        if (jwt.ValidTo < DateTime.UtcNow)
        {
            await LimpiarSesionAsync();
            return AnonymousState;
        }

        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        await _tokenStore.SetAsync(token); // opcional

        return new AuthenticationState(user);
    }

    public async Task NotifyUserAuthentication(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        await _tokenStore.SetAsync(token);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task NotifyUserLogout()
    {
        await LimpiarSesionAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState));
    }

    private async Task LimpiarSesionAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "token");
            await _js.InvokeVoidAsync("localStorage.removeItem", "token_expiracion");
        }
        catch { }

        await _tokenStore.ClearAsync();
    }
}










