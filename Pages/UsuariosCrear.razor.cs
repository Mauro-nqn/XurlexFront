using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;

using Microsoft.AspNetCore.Components;
using IurixBlazor.Shared.Services;
using IurixBlazor.Services;

public class UsuariosCrearBase : ComponentBase
{
    [Inject] protected UsuarioService? UsuarioService { get; set; }
    [Inject] protected CondicionIvaService? CondicionIvaService { get; set; }
    [Inject] protected NavigationManager? Nav { get; set; }

    protected UsuarioDto usuario = new();
    protected List<RolDto> Roles = new();
   
    protected List<CondicionIvaDto> CondicionIvas = new();

    protected override async Task OnInitializedAsync()
    {
        Roles = await UsuarioService!.ObtenerRolesAsync();
        CondicionIvas = await CondicionIvaService!.ObtenerTodasAsync();

    }

    protected async Task GuardarUsuario()
    {
        var exito = await UsuarioService!.CrearUsuarioAsync(usuario);
        if (exito)
            Nav!.NavigateTo("/usuarios");
    }

    protected void Cancelar() => Nav!.NavigateTo("/usuarios");
}
