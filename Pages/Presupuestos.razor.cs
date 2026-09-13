using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;

namespace IurixBlazor.Pages
{
    public class PresupuestosBase : ComponentBase
    {
        [Inject] protected PresupuestoService PresupuestoService { get; set; } = default!;

        [Inject] protected PersonaService PersonaService { get; set; } = default!;

        [Inject] protected NavigationManager Nav { get; set; } = default!;

        [Inject] protected IJSRuntime JS { get; set; } = default!;

        [Inject] protected FacturaPresupuestoService FacturaPresupuestoSrv { get; set; } = default!;


        protected List<PresupuestoDto> Presupuestos { get; set; } = new();
        protected List<PersonaDto> Personas { get; set; } = new();




        protected bool MostrarModalFacturas;
        protected bool CargandoFacturas;
        protected int PresupuestoIdSeleccionado;
        protected List<FacturaResumenDto> FacturasRelacionadas = new();

        //  Estado para el preview
        protected bool MostrarPreviewFactura;
        protected int? FacturaIdPreview;



        // Filtros
        protected DateTime? FiltroDesde { get; set; }
        protected DateTime? FiltroHasta { get; set; }
        protected int? FiltroPersonaId { get; set; }
        protected EstadoPresupuesto? FiltroEstado { get; set; }

        protected bool esMovil = false;


        protected override async Task OnInitializedAsync()
        {
            Personas = await PersonaService.ObtenerPersonasAsync() ?? new();            
            Presupuestos = await PresupuestoService.ObtenerTodosAsync();
            System.Diagnostics.Debug.WriteLine(JsonSerializer.Serialize(Presupuestos));

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


        protected async Task Buscar()
        {
            Presupuestos = await PresupuestoService.BuscarAsync(FiltroDesde, FiltroHasta, FiltroPersonaId, FiltroEstado);
            StateHasChanged();
        }

        protected async Task Limpiar()
        {
            FiltroDesde = null;
            FiltroHasta = null;
            FiltroPersonaId = null;
            FiltroEstado = null;
            Presupuestos = await PresupuestoService.ObtenerTodosAsync();
            StateHasChanged();
        }

     



        protected Task Toast(string msg, string type = "info")
            => JS.InvokeVoidAsync("mostrarToast", msg, type).AsTask();

        protected async Task VerFacturasRelacionadas(int presupuestoId)
        {
            PresupuestoIdSeleccionado = presupuestoId;
            MostrarModalFacturas = true;
            CargandoFacturas = true;
            StateHasChanged();

            try
            {
                FacturasRelacionadas = await FacturaPresupuestoSrv.ObtenerPorPresupuestoAsync(presupuestoId);
            }
            catch (Exception ex)
            {
                // tu helper de toast/log
                await Toast($"No se pudieron cargar las facturas: {ex.Message}", "error");
                FacturasRelacionadas = new();
            }
            finally
            {
                CargandoFacturas = false;
                StateHasChanged();
            }
        }


        protected void CerrarModalFacturas()
        {
            MostrarModalFacturas = false;
            FacturasRelacionadas.Clear();
        }

        //protected void AbrirFactura(int facturaId)
        //{
        //    // navegá a la página de factura / o abrí un modal de factura
        //    Nav.NavigateTo($"/facturas/detalle/{facturaId}");
        //}

        protected void AbrirFactura(int facturaId)
        {
            var returnUrl = Uri.EscapeDataString(Nav.ToBaseRelativePath(Nav.Uri)); // p.ej. "presupuestos"
            Nav.NavigateTo($"/facturas/detalle/{facturaId}?returnUrl={returnUrl}");
        }

        protected void AbrirFacturaPreview(int facturaId)
        {
            FacturaIdPreview = facturaId;
            MostrarPreviewFactura = true;
        }

        protected void CerrarPreviewFactura()
        {
            MostrarPreviewFactura = false;
            FacturaIdPreview = null;
        }

        protected void NuevoPresupuesto()
        {
            Nav.NavigateTo("/presupuestos/editar");
        }

        protected void Ver(int id) => Nav.NavigateTo($"/presupuestos/ver/{id}");

        protected void Enviar(int id) => Nav.NavigateTo($"/presupuestos/ver/{id}?enviar=1");


        protected void Editar(int id)
        {
            Nav.NavigateTo($"/presupuestos/editar/{id}");
        }


        //protected async Task Eliminar(int id)
        //{
        //    var confirmar = await JS.InvokeAsync<bool>("confirm", "¿Seguro que desea eliminar este presupuesto?");
        //    if (confirmar)
        //    {
        //        await PresupuestoService.EliminarAsync(id);
        //        Presupuestos = await PresupuestoService.ObtenerTodosAsync();
        //    }
        //}

        protected async Task Eliminar(int id)
        {
            // Traemos el presupuesto para decidir qué preguntar
            var presupuesto = await PresupuestoService.ObtenerPorIdAsync(id);
            if (presupuesto is null)
            {
                await Toast("No se encontró el presupuesto.", "warning");
                return;
            }

            // Defensa extra: mismo criterio que en la UI
            var puedeEliminar = presupuesto.Estado is EstadoPresupuesto.Presupuestado or EstadoPresupuesto.Cancelado;
            if (!puedeEliminar)
            {
                var msg = presupuesto.Estado switch
                {
                    EstadoPresupuesto.Aprobado => "No se puede eliminar: el presupuesto está Aprobado.",
                    EstadoPresupuesto.FacturadoParcial => "No se puede eliminar: ya tiene facturación parcial.",
                    EstadoPresupuesto.FacturadoTotal => "No se puede eliminar: ya está facturado.",
                    _ => "No se puede eliminar por su estado actual."
                };
                await Toast(msg, "warning");
                return;
            }

            // Armamos el mensaje según si fue enviado por mail
            string mensajeConfirmacion =
                presupuesto.MarcarEnviado ?? false
                ? "⚠️ Este presupuesto fue ENVIADO por email.\n¿Está seguro de eliminarlo? Esta acción no se puede deshacer."
                : "¿Seguro que desea eliminar este presupuesto?";

            var confirmar = await JS.InvokeAsync<bool>("confirm", mensajeConfirmacion);
            if (!confirmar) return;

            try
            {
                await PresupuestoService.EliminarAsync(id);
                await Toast("Presupuesto eliminado.", "success");

                // refrescamos la grilla
                Presupuestos = await PresupuestoService.ObtenerTodosAsync();
                StateHasChanged();
            }
            catch (HttpRequestException ex)
            {
                await Toast($"No se pudo eliminar el presupuesto. {ex.Message}", "error");
            }
        }




        protected static bool PuedeEliminar(PresupuestoDto p)
        => p.Estado is EstadoPresupuesto.Presupuestado or EstadoPresupuesto.Cancelado;

        protected static string? MotivoBloqueoEliminar(PresupuestoDto p)
            => p.Estado switch
            {
                EstadoPresupuesto.Aprobado => "No se puede eliminar: el presupuesto está Aprobado.",
                EstadoPresupuesto.FacturadoParcial => "No se puede eliminar: ya tiene facturación parcial.",
                EstadoPresupuesto.FacturadoTotal => "No se puede eliminar: ya está facturado.",
                _ => null
            };
    }
}
