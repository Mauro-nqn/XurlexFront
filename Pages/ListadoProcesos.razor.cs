using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;


namespace IurixBlazor.Pages
{
    public partial class ListadoProcesos : ComponentBase
    {
        [Inject] public ProcesoListadoService Service { get; set; } = default!;
        [Inject] public PersonaService PersonaService { get; set; } = default!;
        [Inject] public CaracterIntervencionService CaracterIntervencionService { get; set; } = default!;
        [Inject] public UsuarioService UsuarioService { get; set; } = default!;
        [Inject] public GrupoGestionService GrupoGestionService { get; set; } = default!;
        [Inject] public TipoProcesoService TipoProcesoService { get; set; } = default!;
        [Inject] public JurisdiccionService JurisdiccionService { get; set; } = default!;
        [Inject] public TipoEstadoProcesoService EstadoService { get; set; } = default!;
        [Inject] public NavigationManager Nav { get; set; } = default!;
        
        [Inject] protected IJSRuntime JS { get; set; } = default!;

        protected ListadoProcesosFiltroDto Filtro { get; set; } = new();
        protected List<ProcesoListadoItemDto> Items { get; set; } = new();
        protected int Total { get; set; }
        protected bool Cargando { get; set; }

        protected List<UsuarioDto> Responsables { get; set; } = new();
        protected List<GrupoGestionDto> Grupos { get; set; } = new();
        protected List<TipoProcesoDto> TiposProceso { get; set; } = new();
        protected List<JurisdiccionDto> Jurisdicciones { get; set; } = new();
        protected List<TipoEstadoProcesoDto> Estados { get; set; } = new();


        protected List<CaracterIntervencionDto> Caracteres = new();
        protected string PersonaQuery = "";
        protected List<PersonaDto> PersonasEncontradas = new();
        protected string PersonaSeleccionadaNombre = "";

        protected string? PdfPreviewUrl { get; set; }


        protected override async Task OnInitializedAsync()
        {
            Responsables = await UsuarioService.ObtenerUsuariosAsync();
            Grupos = await GrupoGestionService.ObtenerTodosAsync();
            TiposProceso = await TipoProcesoService.ObtenerTodosAsync();
            Jurisdicciones = await JurisdiccionService.ObtenerTodosAsync();
            Caracteres = await CaracterIntervencionService.ObtenerTodosAsync();

            // si querés “Judicial + Extrajudicial” juntos:
            var ej = await EstadoService.ObtenerPorTipoAsync("Judicial");
            var ex = await EstadoService.ObtenerPorTipoAsync("Extrajudicial");
            Estados = ej.Concat(ex).GroupBy(x => x.Id).Select(g => g.First()).ToList();

            await Buscar();
        }

        protected async Task Buscar()
        {
            Cargando = true;
            try
            {
                var res = await Service.BuscarAsync(Filtro);
                Items = res.Items;
                Total = res.Total;
            }
            finally
            {
                Cargando = false;
            }
        }

        protected void Limpiar()
        {
            Filtro = new ListadoProcesosFiltroDto();
            Items.Clear();
            Total = 0;
        }

        protected void ToggleEstado(int id, bool isChecked)
        {
            if (isChecked)
            {
                if (!Filtro.EstadoIds.Contains(id)) Filtro.EstadoIds.Add(id);
            }
            else
            {
                Filtro.EstadoIds.Remove(id);
            }
        }

        //protected void IrAGestion(int gestionId) => Nav.NavigateTo($"/gestiones/crear-editar/{gestionId}");

        protected void IrAGestion(int gestionId)
        {
            var returnUrl = Nav.ToBaseRelativePath(Nav.Uri);          // ej: "gestiones/listado?..."
            var url = $"/gestiones/crear-editar/{gestionId}?returnUrl={Uri.EscapeDataString("/" + returnUrl)}";
            Nav.NavigateTo(url);
        }

        protected async Task Prev() { Filtro.Page--; await Buscar(); }
        protected async Task Next() { Filtro.Page++; await Buscar(); }




        protected void OnTipoGestionChanged(ChangeEventArgs e)
        {
            Filtro.TipoGestion = e.Value?.ToString();

            if (Filtro.TipoGestion == "Extrajudicial")
            {
                // limpiar filtros que solo aplican a judicial
                Filtro.TipoProcesoId = null;
                Filtro.JurisdiccionId = null;
                Filtro.CircunscripcionId = null;
                Filtro.JuzgadoId = null;
                Filtro.SecretariaId = null;
            }
        }


        protected async Task BuscarPersonas()
        {
            PersonasEncontradas = string.IsNullOrWhiteSpace(PersonaQuery)
                ? new()
                : await PersonaService.BuscarAsync(PersonaQuery); // endpoint tipo: /api/personas/buscar?q=
        }

        protected void SeleccionarPersona(PersonaDto p)
        {
            Filtro.PersonaParteId = p.Id;
            PersonaSeleccionadaNombre = !string.IsNullOrWhiteSpace(p.RazonSocial)
                ? p.RazonSocial
                : $"{p.Apellido}, {p.Nombre}";
            PersonasEncontradas.Clear();
        }


        protected async Task ExportarPdf()
        {
            // Podés exportar con filtros aunque no hayas buscado todavía.
            // Si querés obligar a que haya Items, dejá el disabled.
            var bytes = await Service.ExportPdfAsync(Filtro);

            var base64 = Convert.ToBase64String(bytes);
            var fileName = $"procesos_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            await JS.InvokeVoidAsync("downloadFromBytes", base64, "application/pdf", fileName);


        }
        protected void LimpiarPersona()
        {
            Filtro.PersonaParteId = null;
            PersonaSeleccionadaNombre = "";
        }





        protected async Task PrevisualizarPdf()
        {
            // 1) Pedís el PDF al backend con los mismos filtros que ExportarPdf
            //    (acá adaptá a tu service real)
            byte[] pdfBytes = await Service.ExportPdfAsync(Filtro); // <- ejemplo

            // 2) Convertís a blob URL
            PdfPreviewUrl = await JS.InvokeAsync<string>("pdfPreview.createObjectUrl", pdfBytes);

            // 3) Abrís el modal
            await JS.InvokeVoidAsync("pdfPreview.openModal", "pdfPreviewModal");
        }

        protected async Task CerrarPreview()
        {
            // Cerrar modal desde C# si querés (opcional)
            await JS.InvokeVoidAsync("pdfPreview.closeModal", "pdfPreviewModal");

            // Liberar blob url
            if (!string.IsNullOrWhiteSpace(PdfPreviewUrl))
                await JS.InvokeVoidAsync("pdfPreview.revokeObjectUrl", PdfPreviewUrl);

            PdfPreviewUrl = null;
        }

        protected async Task AbrirEnNuevaPestana()
        {
            if (!string.IsNullOrWhiteSpace(PdfPreviewUrl))
                await JS.InvokeVoidAsync("pdfPreview.openNewTab", PdfPreviewUrl);
        }

        protected async Task ImprimirPreview()
        {
            await JS.InvokeVoidAsync("pdfPreview.printFrame", "pdfPreviewFrame");
        }

    }






}
