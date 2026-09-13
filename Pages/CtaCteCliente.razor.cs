using IurixBlazor.Pages;
using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.JSInterop;
using System.Collections.Generic;

public class CtaCteClienteBase : ComponentBase
{
    [Parameter] public int PersonaId { get; set; }

    [Inject] protected CuentaCorrienteService CtaSrv { get; set; } = default!;
    [Inject] protected ReciboService ReciboSrv { get; set; } = default!;
    [Inject] protected MedioPagoService MedioSrv { get; set; } = default!;
    [Inject] protected PersonaService PersonaService { get; set; } = default!;

    [Inject] protected NavigationManager Nav { get; set; } = default!;

    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected string PersonaNombre { get; set; } = "";
    protected CuentaCorrienteDto? Cuenta { get; set; }
    protected List<MovimientoCuentaDto> Movimientos { get; set; } = new();
    //protected List<(MovimientoCuentaDto dto, decimal Running)> MovimientosOrdenados { get; set; } = new();

    protected List<MovimientoRow> MovimientosOrdenados { get; set; } = new();
    
    

    protected bool MostrarEditarRecibo { get; set; }
    protected ReciboDto ReciboEdicion { get; set; } = new();

    protected ReciboVistaDto? ReciboVista;

    protected decimal MontoOriginalRecibo { get; set; }

    protected bool ReciboYaCreado { get; set; } = false;
    protected int? ReciboIdCreado { get; set; } = null;


    protected decimal importeTotalRecibo { get; set; }
    protected decimal DebitosTotal { get; set; }
    protected decimal CreditosTotal { get; set; }
    protected decimal Saldo { get; set; }

    // Recibo + Imputación
    protected bool MostrarRecibo { get; set; }
    protected CrearReciboDto Recibo { get; set; } = new();
    protected List<MedioPagoDto> MediosPago { get; set; } = new();
    protected MovimientoCuentaDto? CreditoCreado { get; set; }
    //protected List<(MovimientoCuentaDto debito, decimal saldo, decimal imputar)> DebitosAbiertos { get; set; } = new();
    protected List<DebitoAbiertoRow> DebitosAbiertos { get; set; } = new();

    // View-model para la grilla de movimientos con saldo acumulado
    public class MovimientoRow
    {
        public MovimientoCuentaDto Mov { get; set; } = new();
        public decimal Running { get; set; }
    }

    // View-model para débito abierto con monto a imputar
    public class DebitoAbiertoRow
    {
        public MovimientoCuentaDto Debito { get; set; } = new();
        public decimal Saldo { get; set; }
        public decimal Imputar { get; set; }  // editable desde UI
    }


    protected bool MostrarFormCrearCuenta { get; set; } = false;


    protected CrearCuentaCorrienteDto CuentaNueva { get; set; } = new()
    {
        PersonaId = 0,              // lo seteo en OnInitialized
        FechaApertura = DateTime.Now,
        SaldoApertura = 0m,
        Activa = true,
        LimiteCredito = null
    };

    protected decimal? LimiteCredito { get; set; }        // null = sin límite
    protected decimal DeudaActual { get; set; }            // max(0, Débitos - Créditos)
    protected decimal? DisponibleCredito { get; set; }     // Limite - DeudaActual (o null)


    protected bool MostrarEditarCuenta { get; set; } = false;
    protected CuentaEdicionVm CuentaEdicion { get; set; } = new();

    protected HashSet<int> RecibosReversados { get; set; } = new();
    protected HashSet<int> PresupuestosReversados { get; set; } = new();



    protected ImputarReciboModal? _imputarModalCtaCte;



    // según tu modelo, obtenés el saldo actual del recibo desde la fila seleccionada

    protected bool esMovil = false;


    protected override async Task OnInitializedAsync()
    {
        var p = await PersonaService.ObtenerPersonaPorIdAsync(PersonaId);
        PersonaNombre = p is null ? $"Cliente {PersonaId}" :
            (!string.IsNullOrWhiteSpace(p.RazonSocial) ? p.RazonSocial : $"{p.Apellido}, {p.Nombre}");


        // defaults form
        CuentaNueva.PersonaId = PersonaId;
        CuentaNueva.FechaApertura = DateTime.Now;
        CuentaNueva.SaldoApertura = 0m;
        CuentaNueva.Activa = true;
        CuentaNueva.LimiteCredito = null;




        MediosPago = await MedioSrv.ObtenerTodosAsync() ?? new();
        await CargarCuentaYMovimientos();
    }

    //protected async Task CrearCuenta()
    //{
    //    // SaldoApertura = 0; Fecha ahora; Activa = true
    //    Cuenta = await CtaSrv.CrearAsync(PersonaId);
    //    await CargarCuentaYMovimientos();
    //    StateHasChanged();
    //}

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var width = await JS.InvokeAsync<int>("obtenerAnchoPantalla");
            esMovil = width < 768; // breakpoint Bootstrap sm/md

            StateHasChanged();
        }
    }

    protected Task Toast(string m, string t = "info") 
        => JS.InvokeVoidAsync("mostrarToast", m, t).AsTask();

    protected void ToggleCrearCuenta()
    {
        MostrarFormCrearCuenta = !MostrarFormCrearCuenta;
        if (MostrarFormCrearCuenta)
        {
            // reset defaults cada vez que abro
            CuentaNueva = new CrearCuentaCorrienteDto
            {
                PersonaId = PersonaId,
                FechaApertura = DateTime.Now,
                SaldoApertura = 0m,
                Activa = true,
                LimiteCredito = null
            };
        }
    }

    protected async Task ConfirmarCrearCuenta()
    {
        var creada = await CtaSrv.CrearAsync(
            personaId: PersonaId,
            fechaApertura: CuentaNueva.FechaApertura,
            saldoApertura: CuentaNueva.SaldoApertura,
            activa: CuentaNueva.Activa,
            limiteCredito: CuentaNueva.LimiteCredito
        );

        Cuenta = creada;
        MostrarFormCrearCuenta = false;

        await CargarCuentaYMovimientos();
        StateHasChanged();
    }



    public class CuentaEdicionVm
    {
        public bool Activa { get; set; }
        public decimal? LimiteCredito { get; set; }
    }

    protected void ToggleEditarCuenta()
    {
        MostrarEditarCuenta = !MostrarEditarCuenta;
        if (MostrarEditarCuenta && Cuenta is not null)
        {
            CuentaEdicion = new CuentaEdicionVm
            {
                Activa = Cuenta.Activa,
                LimiteCredito = Cuenta.LimiteCredito
            };
        }
    }

    protected async Task ConfirmarEditarCuenta()
    {
        if (Cuenta is null) return;

        var actualizada = await CtaSrv.PatchAsync(Cuenta.Id, CuentaEdicion.Activa, CuentaEdicion.LimiteCredito);
        if (actualizada is not null)
        {
            Cuenta = actualizada;
            // refrescar métricas
            LimiteCredito = Cuenta.LimiteCredito;
            // DisponibleCredito se recalcula en CargarCuentaYMovimientos, o podés hacerlo acá:
            DeudaActual = Math.Max(0m, DebitosTotal - CreditosTotal);
            DisponibleCredito = LimiteCredito.HasValue ? LimiteCredito.Value - DeudaActual : (decimal?)null;
        }
        MostrarEditarCuenta = false;
        StateHasChanged();
    }






    protected async Task CargarCuentaYMovimientos()
    {
        Cuenta = await CtaSrv.ObtenerPorPersonaAsync(PersonaId);
        if (Cuenta is null) 
        { 
            Movimientos.Clear();

            // reset info crédito
            LimiteCredito = null;
            DeudaActual = 0m;
            DisponibleCredito = null;

            Recalcular(); 
            return; 
        }

        var lista = await CtaSrv.ObtenerMovimientosPorCuentaAsync(Cuenta.Id) ?? new();
        Movimientos = lista;

        Recalcular();

        // calcular crédito
        LimiteCredito = Cuenta.LimiteCredito;
        DeudaActual = Math.Max(0m, DebitosTotal - CreditosTotal); // lo que debe el cliente
        DisponibleCredito = LimiteCredito.HasValue ? LimiteCredito.Value - DeudaActual : (decimal?)null;
    }

    //private void Recalcular()
    //{
    //    DebitosTotal = Movimientos.Where(m => m.Tipo == TipoMovimientoCuenta.Debito).Sum(m => m.Importe ?? 0);
    //    CreditosTotal = Movimientos.Where(m => m.Tipo == TipoMovimientoCuenta.Credito).Sum(m => m.Importe ?? 0);
    //    Saldo = CreditosTotal - DebitosTotal; // (Créditos - Débitos) si querés al revés, invertí

    //    // Running balance cronológico
    //    var ord = Movimientos.OrderBy(m => m.Fecha).ThenBy(m => m.Id).ToList();
    //    decimal running = 0;
    //    MovimientosOrdenados = ord.Select(m =>
    //    {
    //        running += (m.Tipo == TipoMovimientoCuenta.Credito ? (m.Importe ?? 0) : -(m.Importe ?? 0));
    //        return (m, running);
    //    }).ToList();
    //}



    protected void EditarPresupuestoDesdeCta(int presupuestoId)
    {
        var returnUrl = $"/clientes/{PersonaId}/cta-cte";
        var url = $"/presupuestos/editar/{presupuestoId}?returnUrl={Uri.EscapeDataString(returnUrl)}";
        Nav.NavigateTo(url);
    }






    private void Recalcular()
    {
        DebitosTotal = Movimientos.Where(m => m.Tipo == TipoMovimientoCuenta.Debito).Sum(m => m.Importe ?? 0);
        CreditosTotal = Movimientos.Where(m => m.Tipo == TipoMovimientoCuenta.Credito).Sum(m => m.Importe ?? 0);
        Saldo = CreditosTotal - DebitosTotal;


        //No estamos aplicando reversos

        // Detectar reversas
        //RecibosReversados = Movimientos
        //    .Where(m => m.OrigenTipo == OrigenMovimientoCuenta.Ajuste
        //                && m.Tipo == TipoMovimientoCuenta.Debito
        //                && m.OrigenId.HasValue)
        //    .Select(m => m.OrigenId!.Value)
        //    .ToHashSet();

        //PresupuestosReversados = Movimientos
        //    .Where(m => m.OrigenTipo == OrigenMovimientoCuenta.Reversa
        //                && m.Tipo == TipoMovimientoCuenta.Credito
        //                && m.PresupuestoId.HasValue)
        //    .Select(m => m.PresupuestoId!.Value)
        //    .ToHashSet();



        var ord = Movimientos.OrderBy(m => m.Fecha).ThenBy(m => m.Id).ToList();
        decimal running = 0;
        MovimientosOrdenados = ord.Select(m =>
        {
            running += (m.Tipo == TipoMovimientoCuenta.Credito ? (m.Importe ?? 0) : -(m.Importe ?? 0));
            return new MovimientoRow { Mov = m, Running = running };
        }).ToList();
    }

    protected async Task Refrescar() => await CargarCuentaYMovimientos();



    protected void VerRecibo(int reciboId)
    => Nav.NavigateTo($"/recibos/{reciboId}");




    protected void AbrirRecibo()
    {
        MostrarRecibo = true;
        Recibo = new CrearReciboDto
        {
            PersonaId = PersonaId,
            Fecha = DateTime.Now,
            Total = 0,
            MedioPagoId = 0,
            Observaciones = "Pago a cuenta de honorarios" // <- default como “concepto”
        };
        CreditoCreado = null;
        DebitosAbiertos.Clear();
    }


    //protected async Task GuardarRecibo()
    //{
    //    // 1) crear (o recuperar) cuenta
    //    Cuenta ??= await CtaSrv.ObtenerPorPersonaAsync(PersonaId) ?? await CtaSrv.CrearAsync(PersonaId);

    //    // 2) crear Recibo
    //    var rec = await ReciboSrv.CrearAsync(Recibo);
    //    if (rec is null) return;

    //    // 3) crear movimiento CRÉDITO por el recibo
    //    var credito = await CtaSrv.CrearMovimientoAsync(new CrearMovimientoCuentaDto
    //    {
    //        CuentaCorrienteId = Cuenta!.Id!.Value,
    //        Fecha = Recibo.Fecha,
    //        Tipo = TipoMovimientoCuenta.Credito,
    //        Importe = Recibo.Total,
    //        Detalle = $"Recibo #{rec.Id}",
    //        OrigenTipo = OrigenMovimientoCuenta.Recibo,
    //        OrigenId = rec.Id
    //    });
    //    CreditoCreado = credito;

    //    // 4) preparar débito(s) abiertos
    //    var debs = await CtaSrv.ObtenerDebitosAbiertosAsync(Cuenta.Id!.Value);
    //    DebitosAbiertos = debs.Select(d => (d.debito, d.saldo, imputar: 0m)).ToList();
    //}


    //protected async Task GuardarRecibo()
    //{
    //    Cuenta ??= await CtaSrv.ObtenerPorPersonaAsync(PersonaId) ?? await CtaSrv.CrearAsync(PersonaId);

    //    var rec = await ReciboSrv.CrearAsync(Recibo);
    //    if (rec is null) return;

    //    var credito = await CtaSrv.CrearMovimientoAsync(new CrearMovimientoCuentaDto
    //    {
    //        CuentaCorrienteId = Cuenta!.Id,
    //        Fecha = Recibo.Fecha,
    //        Tipo = TipoMovimientoCuenta.Credito,
    //        Importe = Recibo.Total,
    //        Detalle = $"Recibo #{rec.Id}",
    //        OrigenTipo = OrigenMovimientoCuenta.Recibo,
    //        OrigenId = rec.Id
    //    });
    //    CreditoCreado = credito;

    //    // Traer débitos abiertos y pasarlos a VM editable
    //    // después de crear el crédito, cargar débitos abiertos:
    //    var debs = await CtaSrv.ObtenerDebitosAbiertosAsync(Cuenta.Id);

    //    DebitosAbiertos = debs.Select(d => new DebitoAbiertoRow
    //    {
    //        Debito = d.Debito,
    //        Saldo = d.Saldo,
    //        Imputar = 0m
    //    }).ToList();
    //}



    protected async Task GuardarRecibo()
    {
        Cuenta ??= await CtaSrv.ObtenerPorPersonaAsync(PersonaId) ?? await CtaSrv.CrearAsync(PersonaId);

        var rec = await ReciboSrv.CrearAsync(Recibo);
        if (rec is null) return;

        ReciboYaCreado = true;           // <- marcado
        ReciboIdCreado = rec.Id;         // <- opcional, por si querés navegar o ver

        importeTotalRecibo = Recibo.Total;

        var credito = await CtaSrv.CrearMovimientoAsync(new CrearMovimientoCuentaDto
        {
            CuentaCorrienteId = Cuenta!.Id,
            Fecha = Recibo.Fecha,
            Tipo = TipoMovimientoCuenta.Credito,
            Importe = Recibo.Total,
            Detalle = $"Recibo #{rec.Id}",
            OrigenTipo = OrigenMovimientoCuenta.Recibo,
            OrigenId = rec.Id
        });
        CreditoCreado = credito;

        var debs = await CtaSrv.ObtenerDebitosAbiertosAsync(Cuenta.Id);
        DebitosAbiertos = debs.Select(d => new DebitoAbiertoRow
        {
            Debito = d.Debito,
            Saldo = d.Saldo,
            Imputar = importeTotalRecibo, //0m
        }).ToList();

        // Si no hay débitos abiertos, conviene mostrar un aviso sutil
        if (DebitosAbiertos.Count == 0)
        {
            await Toast("Recibo generado como anticipo (sin imputaciones). Podés imputarlo más tarde.", "info");
        }
    }


    protected async Task GuardarSinImputar()
    {
        // Solo tiene sentido si ya se creó el recibo
        var debeRefrescar = ReciboYaCreado;

        MostrarRecibo = false;
        CreditoCreado = null;
        DebitosAbiertos.Clear();

        if (debeRefrescar)
        {
            await CargarCuentaYMovimientos();   // <- refresca la grilla ahora
            StateHasChanged();
            await Toast("Recibo guardado sin imputaciones.", "success");
        }

        ReciboYaCreado = false;
        ReciboIdCreado = null;
    }



    //protected async Task ConfirmarImputaciones()
    //{
    //    if (CreditoCreado is null || Cuenta is null) return;

    //    var totalImputar = DebitosAbiertos.Sum(x => x.imputar);
    //    if (totalImputar <= 0 || totalImputar > (CreditoCreado.Importe ?? 0)) return;

    //    foreach (var x in DebitosAbiertos.Where(x => x.imputar > 0))
    //    {
    //        await CtaSrv.CrearImputacionAsync(new CrearImputacionDto
    //        {
    //            MovimientoCreditoId = CreditoCreado.Id!.Value,
    //            MovimientoCargoId = x.debito.Id!.Value,
    //            Importe = x.imputar,
    //            ReciboId = null, // opcional, podés setear el rec.Id si querés
    //            OrigenTipo = "Recibo",
    //            OrigenId = CreditoCreado.OrigenId
    //        });
    //    }

    //    MostrarRecibo = false;
    //    CreditoCreado = null;
    //    DebitosAbiertos.Clear();
    //    await CargarCuentaYMovimientos();
    //    StateHasChanged();
    //}


    //protected async Task ConfirmarImputaciones()
    //{
    //    if (CreditoCreado is null || Cuenta is null) return;

    //    var totalImputar = DebitosAbiertos.Sum(x => x.Imputar);
    //    if (totalImputar <= 0 || totalImputar > (CreditoCreado.Importe ?? 0)) return;

    //    foreach (var x in DebitosAbiertos.Where(x => x.Imputar > 0))
    //    {
    //        await CtaSrv.CrearImputacionAsync(new CrearImputacionDto
    //        {
    //            MovimientoCreditoId = CreditoCreado.Id,
    //            MovimientoCargoId = x.Debito.Id,
    //            Importe = x.Imputar,
    //            OrigenTipo = "Recibo",
    //            OrigenId = CreditoCreado.OrigenId
    //        });
    //    }

    //    MostrarRecibo = false;
    //    CreditoCreado = null;
    //    DebitosAbiertos.Clear();
    //    await CargarCuentaYMovimientos();
    //    StateHasChanged();
    //}

    protected async Task ConfirmarImputaciones()
    {
        if (CreditoCreado is null || Cuenta is null) return;

        var totalImputar = DebitosAbiertos.Sum(x => x.Imputar);
        if (totalImputar <= 0 || totalImputar > (CreditoCreado.Importe ?? 0)) return;

        foreach (var x in DebitosAbiertos.Where(x => x.Imputar > 0))
        {
            await CtaSrv.CrearImputacionAsync(new CrearImputacionDto
            {
                MovimientoCreditoId = CreditoCreado.Id,
                MovimientoCargoId = x.Debito.Id,
                Importe = x.Imputar,
                OrigenTipo = "Recibo",
                OrigenId = CreditoCreado.OrigenId
            });
        }

        MostrarRecibo = false;
        CreditoCreado = null;
        DebitosAbiertos.Clear();

        // refrescá SIEMPRE tras confirmar
        await CargarCuentaYMovimientos();

        StateHasChanged();

        ReciboYaCreado = false;
        ReciboIdCreado = null;
    }

    //protected void CancelarRecibo()
    //{
    //    MostrarRecibo = false;
    //    CreditoCreado = null;
    //    DebitosAbiertos.Clear();
    //}

    protected async Task CancelarRecibo()
    {
        // si ya existe el crédito del recibo, corresponde refrescar la grilla
        var debeRefrescar = (CreditoCreado != null);

        MostrarRecibo = false;

        // limpiar estado del modal
        CreditoCreado = null;
        DebitosAbiertos.Clear();

        if (debeRefrescar)
        {
            await CargarCuentaYMovimientos();  // ← aparece el recibo recién creado
            StateHasChanged();
            await Toast("Se cerró la imputación. El recibo quedó guardado sin imputar.", "info");
        }
    }





    protected async Task EditarRecibo(int reciboId)
    {
        // 🔹 Cargar la vista enriquecida para saber imputaciones, saldo, etc.
        ReciboVista = await ReciboSrv.ObtenerVistaAsync(reciboId);
        if (ReciboVista is null)
        {
            await Toast("No se pudo obtener la vista del recibo.", "error");
            return;
        }

        // Traer el recibo a editar
        var rec = await ReciboSrv.ObtenerPorIdAsync(reciboId);
        if (rec is null) { await Toast("No se encontró el recibo.", "warning"); return; }

        ReciboEdicion = new ReciboDto
        {
            Id = rec.Id,
            PersonaId = rec.PersonaId,
            Fecha = rec.Fecha,
            Total = rec.Total,
            MedioPagoId = rec.MedioPagoId,
            Observaciones = rec.Observaciones
        };
        MontoOriginalRecibo = rec.Total;

        MostrarEditarRecibo = true;
        StateHasChanged();
    }

    protected void CancelarEdicionRecibo()
    {
        MostrarEditarRecibo = false;
        ReciboEdicion = new();
    }

    protected async Task GuardarEdicionRecibo()
    {
        // 1) si cambió el monto: AJUSTE (no tocamos imputaciones)
        if (ReciboEdicion.Total != MontoOriginalRecibo)
        {
            try
            {
                await ReciboSrv.AjustarMontoAsync(ReciboEdicion.Id, ReciboEdicion.Total);
            }
            catch (HttpRequestException ex)
            {
                await Toast($"No se pudo ajustar el monto. {ex.Message}", "error");
                return;
            }
        }

        // 2) patch metadata (fecha/medio/obs)
        try
        {
            await ReciboSrv.PatchAsync(ReciboEdicion.Id, new ReciboPatchDto
            {
                Fecha = ReciboEdicion.Fecha,
                MedioPagoId = ReciboEdicion.MedioPagoId,
                Observaciones = ReciboEdicion.Observaciones
            });

            await Toast("Recibo actualizado.", "success");
            MostrarEditarRecibo = false;
            await CargarCuentaYMovimientos();
        }
        catch (HttpRequestException ex)
        {
            await Toast($"No se pudo actualizar el recibo. {ex.Message}", "error");
        }
    }


    //Sacamos la opcion de reversar recibo
    //protected async Task EliminarRecibo(int reciboId)
    //{
    //    // Intentamos eliminar “duro”; si hay imputaciones, lo reversamos (anular)
    //    var confirmar = await JS.InvokeAsync<bool>("confirm", "¿Eliminar el recibo? Si tiene imputaciones se anulará (reversa).");
    //    if (!confirmar) return;

    //    try
    //    {
    //        await ReciboSrv.EliminarAsync(reciboId);
    //        await Toast("Recibo eliminado.", "success");
    //    }
    //    catch (HttpRequestException ex) when ((int?)ex.StatusCode == 409)
    //    {
    //        // Tiene imputaciones → reversar
    //        try
    //        {
    //            await ReciboSrv.ReversarAsync(reciboId);
    //            await Toast("Recibo anulado (reversado).", "info");
    //        }
    //        catch (HttpRequestException ex2)
    //        {
    //            await Toast($"No se pudo anular el recibo. {ex2.Message}", "error");
    //            return;
    //        }
    //    }
    //    catch (HttpRequestException ex)
    //    {
    //        await Toast($"No se pudo eliminar el recibo. {ex.Message}", "error");
    //        return;
    //    }

    //    await CargarCuentaYMovimientos();
    //    StateHasChanged();
    //}






    protected async Task EliminarRecibo(int reciboId)
    {
        var confirmar = await JS.InvokeAsync<bool>("confirm",
            "¿Eliminar el recibo? Solo si no tiene imputaciones. Si ya está imputado, no se podrá eliminar.");
        if (!confirmar) return;

        try
        {
            await ReciboSrv.EliminarAsync(reciboId); // backend valida imputaciones
            await Toast("Recibo eliminado.", "success");
        }
        catch (HttpRequestException ex) when ((int?)ex.StatusCode == 409)
        {
            await Toast("No se puede eliminar: el recibo tiene imputaciones. Generá un asiento/movimiento correctivo.", "warning");
        }
        catch (HttpRequestException ex)
        {
            await Toast($"No se pudo eliminar el recibo. {ex.Message}", "error");
        }

        await CargarCuentaYMovimientos();
        StateHasChanged();
    }







    protected async Task AbrirImputarDesdeCtaCte()
    {
        if (ReciboVista is null) return;
        await _imputarModalCtaCte!.AbrirAsync(ReciboVista.Recibo.Id);
    }

    protected async Task OnImputadoDesdeCtaCte()
    {
        if (ReciboVista is null) return;

        var reciboId = ReciboVista.Recibo.Id;

        // refrescar cuenta corriente
        await CargarCuentaYMovimientos();

        // refrescar la vista del recibo
        ReciboVista = await ReciboSrv.ObtenerVistaAsync(reciboId);

        StateHasChanged();
    }



    //protected async Task AnularReciboDesdeCtaCte()
    //{
    //    if (ReciboEdicion is null) return;

    //    var motivo = await JS.InvokeAsync<string?>("prompt",
    //        $"Motivo de anulación del recibo N.º {ReciboEdicion.Id}:");

    //    if (motivo is null) return;

    //    var usuarioIdStr = await JS.InvokeAsync<string>("localStorage.getItem", "usuario_id");
    //    if (!int.TryParse(usuarioIdStr, out var usuarioId))
    //    {
    //        await Toast("No se pudo obtener el usuario actual.", "error");
    //        return;
    //    }

    //    try
    //    {
    //        await ReciboSrv.AnularAsync(ReciboEdicion.Id, new AnularReciboDto
    //        {
    //            UsuarioId = usuarioId,
    //            Motivo = motivo
    //        });

    //        await Toast("Recibo anulado correctamente.", "success");
    //        await CargarCuentaYMovimientos(); // recargar grilla
    //        MostrarEditarRecibo = false;        // si querés cerrar el panel
    //    }
    //    catch (HttpRequestException ex)
    //    {
    //        await Toast($"No se pudo anular el recibo. {ex.Message}", "error");
    //    }
    //}


    protected async Task AnularReciboDesdeCtaCte()
    {
        if (ReciboVista is null) return;

        var confirmar = await JS.InvokeAsync<bool>("confirm",
            $"Vas a ANULAR el recibo N.º {ReciboVista.Recibo.Id}.\n\n" +
            "Si el recibo tiene imputaciones, se desimputará y los débitos asociados " +
            "volverán a quedar pendientes.\n\n" +
            "¿Confirmás?");
        if (!confirmar) return;

        var motivo = await JS.InvokeAsync<string?>("prompt",
            $"Motivo de anulación del recibo N.º {ReciboVista.Recibo.Id}:");
        if (motivo is null) return;

        var usuarioIdStr = await JS.InvokeAsync<string>("localStorage.getItem", "usuario_id");
        if (!int.TryParse(usuarioIdStr, out var usuarioId))
        {
            await Toast("No se pudo obtener el usuario actual.", "error");
            return;
        }

        try
        {
            await ReciboSrv.AnularAsync(ReciboVista.Recibo.Id, new AnularReciboDto
            {
                UsuarioId = usuarioId,
                Motivo = motivo,
                ForzarDesimputar = true  // si tenés ese campo en el DTO
            });

            await Toast("Recibo anulado correctamente.", "success");
            await CargarCuentaYMovimientos(); // recargar grilla
            MostrarEditarRecibo = false;        // si querés cerrar el panel
        }
        catch (HttpRequestException ex)
        {
            await Toast($"No se pudo anular el recibo. {ex.Message}", "error");
        }
    }


    //protected async Task DeshacerAnulacionDesdeCtaCte()
    //{
    //    if (ReciboVista is null) return;

    //    var confirmar = await JS.InvokeAsync<bool>("confirm",
    //        $"¿Deshacer la anulación del recibo N.º {ReciboVista.Recibo.Id}?\n\n" +
    //        "Esta acción es SOLO para admin y modifica la cuenta corriente.");

    //    if (!confirmar) return;

    //    try
    //    {
    //        await ReciboSrv.DeshacerAnulacionAsync(ReciboVista.Recibo.Id);
    //        await Toast("Anulación deshecha. Recibo reactivado.", "success");

    //        await CargarCuentaYMovimientos();
    //        // recargar la vista del recibo para refrescar botones
    //        ReciboVista = await ReciboSrv.ObtenerVistaAsync(ReciboVista.Recibo.Id);
    //        StateHasChanged();
    //    }
    //    catch (HttpRequestException ex)
    //    {
    //        await Toast($"No se pudo deshacer la anulación. {ex.Message}", "error");
    //    }
    //}


    //protected async Task DeshacerReversa(int movimientoId)
    //{
    //    var confirmar = await JS.InvokeAsync<bool>("confirm",
    //        "¿Deshacer esta reversa? Esto eliminará el movimiento de reversa y recalculará el saldo.");
    //    if (!confirmar) return;

    //    try
    //    {
    //        await ReciboSrv.DeshacerReversaMovimientoAsync(movimientoId);
    //        await Toast("Reversa eliminada correctamente.", "success");


    //        await CargarCuentaYMovimientos();
    //    }
    //    catch (HttpRequestException ex)
    //    {
    //        await Toast("No se pudo deshacer la reversa: " + ex.Message, "error");
    //    }
    //}

    protected async Task DeshacerAnulacionDesdeCtaCte(int movimientoReversaId, int? reciboId = null)
    {
        var ok = await JS.InvokeAsync<bool>("confirm",
            "Esto eliminará la reversa y, si es la última, reactivará el recibo. ¿Continuar?");
        if (!ok) return;

        try
        {
            await ReciboSrv.DeshacerReversaMovimientoAsync(movimientoReversaId);

            await Toast("Reversa eliminada.", "success");

            // 🔄 recargar cuenta corriente (la grilla de movimientos)
            await CargarCuentaYMovimientos();

            // 🔄 si me dieron un reciboId, refresco la vista del recibo
            if (reciboId.HasValue && reciboId.Value > 0)
            {
                ReciboVista = await ReciboSrv.ObtenerVistaAsync(reciboId.Value);
                StateHasChanged();
            }
        }
        catch (HttpRequestException ex)
        {
            await Toast($"No se pudo eliminar la reversa. {ex.Message}", "error");
        }
    }






    protected async Task EliminarMovimientoCuentaAsync(int movimientoId)
    {
        var confirmar = await JS.InvokeAsync<bool>(
            "confirm",
            "¿Eliminar este movimiento de la cuenta corriente? Esto no elimina la factura/recibo, solo el asiento en la cuenta.");

        if (!confirmar)
            return;

        try
        {
            await CtaSrv.EliminarMovimientoAsync(movimientoId);

            // volver a cargar los movimientos y recalcular saldos
            await CargarCuentaYMovimientos();   // el mismo método que ya usás al entrar a la página
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            // si usás Toastr o similar:
            // await Toast("No se pudo eliminar el movimiento.", "error");
        }
    }


    protected async Task VerFactura(int id)
    {
        var url = $"/facturas/preview/{id}";
        await JS.InvokeVoidAsync("open", url, "_blank", "width=860,height=660,scrollbars=yes,resizable=yes");
    }


}
