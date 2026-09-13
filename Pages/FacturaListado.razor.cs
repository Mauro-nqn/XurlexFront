using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;

namespace iurixGo.Client.Pages.FacturaListado
{
    public class FacturasListadoBase : ComponentBase
    {
        [Inject] protected FacturaService FacturaService { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;        
        [Inject] protected PersonaService PersonaService { get; set; } = default!;
        [Inject] protected UsuarioService UsuarioService { get; set; } = default!;
        [Inject] protected PuntoVentaService PuntoVentaService { get; set; } = default!;

        //protected List<FacturaDto>? Facturas;
        [Inject] protected IJSRuntime JS { get; set; } = default!;





        protected List<FacturaListItemDto>? Facturas;
        protected List<PersonaDto> Personas = new();
        protected List<UsuarioDto> Usuarios = new();
        protected List<PuntoVentaDto> PuntosVenta = new();


        // Filtros
        protected DateOnly? FiltroDesde { get; set; }
        protected DateOnly? FiltroHasta { get; set; }
        protected int? FiltroPersonaId { get; set; }
        protected int? FiltroUsuarioId { get; set; }
        protected int? FiltroPuntoVentaId { get; set; }
        protected string? FiltroNumero { get; set; }
        protected string? FiltroOperacion { get; set; }


        protected bool esMovil = false;


        protected override async Task OnInitializedAsync()
        {
            Personas = await PersonaService.ObtenerPersonasAsync() ?? new();
            Usuarios = await UsuarioService.ObtenerUsuariosAsync() ?? new();
            PuntosVenta = await PuntoVentaService.ObtenerTodosAsync() ?? new();

            Facturas = await FacturaService.BuscarAsync(null, null, null, null, null, null, null);
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
            Facturas = await FacturaService.BuscarAsync(
                FiltroDesde, FiltroHasta, FiltroPersonaId, FiltroUsuarioId, FiltroPuntoVentaId, FiltroNumero, FiltroOperacion);
            StateHasChanged();
        }

        protected async Task Limpiar()
        {
            FiltroDesde = null;
            FiltroHasta = null;
            FiltroPersonaId = null;
            FiltroUsuarioId = null;
            FiltroPuntoVentaId = null;
            FiltroNumero = null;
            FiltroOperacion = null;

            Facturas = await FacturaService.BuscarAsync(null, null, null, null, null, null, null);
            StateHasChanged();
        }



        //protected void VerDetalle(int id)
        //{
        //    Navigation.NavigateTo($"/facturas/detalle/{id}");
        //}


        protected async Task VerDetalle(int id)
        {
            var url = $"/facturas/preview/{id}";
        await JS.InvokeVoidAsync("open", url, "_blank", "width=860,height=660,scrollbars=yes,resizable=yes");
        }

        //protected void PreviewFactura(int id)
        //{
        //    Navigation.NavigateTo($"/facturas/preview/{id}");
        //}


        protected async Task PreviewFactura(int id)
        {
            var url = $"/facturas/preview/{id}";
            await JS.InvokeVoidAsync("open", url, "_blank", "width=860,height=660,scrollbars=yes,resizable=yes");
        }
        //protected void ImprimirFactura(int id)
        //{
        //    Navigation.NavigateTo($"/facturas/impresion/{id}");
        //}

        protected async Task ImprimirFactura(int id)
        {
            var url = $"/facturas/impresion/{id}";
            await JS.InvokeVoidAsync("open", url, "_blank", "width=860,height=660,scrollbars=yes,resizable=yes");
        }

        protected void NuevaFactura()
        {
            Navigation.NavigateTo("/facturas/nueva");
        }


        //protected async Task EliminarFactura(int id)
        //{
        //    var confirmar = await JS.InvokeAsync<bool>(
        //        "confirm",
        //        "¿Está seguro que desea eliminar esta factura? Esta acción no puede deshacerse."
        //    );

        //    if (!confirmar)
        //        return;

        //    try
        //    {
        //        await FacturaService.EliminarAsync(id);
        //        await JS.InvokeVoidAsync("mostrarToast", "🗑 Factura eliminada correctamente.", "success");

        //        // Recargar listado
        //        Facturas = await FacturaService.BuscarAsync(
        //            FiltroDesde, FiltroHasta, FiltroPersonaId, FiltroUsuarioId, FiltroPuntoVentaId, FiltroNumero);

        //        StateHasChanged();
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        await JS.InvokeVoidAsync("mostrarToast", $"No se pudo eliminar la factura. {ex.Message}", "error");
        //    }
        //}

        protected async Task EliminarFactura(int id)
        {
            var confirmar = await JS.InvokeAsync<bool>(
                "confirm",
                "¿Está seguro que desea eliminar esta factura? Esta acción no puede deshacerse."
            );

            if (!confirmar)
                return;

            try
            {
                await FacturaService.EliminarAsync(id);
                await JS.InvokeVoidAsync("mostrarToast", "🗑 Factura eliminada correctamente.", "success");

                // Recargar listado con los filtros actuales
                Facturas = await FacturaService.BuscarAsync(
                    FiltroDesde, FiltroHasta, FiltroPersonaId, FiltroUsuarioId, FiltroPuntoVentaId, FiltroNumero, FiltroOperacion);

                StateHasChanged();
            }
            catch (InvalidOperationException ex)
            {
                // Mensaje de negocio (ej: “tiene movimientos en Cta Cte”)
                await JS.InvokeVoidAsync("mostrarToast", ex.Message, "warning");
            }
            catch (HttpRequestException ex)
            {
                // Error técnico (red, servidor, etc.)
                await JS.InvokeVoidAsync("mostrarToast", $"No se pudo eliminar la factura. {ex.Message}", "error");
            }
        }




    }
}
