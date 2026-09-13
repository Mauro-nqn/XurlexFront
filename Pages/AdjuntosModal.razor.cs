using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Services;
using IurixBlazor.Services;



public partial class AdjuntosModalBase : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ArchivoMovimientoService ArchivoService { get; set; } = default!;

    [Parameter] public int MovimientoId { get; set; }

    // Permite múltiples instancias (IDs configurables)
    [Parameter] public string ModalId { get; set; } = "modalAdjuntos";
    [Parameter] public string VisorId { get; set; } = "modalVisorAdjunto";

    protected List<ArchivoTemporalDto>? Archivos { get; set; }
    protected bool Cargando { get; set; }

    protected ArchivoTemporalDto? ArchivoSeleccionadoParaVer { get; set; }

    protected bool esMovil = false;


    protected bool MostrarPdfSinPreview =>
    esMovil
    && ArchivoSeleccionadoParaVer is not null
    && EsPdf(ArchivoSeleccionadoParaVer.TipoMime);


    // API pública: abrir el modal y cargar datos
    public async Task ShowAsync(int movimientoId)
    {
        MovimientoId = movimientoId;
        await CargarAdjuntosAsync();
        await JS.InvokeVoidAsync("bsModal.show", $"#{ModalId}");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {

            var width = await JS.InvokeAsync<int>("obtenerAnchoPantalla");
            esMovil = width < 768; // breakpoint Bootstrap sm/md

            StateHasChanged();
        }
    }

    protected async Task CerrarAsync()
    {
        await JS.InvokeVoidAsync("bsModal.hide", $"#{ModalId}");
      
    }

    protected async Task CerrarVisorAsync()
    {
        await JS.InvokeVoidAsync("bsModal.hide", $"#{VisorId}");
       

        ArchivoSeleccionadoParaVer = null;
    }

    //protected async Task VerArchivoAsync(ArchivoTemporalDto archivo)
    //{
    //    // Si no tenemos contenido local, lo descargamos
    //    if (!archivo.ContenidoCargado && archivo.Id.HasValue)
    //    {
    //        var contenido = await ArchivoService.DescargarContenidoAsync(archivo.Id.Value);
    //        if (contenido is not null)
    //        {
    //            archivo.Contenido = contenido;
    //        }
    //    }

    //    if (archivo.ContenidoCargado)
    //    {
    //        archivo.DataUrlBase64 = $"data:{archivo.TipoMime};base64,{Convert.ToBase64String(archivo.Contenido)}";
    //        ArchivoSeleccionadoParaVer = archivo;
    //        await JS.InvokeVoidAsync("bsModal.show", $"#{VisorId}");
    //    }
    //}

    protected async Task VerArchivoAsync(ArchivoTemporalDto archivo)
    {
        ArchivoSeleccionadoParaVer = archivo;

        // En móvil, para PDF: NO descargar a memoria, vamos por URL inline
        if (esMovil && archivo.Id.HasValue && EsPdf(archivo.TipoMime))
        {
            await JS.InvokeVoidAsync("bsModal.show", $"#{VisorId}");
            return;
        }

        // Caso normal: traemos contenido y armamos DataUrl (imágenes y desktop)
        if (!archivo.ContenidoCargado && archivo.Id.HasValue)
        {
            var contenido = await ArchivoService.DescargarContenidoAsync(archivo.Id.Value);
            if (contenido is not null)
                archivo.Contenido = contenido;
        }

        if (archivo.ContenidoCargado)
        {
            archivo.DataUrlBase64 = $"data:{archivo.TipoMime};base64,{Convert.ToBase64String(archivo.Contenido)}";
            await JS.InvokeVoidAsync("bsModal.show", $"#{VisorId}");
        }
    }

    //protected string ObtenerUrlDescarga(ArchivoTemporalDto archivo)
    //    => archivo.Id.HasValue
    //        ? $"api/archivomovimiento/{archivo.Id}/contenido"
    //        : $"data:{archivo.TipoMime};base64,{Convert.ToBase64String(archivo.Contenido)}";


    protected string ObtenerUrlDescarga(ArchivoTemporalDto archivo)
    => archivo.Id.HasValue
        ? $"api/archivomovimiento/{archivo.Id}/contenido"
        : $"data:{archivo.TipoMime};base64,{Convert.ToBase64String(archivo.Contenido)}";

    protected string ObtenerUrlVerInline(ArchivoTemporalDto archivo)
        => archivo.Id.HasValue
            ? $"api/archivomovimiento/{archivo.Id}/ver"
            : $"data:{archivo.TipoMime};base64,{Convert.ToBase64String(archivo.Contenido)}";

    private async Task CargarAdjuntosAsync()
    {
        try
        {
            Cargando = true;
            StateHasChanged();

            Archivos = await ArchivoService.ListarPorMovimientoAsync(MovimientoId)
                       ?? new List<ArchivoTemporalDto>();
        }
        finally
        {
            Cargando = false;
            StateHasChanged();
        }
    }

    protected static bool EsImagen(string? mime)
        => mime?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true;

    protected static bool EsPdf(string? mime)
        => string.Equals(mime, MediaTypeNames.Application.Pdf, StringComparison.OrdinalIgnoreCase)
           || string.Equals(mime, "application/pdf", StringComparison.OrdinalIgnoreCase);
}
