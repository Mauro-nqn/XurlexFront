using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using IurixBlazor.Shared.Helpers;
using IurixBlazor.Shared.Services;



public partial class VerMovimientoModalBase : ComponentBase
    {


        [Inject] MovimientoService MovimientoService { get; set; } = default!;
        [Inject] AgendaService AgendaService { get; set; } = default!;
        [Inject] ArchivoMovimientoService ArchivoService { get; set; } = default!;
        [Inject] IJSRuntime JS { get; set; } = default!;

        //------ Estado interno
        
        protected string ModalId { get; } = $"verMovimientoModal_{Guid.NewGuid():N}";

    //  1) Parámetro para el visor (id del modal visor)
    [Parameter] public string VisorId { get; set; } = "modalVisorMovimiento";

    //  2) Estado del archivo que se ve en el visor
    protected ArchivoTemporalDto? ArchivoSeleccionadoParaVer { get; set; }
    protected bool Cargando { get; set; }
    protected string? Error { get; set; }

    protected MovimientoDto? Movimiento { get; set; }
    protected string? NombreProceso { get; set; }
    protected AgendaDto? Agenda { get; set; }

    protected List<ArchivoTemporalDto> Archivos { get; set; } = new();
    protected bool MostrarPreview { get; set; }
    protected ArchivoTemporalDto? ArchivoPreview { get; set; }

    // Apertura diferida (evita timing issues con el DOM)
    private bool _pendingOpen;

    private bool _reopenParentOnClose;

    [Parameter] public EventCallback OnSaved { get; set; }   // <-- requerido
    [Parameter] public EventCallback OnClosed { get; set; }  // <-- si lo pasás en markup, también



    //------ API pública
    // API pública para abrir desde la página que contiene el botón
    public async Task ShowAsync(int movimientoId, string? nombreProceso = null)
    {
        Reset();
        NombreProceso = nombreProceso;
        Cargando = true;
        StateHasChanged();

        try
        {
            Movimiento = await MovimientoService.ObtenerPorIdAsync(movimientoId);

            if (Movimiento is null)
            {
                Error = "No se encontró el movimiento.";
                _pendingOpen = true;   // igual abrimos para mostrar error
                return;
            }

            // Archivos: adaptamos a dto temporal para permitir preview base64
            Archivos = (Movimiento.Archivos ?? Enumerable.Empty<ArchivoMovimientoDto>())
                .Select(a => new ArchivoTemporalDto
                {
                    Id = a.Id,
                    MovimientoId = a.MovimientoId,
                    Nombre = a.Nombre,
                    TipoMime = a.TipoMime,
                    Extension = a.Extension,
                    FechaSubida = a.FechaSubida,
                    UsuarioId = a.UsuarioId
                })
                .ToList();

            // Agenda (si existe)
            Agenda = await AgendaService.ObtenerPorMovimientoAsync(movimientoId);

            _pendingOpen = true;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            _pendingOpen = true;
        }
        finally
        {
            Cargando = false;
            StateHasChanged();
        }
    }

    public async Task Cerrar()
    {
        await JS.InvokeVoidAsync("bsModal.hide", $"#{ModalId}");
        MostrarPreview = false;
        ArchivoPreview = null;
    }





    // para reabrir el padre al cerrar el visor
   

    // ... (resto de propiedades que ya tenías: Movimiento, Archivos, etc.)

    //  3) Abrir visor desde la lista de adjuntos del movimiento
    protected async Task VerArchivoEnVisorAsync(ArchivoTemporalDto archivo)
    {
        try
        {
            // Descarga perezosa si falta contenido
            if (!archivo.ContenidoCargado && archivo.Id.HasValue)
            {
                var contenido = await ArchivoService.DescargarContenidoAsync(archivo.Id.Value);
                if (contenido is not null) archivo.Contenido = contenido;
            }

            if (!archivo.ContenidoCargado)
            {
                await JS.InvokeVoidAsync("mostrarToast", "No se pudo cargar el archivo.", "error");
                return;
            }

            archivo.DataUrlBase64 = $"data:{archivo.TipoMime};base64,{Convert.ToBase64String(archivo.Contenido)}";
            ArchivoSeleccionadoParaVer = archivo;

            // Opcional: ocultar el modal padre para evitar conflictos de foco/backdrop
            _reopenParentOnClose = true;
            await JS.InvokeVoidAsync("bsModal.hide", $"#{ModalId}");

            // Mostrar visor
            await JS.InvokeVoidAsync("bsModal.show", $"#{VisorId}");
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("mostrarToast", $"Error: {ex.Message}", "error");
        }
    }

    //  4) Cerrar visor y (opcional) reabrir el modal padre
    protected async Task CerrarVisorAsync()
    {
        await JS.InvokeVoidAsync("bsModal.hide", $"#{VisorId}");
        if (_reopenParentOnClose)
        {
            _reopenParentOnClose = false;
            await JS.InvokeVoidAsync("bsModal.show", $"#{ModalId}");
        }
    }

    //  5) Helpers de tipo
    protected static bool EsImagen(string? mime) => !string.IsNullOrWhiteSpace(mime) && mime.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    protected static bool EsPdf(string? mime) => string.Equals(mime, "application/pdf", StringComparison.OrdinalIgnoreCase);

    //  6) URL de descarga
    protected string ObtenerUrlDescarga(ArchivoTemporalDto a) =>
        a.Id.HasValue
            ? $"api/archivomovimiento/{a.Id}/contenido"
            : $"data:{a.TipoMime};base64,{Convert.ToBase64String(a.Contenido)}";


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_pendingOpen)
        {
            _pendingOpen = false;
            try
            {
                await JS.InvokeVoidAsync("bsModal.show", $"#{ModalId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al abrir modal: " + ex.Message);
            }
        }
    }

    protected string EscritoResumen =>
        ObtenerResumen(Movimiento?.EscritoPersonalizadoHtml);

    protected static string ObtenerResumen(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        var decoded = System.Net.WebUtility.HtmlDecode(html);
        var plain = System.Text.RegularExpressions.Regex.Replace(decoded, "<.*?>", string.Empty)
            .Replace("\n", " ").Replace("\r", " ").Trim();
        plain = System.Text.RegularExpressions.Regex.Replace(plain, @"\s+", " ");
        return plain.Length <= 120 ? plain : plain[..120] + "...";
    }

    protected async Task VerArchivoAsync(ArchivoTemporalDto archivo)
    {
        try
        {
            if (!archivo.ContenidoCargado && archivo.Id.HasValue)
            {
                var contenido = await ArchivoService.DescargarContenidoAsync(archivo.Id.Value);
                if (contenido is not null)
                    archivo.Contenido = contenido;
            }

            if (archivo.ContenidoCargado)
            {
                archivo.DataUrlBase64 = $"data:{archivo.TipoMime};base64,{Convert.ToBase64String(archivo.Contenido)}";
                ArchivoPreview = archivo;
                MostrarPreview = true;
            }
            else
            {
                await JS.InvokeVoidAsync("mostrarToast", "No se pudo cargar el archivo para vista previa.", "error");
            }
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("mostrarToast", $"Error cargando archivo: {ex.Message}", "error");
        }
    }

    protected void Reset()
    {
        Cargando = false;
        Error = null;
        Movimiento = null;
        NombreProceso = null;
        Agenda = null;
        Archivos = new();
        MostrarPreview = false;
        ArchivoPreview = null;
    }
}



