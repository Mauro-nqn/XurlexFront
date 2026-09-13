using IurixBlazor.Services.Auth;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics;

namespace IurixBlazor.Pages;

public class AgendaCalendarBase : ComponentBase
{
    [Inject] ITokenStore TokenStore { get; set; } = default!;   
    [Inject] protected AgendaService AgendaService { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected List<AgendaDto> Eventos = new();



    private DotNetObjectReference<AgendaCalendarBase>? _objRef;


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        //if (firstRender)
        //{
        //    Eventos = await AgendaService.ObtenerTodasAsync();
        //    _objRef = DotNetObjectReference.Create(this);
        //    await JS.InvokeVoidAsync("renderAgendaCalendar", Eventos, _objRef);
        //}

        if (!firstRender) return;

        // Esperar a que el token se bootstrapée (máx. ~1s)
        string? token = await TokenStore.GetAsync();
        for (int i = 0; i < 10 && string.IsNullOrWhiteSpace(token); i++)
        {
            await Task.Delay(100);
            token = await TokenStore.GetAsync();
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            Debug.WriteLine("[AgendaCalendar] No hay token aún; no llamo a la API.");
            return;
        }

        Debug.WriteLine("[AgendaCalendar] Token listo; llamo a AgendaService.");
        Eventos = await AgendaService.ObtenerTodasAsync();
        _objRef = DotNetObjectReference.Create(this);
        await JS.InvokeVoidAsync("renderAgendaCalendar", Eventos, _objRef);
    }



    //[JSInvokable]
    //public async Task OnDiaClick(string isoDate) // ej: "2025-08-09"
    //{
    //    // Pedí al backend los eventos de ese día
    //    var delDia = await AgendaService.ObtenerPorFechaAsync(DateOnly.Parse(isoDate));

    //    // Armá una descripción simple (HTML)
    //    var html = delDia.Any()
    //        ? string.Join("<hr/>", delDia.Select(e =>
    //            $"<div><strong>{e.TipoAgendamientoNombre}</strong> {(e.PersonaNombre is not null ? $" - {e.PersonaNombre}" : "")}<br/>" +
    //            $"{(e.Observaciones is not null ? e.Observaciones : "")}</div>"))
    //        : "<em>Sin eventos para esta fecha.</em>";

    //    await JS.InvokeVoidAsync("setEventoModal", "Eventos del día", isoDate, html);
    //    await JS.InvokeVoidAsync("mostrarModalEventosDia");
    //}

    //    [JSInvokable]
    //    public async Task OnDiaClick(string isoDate)
    //    {
    //        var fecha = DateOnly.Parse(isoDate);
    //        var delDia = await AgendaService.ObtenerPorFechaAsync(fecha);

    //        if (!delDia.Any())
    //        {
    //            await JS.InvokeVoidAsync("setEventoModal",
    //                $"Eventos del {fecha:dd/MM/yyyy}",
    //                fecha.ToString("yyyy-MM-dd"),
    //                "<em>Sin eventos para esta fecha.</em>");
    //            await JS.InvokeVoidAsync("mostrarModalEventosDia");
    //            return;
    //        }

    //        var htmlEventos = delDia.Select(e =>
    //        {
    //            var titulo = $"{e.TipoAgendamientoNombre ?? ""}" +
    //                         $"{(string.IsNullOrWhiteSpace(e.PersonaNombre) ? "" : " - " + e.PersonaNombre)}";

    //            var hora = e.FechaAgendada.ToLocalTime().ToString("HH:mm");

    //            var obs = string.IsNullOrWhiteSpace(e.Observaciones)
    //                ? ""
    //                : $"<div>{e.Observaciones}</div>";

    //            return $@"
    //<div class=""mb-2"">
    //  <div><strong>{hora} - {titulo}</strong></div>
    //  {obs}
    //</div>";
    //        });

    //        var html = string.Join("<hr/>", htmlEventos);

    //        await JS.InvokeVoidAsync("setEventoModal",
    //            $"Eventos del {fecha:dd/MM/yyyy}",
    //            fecha.ToString("yyyy-MM-dd"),
    //            html);

    //        await JS.InvokeVoidAsync("mostrarModalEventosDia");
    //    }



    private static string NombreEstado(EstadoAgenda estado) => estado switch
    {
        EstadoAgenda.Pendiente => "Pendiente",
        EstadoAgenda.Realizado => "Realizado",
        EstadoAgenda.Cancelado => "Cancelado",
        EstadoAgenda.Reprogramado => "Reprogramado",
        _ => "Desconocido"
    };


    private static string ColorEstado(EstadoAgenda estado) => estado switch
    {
        EstadoAgenda.Pendiente => "#dc3545", // rojo
        EstadoAgenda.Realizado => "#198754", // verde
        EstadoAgenda.Cancelado => "#6c757d", // gris
        EstadoAgenda.Reprogramado => "#fd7e14", // naranja
        _ => "#343a40"
    };




    [JSInvokable]
    public async Task OnDiaClick(string isoDate)
    {
        var fecha = DateOnly.Parse(isoDate);
        var delDia = await AgendaService.ObtenerPorFechaAsync(fecha);

        if (!delDia.Any())
        {
            await JS.InvokeVoidAsync(
                "setEventoModal",
                $"Eventos del {fecha:dd/MM/yyyy}",
                fecha.ToString("yyyy-MM-dd"),
                "<em>Sin eventos para esta fecha.</em>"
            );
            await JS.InvokeVoidAsync("mostrarModalEventosDia");
            return;
        }

        var htmlEventos = delDia.Select(e =>
        {
            var hora = e.FechaAgendada.ToLocalTime().ToString("HH:mm");

            var tipo = e.TipoAgendamientoNombre ?? "(sin tipo)";
            var cliente = string.IsNullOrWhiteSpace(e.PersonaNombre) ? "(sin cliente)" : e.PersonaNombre;
            var titulo = string.IsNullOrWhiteSpace(e.Titulo) ? "(sin título)" : e.Titulo;

            var estadoTexto = NombreEstado(e.EstadoAgenda);
            var colorEstado = ColorEstado(e.EstadoAgenda);

            var estadoHtml =
                $@"<span class=""badge"" 
               style=""background-color:{colorEstado};"">
               {estadoTexto}
          </span>";

            var obs = string.IsNullOrWhiteSpace(e.Observaciones)
                ? ""
                : $@"<div class=""mt-1""><strong>Detalle:</strong> {e.Observaciones}</div>";

            return $@"
<div class=""mb-2"">
  <div><strong>{hora}</strong></div>
  <div><strong>Tipo:</strong> {tipo}</div>
  <div><strong>Cliente:</strong> {cliente}</div>
  <div><strong>Título:</strong> {titulo}</div>
  <div><strong>Estado:</strong> {estadoHtml}</div>
  {obs}
</div>";
        });

        var html = string.Join("<hr/>", htmlEventos);

        await JS.InvokeVoidAsync(
            "setEventoModal",
            $"Eventos del {fecha:dd/MM/yyyy}",
            fecha.ToString("yyyy-MM-dd"),
            html
        );

        await JS.InvokeVoidAsync("mostrarModalEventosDia");
    }




    //[JSInvokable]
    //public async Task OnEventoClick(int id)
    //{
    //    var evt = await AgendaService.ObtenerPorIdAsync(id);
    //    var fecha = (evt.FechaAgendada.Kind == DateTimeKind.Utc ? evt.FechaAgendada.ToLocalTime() : evt.FechaAgendada)
    //                .ToString("yyyy-MM-dd HH:mm");
    //    var html = $"<div><strong>{evt.TipoAgendamientoNombre}</strong>" +
    //               $"{(string.IsNullOrWhiteSpace(evt.PersonaNombre) ? "" : $" - {evt.PersonaNombre}")}<br/>" +
    //               $"{(string.IsNullOrWhiteSpace(evt.Observaciones) ? "" : evt.Observaciones)}</div>";

    //    await JS.InvokeVoidAsync("setEventoModal", "Detalle del evento", fecha, html);
    //    await JS.InvokeVoidAsync("mostrarModalEventosDia");
    //}

    //public async ValueTask DisposeAsync()
    //{
    //    _objRef?.Dispose();
    //    await Task.CompletedTask;
    //}


    [JSInvokable]
    public async Task OnEventoClick(int id)
    {
        var evt = await AgendaService.ObtenerPorIdAsync(id);

        var fechaHora = (evt.FechaAgendada.Kind == DateTimeKind.Utc
                            ? evt.FechaAgendada.ToLocalTime()
                            : evt.FechaAgendada);

        var tipo = evt.TipoAgendamientoNombre ?? "(sin tipo)";
        var cliente = string.IsNullOrWhiteSpace(evt.PersonaNombre) ? "(sin cliente)" : evt.PersonaNombre;
        var titulo = string.IsNullOrWhiteSpace(evt.Titulo) ? "(sin título)" : evt.Titulo;
        var estadoTexto = NombreEstado(evt.EstadoAgenda);   // 👈 enum, mismo helper

        var obs = string.IsNullOrWhiteSpace(evt.Observaciones)
            ? ""
            : $@"<div class=""mt-2""><strong>Detalle:</strong> {evt.Observaciones}</div>";

        var html = $@"
<div>
  <div><strong>Fecha y hora:</strong> {fechaHora:dd/MM/yyyy HH:mm}</div>
  <div><strong>Tipo:</strong> {tipo}</div>
  <div><strong>Cliente:</strong> {cliente}</div>
  <div><strong>Título:</strong> {titulo}</div>
  <div><strong>Estado:</strong> {estadoTexto}</div>
  {obs}
</div>";

        await JS.InvokeVoidAsync(
            "setEventoModal",
            "Detalle del evento",
            fechaHora.ToString("yyyy-MM-dd HH:mm"),
            html
        );

        await JS.InvokeVoidAsync("mostrarModalEventosDia");
    }


}
