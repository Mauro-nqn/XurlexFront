using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;

public class EstudioConfigBase : ComponentBase
{
    [Inject] protected EstudioConfigService Service { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    protected bool Cargando = true;
    protected string? Error;
    protected bool Existe;

    protected EstudioConfigDto? Vista;

    protected CrearEstudioConfigDto CrearDto = new();
    protected ActualizarEstudioConfigDto PatchDto = new();

    protected bool Editando = false;
    protected string? LogoPreviewBase64;

    protected override async Task OnInitializedAsync()
    {
        await Cargar();
    }

    protected async Task Cargar()
    {
        Cargando = true; Error = null; Existe = false; Editando = false; LogoPreviewBase64 = null;

        try
        {
            Vista = await Service.ObtenerAsync();
            if (Vista != null)
            {
                Existe = true;
                // preparar dto de edición con RowVersion para concurrencia
                PatchDto = new ActualizarEstudioConfigDto
                {
                    
                    // opcional: podés precargar valores actuales si querés UX de edición con placeholders
                    Nombre = Vista.Nombre,
                    Domicilio = Vista.Domicilio,
                    Telefono = Vista.Telefono,
                    Email = Vista.Email,
                    Web = Vista.Web,
                    Cuit = Vista.Cuit,
                    Provincia = Vista.Provincia,
                    Localidad = Vista.Localidad,
                    Observaciones = Vista.Observaciones,
                    // Logo: lo manejamos por archivo -> lo seteo sólo si suben algo
                };
            }
            else
            {
                Existe = false;
                CrearDto = new CrearEstudioConfigDto();
            }
        }
        catch (HttpRequestException ex)
        {
            // si el backend devuelve 404, tomalo como "no existe"
            Error = ex.Message;
        }
        finally
        {
            Cargando = false;
            StateHasChanged();
        }
    }

    protected void HabilitarEdicion() => Editando = true;
    protected void CancelarEdicion()
    {
        Editando = false;
        LogoPreviewBase64 = null;
        // revertir cambios
        if (Vista != null)
        {
            PatchDto = new ActualizarEstudioConfigDto
            {
                
                Nombre = Vista.Nombre,
                Domicilio = Vista.Domicilio,
                Telefono = Vista.Telefono,
                Email = Vista.Email,
                Web = Vista.Web,
                Cuit = Vista.Cuit,
                Provincia = Vista.Provincia,
                Localidad = Vista.Localidad,
                Observaciones = Vista.Observaciones
            };
        }
    }

    protected async Task Recargar() => await Cargar();

    // CREAR
    protected async Task Crear()
    {
        try
        {
            await Service.CrearAsync(CrearDto);
            await Cargar();
        }
        catch (Exception ex)
        {
            Error = $"Error al crear: {ex.Message}";
        }
    }

    // GUARDAR (PATCH)
    protected async Task Guardar()
    {
        try
        {
            // Si subieron un logo en la edición por base64, incluimos en PATCH
            if (!string.IsNullOrWhiteSpace(LogoPreviewBase64))
                PatchDto.LogoBase64 = LogoPreviewBase64;

            await Service.PatchAsync(PatchDto);
            await Cargar();
        }
        catch (HttpRequestException ex)
        {
            Error = $"Error HTTP al guardar: {ex.Message}";
        }
        catch (Exception ex)
        {
            Error = $"Error al guardar: {ex.Message}";
        }
    }

    // Archivo -> Base64 para creación
    protected async Task OnLogoFileChangeCrear(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file == null) return;
        using var ms = new MemoryStream();
        await file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024).CopyToAsync(ms);
        CrearDto.LogoBase64 = Convert.ToBase64String(ms.ToArray());
        StateHasChanged();
    }

    // Archivo -> Base64 para edición (vista previa + PATCH)
    protected async Task OnLogoFileChangePatch(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file == null) return;
        using var ms = new MemoryStream();
        await file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024).CopyToAsync(ms);
        LogoPreviewBase64 = Convert.ToBase64String(ms.ToArray());
        StateHasChanged();
    }
}
