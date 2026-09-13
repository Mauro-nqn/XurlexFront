using IurixBlazor.Pages;
using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static CtaCteClienteBase;

public class ReciboDetalleBase : ComponentBase
{
    [Parameter] public int Id { get; set; }
    [Inject] protected ReciboService ReciboSrv { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected EstudioConfigService EstudioConfigService { get; set; } = default!;

    [Inject] protected CuentaCorrienteService CtaSrv { get; set; } = default!;

    [Inject] protected MedioPagoService MedioPagoService { get; set; } = default!;

    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected ReciboVistaDto? Vista { get; set; }
    protected string ClienteNombre => Vista is null ? "" :
        (!string.IsNullOrWhiteSpace(Vista.Cliente.RazonSocial) ? Vista.Cliente.RazonSocial : $"{Vista.Cliente.Apellido}, {Vista.Cliente.Nombre}");
   

    //protected string ImporteEnLetras => Vista is null ? "" : NumeroEnLetras(Vista.Recibo.Total);


    protected string LogoUrl { get; set; } = "/images/logo.png";
    protected string NombreEstudio { get; set; } = "Estudio Jurídico";
    protected string PieEstudio { get; set; } = "Dirección • Teléfono • www.tusitio.com";

    protected string NotaAlPie { get; set; } = "Observaciones";


    // Modal imputación
    protected bool MostrarImputar { get; set; }
    protected int MovimientoCreditoId { get; set; }
    protected decimal SaldoDisponible { get; set; }
    protected List<DebitoAbiertoRow> DebitosAbiertosModal { get; set; } = new();
    protected decimal TotalAImputar { get; set; }
    protected string? ValidacionError { get; set; }
    protected bool PuedeConfirmarImputacion =>
        DebitosAbiertosModal.Count > 0 &&
        TotalAImputar > 0 &&
        TotalAImputar <= SaldoDisponible &&
        string.IsNullOrEmpty(ValidacionError);


    protected List<MedioPagoDto> MediosPago { get; set; } = new();
    protected string MedioPagoNombre { get; set; } = "-";


    protected string ImporteEnLetras { get; set; } = "";
    // ya lo setea ActualizarMedioPagoNombreYLetras()

    protected bool EstaAnulado => Vista?.Recibo.Anulado ?? false;

    protected ImputarReciboModal? _imputarModal;



    protected bool PuedeAnularRecibo
    {
        get
        {
            if (Vista is null) return false;
            if (Vista.Recibo.Anulado) return false;

            var hayImputaciones = Vista.Imputaciones?.Any() ?? false;
            if (hayImputaciones) return false;

            // el permiso real lo valida PermisoView, pero esto sirve
            // si querés además lógica propia
            return true;
        }
    }


    protected override async Task OnInitializedAsync()
    {
        await CargarEstudioConfigAsync();
        // Traé la vista del recibo como ya lo hacés (ej: Vista = await ReciboSrv.ObtenerVistaAsync(Id);)
        await RecargarVistaRecibo(); // tu método que llama GET /api/recibos/{id}/vista

        // Cargá medios de pago una vez
        MediosPago = await MedioPagoService.ObtenerTodosAsync() ?? new List<MedioPagoDto>();



        // Resolvemos el nombre
        ActualizarMedioPagoNombreYLetras();
    }


    private void ActualizarMedioPagoNombreYLetras()
    {
        var medioId = Vista?.Recibo.MedioPagoId;
        if (medioId.HasValue)
            MedioPagoNombre = MediosPago.FirstOrDefault(m => m.Id == medioId.Value)?.Nombre ?? $"#{medioId.Value}";
        else
            MedioPagoNombre = "-";

        // Si querés guardar el importe en letras en una propiedad:
        ImporteEnLetras = NumeroEnLetras(Vista?.Recibo.Total ?? 0m);
    }


    // Llamá también a este helper después de PATCH o de ajustar monto:
    private async Task RecargarVistaRecibo()
    {
        Vista = await ReciboSrv.ObtenerVistaAsync(Id);
        ActualizarMedioPagoNombreYLetras();
        StateHasChanged();
    }


    protected Task Toast(string msg, string type = "info")
    => JS.InvokeVoidAsync("mostrarToast", msg, type).AsTask();

    protected async Task CargarEstudioConfigAsync()
    {
        var cfg = await EstudioConfigService.ObtenerAsync(); // puede ser null si aún no creaste el registro
        if (cfg == null) return;

        // Nombre
        if (!string.IsNullOrWhiteSpace(cfg.Nombre))
            NombreEstudio = cfg.Nombre;

        // Logo
        if (!string.IsNullOrWhiteSpace(cfg.LogoBase64))
            LogoUrl = $"data:image/png;base64,{cfg.LogoBase64}";

        // Pie / datos de contacto
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(cfg.Domicilio)) partes.Add(cfg.Domicilio);
        if (!string.IsNullOrWhiteSpace(cfg.Telefono)) partes.Add(cfg.Telefono);
        if (!string.IsNullOrWhiteSpace(cfg.Web)) partes.Add(cfg.Web);
        if (!string.IsNullOrWhiteSpace(cfg.Email)) partes.Add(cfg.Email);

        if (partes.Count > 0)
            PieEstudio = string.Join(" • ", partes);

        // Nota opcional para PDF/email desde Observaciones
        if (!string.IsNullOrWhiteSpace(cfg.Observaciones))
            NotaAlPie = cfg.Observaciones;
    }

    protected void Volver() => Nav.NavigateTo($"/clientes/{Vista?.Recibo.PersonaId}/cta-cte");
    protected async Task Imprimir()
    {
        await JS.InvokeVoidAsync("print"); // window.print()
    }

    // Conversor simple a letras (ES-AR)
    protected string NumeroEnLetras(decimal n)
    {
        long entero = (long)Math.Floor(n);
        int cent = (int)Math.Round((n - entero) * 100m);
        return $"pesos {EnteroEnLetras(entero)} con {(cent > 0 ? $"{cent:00}/100" : "00/100")}";
    }

    private static string EnteroEnLetras(long valor)
    {
        if (valor == 0) return "cero";
        if (valor < 0) return "menos " + EnteroEnLetras(-valor);

        string[] unidades = {"","uno","dos","tres","cuatro","cinco","seis","siete","ocho","nueve",
            "diez","once","doce","trece","catorce","quince","dieciséis","diecisiete","dieciocho","diecinueve"};
        string[] decenas = { "", "", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa" };
        string[] centenas = { "", "ciento", "doscientos", "trescientos", "cuatrocientos", "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos" };

        if (valor < 20) return unidades[valor];
        if (valor < 100)
        {
            var d = (int)(valor / 10);
            var r = (int)(valor % 10);
            if (valor == 20) return "veinte";
            if (valor < 30) return "veinti" + unidades[r];
            return decenas[d] + (r > 0 ? " y " + unidades[r] : "");
        }
        if (valor == 100) return "cien";
        if (valor < 1000)
        {
            var c = (int)(valor / 100);
            var r = valor % 100;
            return centenas[c] + (r > 0 ? " " + EnteroEnLetras(r) : "");
        }
        if (valor < 2000) return "mil" + (valor % 1000 > 0 ? " " + EnteroEnLetras(valor % 1000) : "");
        if (valor < 1000000)
        {
            var m = valor / 1000;
            var r = valor % 1000;
            return EnteroEnLetras(m) + " mil" + (r > 0 ? " " + EnteroEnLetras(r) : "");
        }
        if (valor < 2000000) return "un millón" + (valor % 1000000 > 0 ? " " + EnteroEnLetras(valor % 1000000) : "");
        {
            var mm = valor / 1000000;
            var r = valor % 1000000;
            return EnteroEnLetras(mm) + " millones" + (r > 0 ? " " + EnteroEnLetras(r) : "");
        }
    }



    //protected async Task AbrirImputarRecibo()
    //{
    //    if (Vista is null) return;

    //    // 1) saldo disponible del recibo
    //    SaldoDisponible = Vista.SaldoResultante;

    //    // 2) obtener MovimientoCreditoId del recibo
    //    // Opción A: si lo agregaste a ReciboVistaDto -> MovimientoCreditoId = Vista.MovimientoCreditoId;
    //    // Opción B: usar endpoint auxiliar:
    //    try
    //    {
    //        MovimientoCreditoId = await ReciboSrv.ObtenerMovimientoCreditoIdAsync(Vista!.Recibo.Id);
    //        // implementá este método en ReciboService que llame GET /recibos/{id}/mov-credito-id
    //    }
    //    catch (HttpRequestException ex)
    //    {
    //        await Toast($"No se pudo obtener el movimiento del recibo. {ex.Message}", "error");
    //        return;
    //    }

    //    // 3) obtener cuenta y débitos abiertos (facturas abiertas)
    //    try
    //    {
    //        if (!Vista.Recibo.PersonaId.HasValue)
    //        {
    //            await Toast("El recibo no tiene asociado un cliente válido.", "warning");
    //            return;
    //        }

    //        var cuenta = await CtaSrv.ObtenerPorPersonaAsync(Vista.Recibo.PersonaId.Value);

    //        if (cuenta is null)
    //        {
    //            await Toast("El cliente no tiene cuenta corriente.", "warning");
    //            return;
    //        }

    //        var debs = await CtaSrv.ObtenerDebitosAbiertosAsync(cuenta.Id);
    //        DebitosAbiertosModal = debs.Select(d => new DebitoAbiertoRow
    //        {
    //            Debito = d.Debito,
    //            Saldo = d.Saldo,
    //            Imputar = 0m
    //        }).ToList();
    //    }
    //    catch (HttpRequestException ex)
    //    {
    //        await Toast($"No se pudieron cargar débitos abiertos. {ex.Message}", "error");
    //        return;
    //    }

    //    // 4) estado UI
    //    ValidacionError = null;
    //    TotalAImputar = 0m;
    //    MostrarImputar = true;
    //    StateHasChanged();
    //}



    //protected void OnImputarChanged(DebitoAbiertoRow row, ChangeEventArgs e)
    //{
    //    if (e?.Value is null) return;

    //    // Parse decimal (usa cultura invariante si hace falta)
    //    if (!decimal.TryParse(e.Value.ToString(), out var val)) val = 0m;

    //    // Clamp por fila: 0..Saldo de ese débito
    //    if (val < 0) val = 0m;
    //    if (val > row.Saldo) val = row.Saldo;

    //    row.Imputar = val;

    //    // Recalcular total y validaciones
    //    TotalAImputar = DebitosAbiertosModal.Sum(x => x.Imputar);

    //    if (TotalAImputar > SaldoDisponible)
    //        ValidacionError = $"La suma a imputar ({TotalAImputar:C}) supera el saldo disponible del recibo ({SaldoDisponible:C}).";
    //    else
    //        ValidacionError = null;

    //    StateHasChanged();
    //}



    //protected async Task ConfirmarImputacionesRecibo()
    //{
    //    if (!PuedeConfirmarImputacion || MovimientoCreditoId == 0) return;

    //    try
    //    {
    //        foreach (var x in DebitosAbiertosModal.Where(r => r.Imputar > 0))
    //        {
    //            await CtaSrv.CrearImputacionAsync(new CrearImputacionDto
    //            {
    //                MovimientoCreditoId = MovimientoCreditoId,
    //                MovimientoCargoId = x.Debito.Id,
    //                Importe = x.Imputar,
    //                OrigenTipo = "Recibo",
    //                OrigenId = Vista!.Recibo.Id
    //            });
    //        }

    //        await Toast("Imputación realizada.", "success");
    //        CerrarImputar();

    //        // recargar la vista para ver imputaciones, totales y saldo actualizados

    //        await RecargarVistaRecibo();      // si tenés un método que vuelva a llamar GET /recibos/{id}/vista

    //    }
    //    catch (HttpRequestException ex)
    //    {
    //        await Toast($"No se pudo imputar. {ex.Message}", "error");
    //    }
    //}


    protected async Task AbrirImputarRecibo()
    {
        
        if (Vista is null) return;
        await _imputarModal!.AbrirAsync(Vista.Recibo.Id);
    }

    protected async Task OnImputadoRecibo()
    {
        await RecargarVistaRecibo();
    }


    //protected async Task AnularRecibo()
    //{
    //    if (Vista is null) return;

    //    // Preguntar motivo con prompt simple (si ya tenés modal mejor)
    //    var motivo = await JS.InvokeAsync<string?>("prompt",
    //        $"Motivo de anulación del recibo N.º {Vista.Recibo.Id}:");

    //    // Si canceló el prompt
    //    if (motivo is null) return;

    //    var usuarioIdStr = await JS.InvokeAsync<string>("localStorage.getItem", "usuario_id");
    //    if (!int.TryParse(usuarioIdStr, out var usuarioId))
    //    {
    //        await Toast("No se pudo obtener el usuario actual.", "error");
    //        return;
    //    }



    //    try
    //    {
    //        await ReciboSrv.AnularAsync(Vista.Recibo.Id, new AnularReciboDto
    //        {
    //            UsuarioId = usuarioId,
    //            Motivo = motivo
    //        });

    //        await Toast("Recibo anulado correctamente.", "success");

    //        // recargar vista para ver el estado ANULADO y totales
    //        await RecargarVistaRecibo();
    //    }
    //    catch (HttpRequestException ex)
    //    {
    //        await Toast($"No se pudo anular el recibo. {ex.Message}", "error");
    //    }
    //}


    protected async Task AnularRecibo()
    {
        if (Vista is null) return;

        var confirmar = await JS.InvokeAsync<bool>("confirm",
            $"Vas a ANULAR el recibo N.º {Vista.Recibo.Id}.\n\n" +
            "Si el recibo tiene imputaciones, se desimputará y los débitos asociados " +
            "volverán a quedar pendientes.\n\n" +
            "¿Confirmás?");
        if (!confirmar) return;

        var motivo = await JS.InvokeAsync<string?>("prompt",
            $"Motivo de anulación del recibo N.º {Vista.Recibo.Id}:");
        if (motivo is null) return;

        var usuarioIdStr = await JS.InvokeAsync<string>("localStorage.getItem", "usuario_id");
        if (!int.TryParse(usuarioIdStr, out var usuarioId))
        {
            await Toast("No se pudo obtener el usuario actual.", "error");
            return;
        }

        try
        {
            await ReciboSrv.AnularAsync(Vista.Recibo.Id, new AnularReciboDto
            {
                UsuarioId = usuarioId,
                Motivo = motivo
                // ForzarDesimputar = true;  // si tenés ese campo en el DTO
            });

            await Toast("Recibo anulado correctamente.", "success");
            await RecargarVistaRecibo();
        }
        catch (HttpRequestException ex)
        {
            await Toast($"No se pudo anular el recibo. {ex.Message}", "error");
        }
    }





    protected void CerrarImputar()
    {
        MostrarImputar = false;
        DebitosAbiertosModal.Clear();
        TotalAImputar = 0m;
        MovimientoCreditoId = 0;
        ValidacionError = null;
    }

}
