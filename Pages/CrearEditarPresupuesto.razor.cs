using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.WebUtilities;



public class CrearEditarPresupuestoBase : ComponentBase
{
    [Inject] protected PresupuestoService PresupuestoService { get; set; } = default!;
    [Inject] protected PersonaService PersonaService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    [Inject] protected DistribucionHonorarioService DistribucionService { get; set; } = default!;
    [Inject] protected IvaAlicuotaService IvaAlicuotaService { get; set; } = default!;
    [Inject] protected CuentaCorrienteService CuentaSrv { get; set; } = default!;

    [Inject] protected AjusteVariableService AjusteVariableSrv { get; set; } = default!;

    [Inject] protected IJSRuntime JS { get; set; } = default!;


    [Parameter] public int? Id { get; set; }

    protected PresupuestoDto Presupuesto { get; set; } = new();
    protected List<PresupuestoDetalleDto> Detalles { get; set; } = new();
    protected List<PersonaDto> Personas { get; set; } = new();
    protected List<string> EstadosPresupuesto { get; set; } = Enum.GetNames(typeof(EstadoPresupuesto)).ToList();

    protected List<DistribucionHonorarioDto> Distribuciones { get; set; } = new();

    protected List<IvaAlicuotaDto> IvaAlicuotas { get; set; } = new();

    protected bool CargarCtaCteAlAprobar { get; set; } = false;  
    protected bool RetirarDeCtaCteAlDesaprobar { get; set; } = false;

    protected EstadoPresupuesto EstadoAntes { get; set; }

    protected string? AlertText { get; set; }
    protected string AlertCss { get; set; } = "info";
    protected void ShowAlert(string text, string css = "info") { AlertText = text; AlertCss = css; StateHasChanged(); }


    protected bool CargoPresupuestoYaCargado { get; set; } = false;

    protected bool BusyCtaCte { get; set; } = false;

    protected string? ReturnUrl { get; set; }

    protected List<AjusteVariableDto> Variables { get; set; } = new();
    //protected int? AjusteVariableGlobalId { get; set; } = null;

    //private int? _ajusteVariableGlobalId;
    //protected int? AjusteVariableGlobalId
    //{
    //    get => _ajusteVariableGlobalId;
    //    set
    //    {
    //        _ajusteVariableGlobalId = value;

    //        if (Detalles is null) return;

    //        if (value is null || value <= 0)
    //        {
    //            // limpiar variable en todas las líneas
    //            foreach (var d in Detalles)
    //            {
    //                d.AjusteVariableId = null;
    //                d.VariableCoef = null;
    //                d.VariableValorUsado = null;
    //            }
    //        }
    //        else
    //        {
    //            var varSel = Variables.FirstOrDefault(v => v.Id == value.Value);
    //            foreach (var d in Detalles)
    //            {
    //                d.AjusteVariableId = value;

    //                // Tomamos un "snapshot" de referencia para futuras revalorizaciones
    //                if (varSel is not null)
    //                {
    //                    d.VariableValorUsado = varSel.ValorActual;

    //                    // Solo calculamos coef si el precio actual es > 0 y no hay coef previo
    //                    if (d.PrecioUnitario > 0 && (d.VariableCoef is null || d.VariableCoef == 0))
    //                    {
    //                        d.VariableCoef = Math.Round(
    //                            d.PrecioUnitario / varSel.ValorActual,
    //                            6, MidpointRounding.AwayFromZero);
    //                    }
    //                }
    //            }
    //        }

    //        InvokeAsync(StateHasChanged);
    //    }
    //}

    private int? _ajusteVariableGlobalId;

    protected int? AjusteVariableGlobalId
    {
        get => _ajusteVariableGlobalId;
        set
        {
            _ajusteVariableGlobalId = value;

            if (Detalles is null) return;

            if (value is null || value <= 0)
            {
                // Limpiamos solo el vínculo, NO el histórico
                foreach (var d in Detalles)
                {
                    d.AjusteVariableId = null;
                    // d.VariableCoef = null;
                    // d.VariableValorUsado = null; // 👈 NO tocar acá
                }
            }
            else
            {
                foreach (var d in Detalles)
                {
                    d.AjusteVariableId = value;
                    // 👇 No tocamos VariableValorUsado ni VariableCoef
                }
            }

            InvokeAsync(StateHasChanged);
        }
    }






    protected static readonly EstadoPresupuesto[] EstadosEditables = new[]
        {
            EstadoPresupuesto.Presupuestado,
            EstadoPresupuesto.Aprobado,
            EstadoPresupuesto.Cancelado
        };

    protected bool TieneCargoCtaCte { get; set; } = false; // setealo al cargar
    protected bool EstaFacturado => Presupuesto.Estado is EstadoPresupuesto.FacturadoParcial or EstadoPresupuesto.FacturadoTotal;

    protected bool EstaAsignado => Presupuesto.Estado is EstadoPresupuesto.Asignado;

    protected int selectKey = 0;


    protected bool EsNuevo => !Id.HasValue;


    protected IJSObjectReference? _popoversModule;


    //protected override async Task OnInitializedAsync()
    //{
    //    Personas = await PersonaService.ObtenerPersonasAsync();

    //    if (Id.HasValue)
    //    {
    //        Presupuesto = await PresupuestoService.ObtenerPorIdAsync(Id.Value) ?? new PresupuestoDto();
    //        Detalles = await PresupuestoService.ObtenerDetallesAsync(Id.Value);
    //        CalcularTotales();
    //    }
    //    else
    //    {
    //        Presupuesto = new PresupuestoDto
    //        {
    //            Fecha = DateTime.Now,
    //            Estado = EstadoPresupuesto.Presupuestado,
    //            Detalles = new List<PresupuestoDetalleDto>(),
    //            Neto = 0,
    //            Iva = 0,
    //            Total = 0
    //        };
    //    }
    //}

    protected override async Task OnInitializedAsync()
    {
        Personas = await PersonaService.ObtenerPersonasAsync();
        Distribuciones = await DistribucionService.ObtenerTodosAsync();
        IvaAlicuotas = await IvaAlicuotaService.ObtenerTodasAsync();

        Variables = await AjusteVariableSrv.ObtenerTodosAsync() ?? new();





        if (Id.HasValue)
        {
            Presupuesto = await PresupuestoService.ObtenerPorIdAsync(Id.Value) ?? new PresupuestoDto();

            // 👇 Usamos los detalles que vienen en el DTO principal
            Detalles = Presupuesto.Detalles?.ToList() ?? new List<PresupuestoDetalleDto>();


            //Detalles = await PresupuestoService.ObtenerDetallesAsync(Id.Value);
            EstadoAntes = Presupuesto.Estado;            

            CalcularTotales();

            //  Chequear si ya fue cargado en cta cte
            CargoPresupuestoYaCargado = await CuentaSrv.ExisteCargoPresupuestoAsync(Presupuesto.PersonaId, Presupuesto.Id);
            TieneCargoCtaCte = await CuentaSrv.ExisteCargoPresupuestoAsync(Presupuesto.PersonaId, Presupuesto.Id);

            //if (Detalles.Count > 0 && Detalles.All(d => d.AjusteVariableId == Detalles[0].AjusteVariableId))
            //    AjusteVariableGlobalId = Detalles[0].AjusteVariableId;
            //else
            //    AjusteVariableGlobalId = null;

            // Después de cargar Presupuesto y Detalles:
            if (Detalles.Count > 0 && Detalles.All(d => d.AjusteVariableId == Detalles[0].AjusteVariableId))
                _ajusteVariableGlobalId = Detalles[0].AjusteVariableId;   //  backing field
            else
                _ajusteVariableGlobalId = null;


        }
        else
        {
            Presupuesto = new PresupuestoDto
            {
                Nombre = "",
                Fecha = DateTime.Now,
                Estado = EstadoPresupuesto.Presupuestado,
                Detalles = new List<PresupuestoDetalleDto>(),
                Neto = 0,
                Iva = 0,
                Total = 0
            };

            EstadoAntes = Presupuesto.Estado;
        }


        var uri = Nav.ToAbsoluteUri(Nav.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue("returnUrl", out var val))
            ReturnUrl = val;
    }

    //protected void AgregarDetalle()
    //{
    //    Detalles.Add(new PresupuestoDetalleDto
    //    {
    //        Cantidad = 1,
    //        PrecioUnitario = 0,
    //        IvaAlicuotaId = IvaAlicuotas.FirstOrDefault()?.Id ?? 0 //  Inicializar con la primera alícuota
    //    });
    //    CalcularTotales();
    //}

    // 2) Agregar/Eliminar con defensa (no cambies tu lógica de estado)


    //protected void OnVariableGlobalChanged(int? idSeleccionado)
    //{
    //    AjusteVariableGlobalId = idSeleccionado;

    //    // aplico a todas las líneas
    //    foreach (var d in Detalles)
    //        d.AjusteVariableId = idSeleccionado;

    //    StateHasChanged();
    //}

    protected void OnVariableGlobalChanged(int? idSeleccionado)
    {
        AjusteVariableGlobalId = idSeleccionado;

        foreach (var d in Detalles)
        {
            d.AjusteVariableId = idSeleccionado;

            if (idSeleccionado is null)
            {
                // quitamos variable
                d.VariableValorUsado = null;
                continue;
            }

            // Si la línea nunca tuvo valor anclado, lo seteamos ahora
            if (d.VariableValorUsado is null)
            {
                var variable = Variables.FirstOrDefault(v => v.Id == idSeleccionado);
                if (variable != null)
                {
                    d.VariableValorUsado = variable.ValorActual;
                }
            }
        }

        StateHasChanged();
    }






    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _popoversModule ??= await JS.InvokeAsync<IJSObjectReference>(
                    "import", "/js/popovers.js");

                await _popoversModule.InvokeVoidAsync("initPopovers");
                await _popoversModule.InvokeVoidAsync("initTooltips"); // si usás tooltips
            }
            catch (JSException ex)
            {
                Console.Error.WriteLine($"[JS INIT] {ex.Message}");
            }
        }
    }

    // Llamalo si agregás elementos con popover dinámicamente:
    protected async Task RefreshPopoversAsync()
    {
        if (_popoversModule is not null)
            await _popoversModule.InvokeVoidAsync("initPopovers");
    }

























    //protected async void AgregarDetalle()
    //{
    //    if (EstaBloqueadoDetalles())
    //    {
    //        await Toast("No se pueden agregar ítems: el presupuesto está bloqueado.", "warning");
    //        return;
    //    }

    //    // Elegir IVA 21% de la lista si existe.
    //    var iva21 = IvaAlicuotas?
    //        .FirstOrDefault(a => a.Porcentaje == 21m)
    //        ?? IvaAlicuotas?.FirstOrDefault(a => a.CodigoAfip == 5) // opcional si usás códigos AFIP
    //        ?? IvaAlicuotas?.FirstOrDefault(); // último fallback: la primera

    //    Detalles.Add(new PresupuestoDetalleDto
    //    {            
    //        Cantidad = 1,
    //        PrecioUnitario = 0,
    //        IvaAlicuotaId = iva21?.Id ?? 0,
    //        AjusteVariableId = AjusteVariableGlobalId
    //    });
    //    CalcularTotales();
    //    StateHasChanged();
    //}



    protected async void AgregarDetalle()
    {
        if (EstaBloqueadoDetalles())
        {
            await Toast("No se pueden agregar ítems: el presupuesto está bloqueado.", "warning");
            return;
        }

        // Elegir IVA 21% de la lista si existe.
        var iva21 = IvaAlicuotas?
            .FirstOrDefault(a => a.Porcentaje == 21m)
            ?? IvaAlicuotas?.FirstOrDefault(a => a.CodigoAfip == 5)
            ?? IvaAlicuotas?.FirstOrDefault();

        decimal? variableValorUsado = null;

        if (AjusteVariableGlobalId is not null)
        {
            var varSel = Variables.FirstOrDefault(v => v.Id == AjusteVariableGlobalId);
            if (varSel is not null)
            {
                // Snapshot del valor actual de la variable
                variableValorUsado = varSel.ValorActual;
            }
        }

        var nuevo = new PresupuestoDetalleDto
        {
            Cantidad = 1,
            PrecioUnitario = 0,
            IvaAlicuotaId = iva21?.Id ?? 0,
            AjusteVariableId = AjusteVariableGlobalId,
            VariableValorUsado = variableValorUsado, // 👈 anclado solo para el nuevo
                                                     // VariableCoef lo podés calcular luego cuando tenga precio
        };

        Detalles.Add(nuevo);

        CalcularTotales();
        StateHasChanged();
    }


    //protected void EliminarDetalle(PresupuestoDetalleDto detalle)
    //{
    //    Detalles.Remove(detalle);
    //    CalcularTotales();
    //}


    protected async void EliminarDetalle(PresupuestoDetalleDto detalle)
    {
        if (EstaBloqueadoDetalles())
        {
            await Toast("No se pueden eliminar ítems: el presupuesto está bloqueado.", "warning");
            return;
        }

        Detalles.Remove(detalle);
        CalcularTotales();
        StateHasChanged();
    }

    //protected void CalcularTotales()
    //{
    //    Presupuesto.Neto = Detalles.Sum(d => d.Subtotal);
    //    Presupuesto.Iva = Math.Round(Presupuesto.Neto * 0.21m, 2); // IVA 21%
    //    Presupuesto.Total = Presupuesto.Neto + Presupuesto.Iva;
    //}




    protected bool EstaBloqueadoDetalles()
    {
        if (EsNuevo) return false;

        if (TieneCargoCtaCte) return true;
        return Presupuesto.Estado is EstadoPresupuesto.Aprobado
                                  or EstadoPresupuesto.Asignado  
                                  or EstadoPresupuesto.FacturadoParcial
                                  or EstadoPresupuesto.FacturadoTotal;
    }

    protected string MensajeBloqueoDetalles()
    {
        if (TieneCargoCtaCte)
            return "Bloqueado: el presupuesto tiene cargo en Cuenta Corriente.";
        return "Bloqueado: el presupuesto está Aprobado/Facturado y no admite cambios de ítems.";
    }




    protected IEnumerable<EstadoPresupuesto> EstadosVisibles()
    {
        if (EsNuevo)
            return new[] { EstadoPresupuesto.Presupuestado };

        var visibles = new List<EstadoPresupuesto>
    {
        EstadoPresupuesto.Presupuestado,
        EstadoPresupuesto.Aprobado,
        EstadoPresupuesto.Cancelado
    };

        // Si el estado actual no está en la lista, lo agrego para que se muestre
        if (!visibles.Contains(Presupuesto.Estado))
            visibles.Add(Presupuesto.Estado);

        return visibles;
    }

    //protected bool PuedeEditarEstado()
    //{
    //    // Si está facturado, no se puede editar
    //    if (EstaFacturado) return false;

    //    // Si tiene cargo en Cta Cte y está Aprobado, bloqueá (o ajustá tu regla)
    //    if (TieneCargoCtaCte && Presupuesto.Estado == EstadoPresupuesto.Aprobado) return false;

    //    return true;
    //}

    protected bool PuedeEditarEstado()
    {
        if (EsNuevo) return true;

        if (Presupuesto.Estado is EstadoPresupuesto.FacturadoParcial or EstadoPresupuesto.FacturadoTotal or EstadoPresupuesto.Asignado)
            return false;

        if (TieneCargoCtaCte) //  candado por cargo en Cta Cte
            return false;

        return true;
    }


    protected string MensajeBloqueoEstado()
    {
        if (EstaFacturado) return "El estado lo define la facturación (no editable).";
        if (EstaAsignado) return "El presupuesto esta asigando a una gestión (no editable)";
        if (TieneCargoCtaCte && Presupuesto.Estado == EstadoPresupuesto.Aprobado)
            return "Tiene cargo en cuenta corriente: primero retire el cargo para cambiar el estado.";
        return "";
    }









    protected void CalcularTotales()
    {
        Presupuesto.Neto = Detalles.Sum(d => d.Subtotal);

        // Sumar IVA de cada ítem según su porcentaje seleccionado
        Presupuesto.Iva = Detalles.Sum(d =>
        {
            var alicuota = IvaAlicuotas.FirstOrDefault(a => a.Id == d.IvaAlicuotaId)?.Porcentaje ?? 0;
            return Math.Round(d.Subtotal * (alicuota / 100), 2);
        });

        Presupuesto.Total = Presupuesto.Neto + Presupuesto.Iva;
    }


    //protected async Task Guardar()
    //{
    //    Presupuesto.Detalles = Detalles;

    //    if (Id.HasValue)
    //        await PresupuestoService.ActualizarAsync(Id.Value, Presupuesto);
    //    else
    //        await PresupuestoService.CrearAsync(Presupuesto);

    //    Nav.NavigateTo("/presupuestos");
    //}

    protected async Task Guardar()
    {
        var estadoPrevio = EstadoAntes;


        // Asegurarnos de pasar el IvaAlicuotaId en los detalles antes de guardar
        Presupuesto.Detalles = Detalles.Select(d => new PresupuestoDetalleDto
        {
            Id = d.Id,
            Descripcion = d.Descripcion,
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario,
            IvaAlicuotaId = d.IvaAlicuotaId,
            AjusteVariableId = d.AjusteVariableId,
            VariableCoef = d.VariableCoef,
            VariableValorUsado = d.VariableValorUsado
        }).ToList();

        if (Id.HasValue)
        {
            await PresupuestoService.ActualizarAsync(Id.Value, Presupuesto);
            Presupuesto.Id = Id.Value; // <-- asegurar
        }
        else
        {
            //await PresupuestoService.CrearAsync(Presupuesto);
            // que el servicio DEVUELVA el creado con Id
            var creado = await PresupuestoService.CrearAsync(Presupuesto);
            if (creado != null) Presupuesto.Id = creado.Id;
        }

        // si pasó a APROBADO y el toggle está marcado


        // Crear cargo
        //if (estadoPrevio != EstadoPresupuesto.Aprobado
        //    && Presupuesto.Estado == EstadoPresupuesto.Aprobado
        //    && CargarCtaCteAlAprobar
        //    && !CargoPresupuestoYaCargado)
        //{
        //    var res = await CuentaSrv.CrearCargoPresupuestoAsync(
        //        Presupuesto.PersonaId,
        //        Presupuesto.Id,
        //        Presupuesto.Total
        //    );
        //    // si tu método devuelve bool/enum, usalo; si no, consultá de nuevo:
        //    CargoPresupuestoYaCargado = true;
        //}

        //// Retirar cargo
        //if (Presupuesto.Estado == EstadoPresupuesto.Aprobado
        //    && RetirarDeCtaCteAlDesaprobar
        //    && CargoPresupuestoYaCargado)
        //{
        //    try
        //    {
        //        await CuentaSrv.RevertirCargoPresupuestoAsync(Presupuesto.PersonaId, Presupuesto.Id);
        //        CargoPresupuestoYaCargado = false;
        //        ShowAlert("Se retiró el cargo del presupuesto de la Cuenta Corriente.", "success");
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        ShowAlert(ex.Message, "warning"); // p.ej. tiene imputaciones…
        //   }



        //}


        // Si el presupuesto YA está en Cta Cte, ajustá por delta:
        if (CargoPresupuestoYaCargado)
        {
            try
            {
                await CuentaSrv.AjustarPresupuestoAsync(Presupuesto.PersonaId, Presupuesto.Id, Presupuesto.Total);
                await Toast("Cuenta Corriente sincronizada con el nuevo total.", "success");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                await Toast("No se puede bajar por debajo de lo ya imputado.", "warning");
            }
        }

        if (!string.IsNullOrWhiteSpace(ReturnUrl))
            Nav.NavigateTo(ReturnUrl!);
        else
            Nav.NavigateTo("/presupuestos");

    }





    protected async Task GuardarSinSalir()
    {
        var estadoPrevio = EstadoAntes;


        // Asegurarnos de pasar el IvaAlicuotaId en los detalles antes de guardar
        Presupuesto.Detalles = Detalles.Select(d => new PresupuestoDetalleDto
        {
            Id = d.Id,
            Descripcion = d.Descripcion,
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario,
            IvaAlicuotaId = d.IvaAlicuotaId,
            AjusteVariableId = d.AjusteVariableId,
            VariableCoef = d.VariableCoef,
            VariableValorUsado = d.VariableValorUsado
        }).ToList();

        if (Id.HasValue)
        {
            await PresupuestoService.ActualizarAsync(Id.Value, Presupuesto);
            Presupuesto.Id = Id.Value; // <-- asegurar
        }
        else
        {
            //await PresupuestoService.CrearAsync(Presupuesto);
            // que el servicio DEVUELVA el creado con Id
            var creado = await PresupuestoService.CrearAsync(Presupuesto);
            if (creado != null) Presupuesto.Id = creado.Id;
        }



        // Si el presupuesto YA está en Cta Cte, ajustá por delta:
        if (CargoPresupuestoYaCargado)
        {
            try
            {
                await CuentaSrv.AjustarPresupuestoAsync(Presupuesto.PersonaId, Presupuesto.Id, Presupuesto.Total);
                await Toast("Cuenta Corriente sincronizada con el nuevo total.", "success");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                await Toast("No se puede bajar por debajo de lo ya imputado.", "warning");
            }
        }

        //if (!string.IsNullOrWhiteSpace(ReturnUrl))
        //    Nav.NavigateTo(ReturnUrl!);
        //else
        //    Nav.NavigateTo("/presupuestos");

    }



    protected Task Toast(string msg, string type = "info")
    => JS.InvokeVoidAsync("mostrarToast", msg, type).AsTask();



    protected async Task OnEstadoChangedAsync(EstadoPresupuesto nuevo)
    {
        if (EsNuevo || !PuedeEditarEstado()) return;

        var anterior = Presupuesto.Estado;
        try
        {
            Presupuesto.Estado = nuevo;
            await PresupuestoService.ActualizarEstadoAsync(Presupuesto.Id, nuevo);
            selectKey++;

            if (nuevo == EstadoPresupuesto.Aprobado)
            {
                TieneCargoCtaCte = await CuentaSrv.ExisteCargoPresupuestoAsync(Presupuesto.PersonaId, Presupuesto.Id);
                CargoPresupuestoYaCargado = TieneCargoCtaCte;
            }

            await Toast("Estado guardado.", "success");
        }
        catch (HttpRequestException ex)
        {
            Presupuesto.Estado = anterior;
            selectKey++;
            await Toast($"No se pudo guardar el estado. {ex.Message}", "warning");
        }
    }




    protected async Task AgregarACtaCte()
    {
        BusyCtaCte = true;
        try
        {
            await CuentaSrv.CrearCargoPresupuestoAsync(Presupuesto.PersonaId, Presupuesto.Id, Presupuesto.Total);

            // refrescamos el flag consultando al backend
            TieneCargoCtaCte = await CuentaSrv.ExisteCargoPresupuestoAsync(
                Presupuesto.PersonaId, Presupuesto.Id);

            CargoPresupuestoYaCargado = true;

            //  forzá re-render del select
            selectKey++;
            
            await Toast("Se agregó el cargo del presupuesto a la Cuenta Corriente.", "success");
        }
        catch (HttpRequestException ex)
        {
            await Toast($"No se pudo agregar a Cta Cte. {ex.Message}", "warning");
        }
        finally { BusyCtaCte = false; StateHasChanged(); }
    }


    //Sacamos la opcion de reversar aca

    //protected async Task RetirarDeCtaCte()
    //{
    //    BusyCtaCte = true;
    //    try
    //    {
    //        await CuentaSrv.RevertirCargoPresupuestoAsync(Presupuesto.PersonaId, Presupuesto.Id);
    //        CargoPresupuestoYaCargado = false;
    //        await Toast("Se retiró el cargo del presupuesto de la Cuenta Corriente.", "success");
    //    }

    //    catch (InvalidOperationException ex)
    //    {
    //        // <- acá caen los 409 que tu service convierte en InvalidOperationException
    //        var confirmar = await JS.InvokeAsync<bool>(
    //            "confirm",
    //            "El cargo tiene imputaciones. ¿Querés reversarlo? Esto creará un crédito por el saldo pendiente."
    //        );
    //        if (confirmar)
    //        {
    //            await CuentaSrv.ReversarCargoPresupuestoAsync(Presupuesto.PersonaId, Presupuesto.Id);
    //            await Toast("Se reversó el cargo (neta en 0).", "info");
    //            // OJO: el débito original sigue existiendo -> CargoPresupuestoYaCargado queda TRUE
    //        }
    //    }
    //    catch (HttpRequestException ex)
    //    {
    //        await Toast($"No se pudo retirar de Cta Cte. {ex.Message}", "error");
    //    }
    //    finally
    //    {
    //        BusyCtaCte = false;
    //        StateHasChanged();
    //    }
    //    //catch (InvalidOperationException ex)
    //    //{
    //    //    // p.ej.: “no se puede retirar, tiene imputaciones”
    //    //    await Toast(ex.Message, "warning");
    //    //}
    //    //catch (HttpRequestException ex)
    //    //{
    //    //    await Toast($"No se pudo retirar de Cta Cte. {ex.Message}", "warning");
    //    //}
    //    //finally { BusyCtaCte = false; StateHasChanged(); }
    //}



    //Se puede retirar el presupuesto de la cta cte si no tiene imputaciones
    protected async Task RetirarDeCtaCte()
    {
        BusyCtaCte = true;
        try
        {
            await CuentaSrv.RevertirCargoPresupuestoAsync(Presupuesto.PersonaId, Presupuesto.Id);
            
            // 2) Refrescar flags desde backend (no uses sólo toggles locales)
            TieneCargoCtaCte = await CuentaSrv.ExisteCargoPresupuestoAsync(Presupuesto.PersonaId, Presupuesto.Id);
            CargoPresupuestoYaCargado = false;

            // 3) Forzar re-render del select (reconstruye el componente)
            selectKey++;
            
            await Toast("Se retiró el cargo del presupuesto de la Cuenta Corriente.", "success");
        }
        catch (InvalidOperationException ex)
        {
            await Toast("No se puede retirar: el presupuesto tiene imputaciones. Debe generar un nuevo presupuesto.", "warning");
        }
        catch (HttpRequestException ex)
        {
            await Toast($"No se pudo retirar de Cta Cte. {ex.Message}", "error");
        }
        finally
        {
            BusyCtaCte = false;
            StateHasChanged();
        }
    }




    //protected async Task Revalorizar()
    //{
    //    if (!Id.HasValue) return;
    //    var ok = await JS.InvokeAsync<bool>("confirm", "¿Actualizar precios al valor vigente?");
    //    if (!ok) return;

    //    await PresupuestoService.RevalorizarAsync(Id.Value, aplicar: true);

    //    // Recargar para ver los nuevos importes
    //    Presupuesto = await PresupuestoService.ObtenerPorIdAsync(Id.Value) ?? new();
    //    Detalles = await PresupuestoService.ObtenerDetallesAsync(Id.Value);
    //    CalcularTotales();
    //    StateHasChanged();
    //}



    protected async Task Revalorizar()
    {
        if (!Id.HasValue) return;

        // 1) PREVIEW
        var preview = await PresupuestoService.PrevisualizarRevalorizarAsync(Id.Value);
        var cambios = preview?.cambios ?? new();

        if (cambios.Count == 0)
        {
            await Toast("No hay ítems con variable de ajuste para revalorizar.", "info");
            return;
        }

        // Mostramos un resumen simple
        var resumen = string.Join(Environment.NewLine,
            cambios.Select(c => $"{c.Descripcion}: {c.Antes:C} → {c.Despues:C} ({c.Variable}={c.ValorActual})"));

        var confirmar = await JS.InvokeAsync<bool>(
            "confirm",
            $"Se actualizarán estos ítems:\n\n{resumen}\n\n¿Aplicar cambios?");
        if (!confirmar) return;

        // 2) APLICAR
        await PresupuestoService.RevalorizarAsync(Id.Value, aplicar: true);



        //await GuardarSinSalir();

        // 3) Refrescar datos en pantalla
        Presupuesto = await PresupuestoService.ObtenerPorIdAsync(Id.Value) ?? new();
        Detalles = await PresupuestoService.ObtenerDetallesAsync(Id.Value);
        CalcularTotales();
        StateHasChanged();



        await Toast("Precios actualizados según variables vigentes.", "success");



    }




    protected async Task OnSeleccionVariableGlobal(ChangeEventArgs _)
    {
        if (AjusteVariableGlobalId is null || AjusteVariableGlobalId <= 0) return;

        var varSel = Variables.FirstOrDefault(v => v.Id == AjusteVariableGlobalId.Value);
        if (varSel is null || varSel.ValorActual <= 0m)
        {
            await Toast("Variable inválida o sin valor actual.", "warning");
            return;
        }

        foreach (var d in Detalles)
        {
            // NO cambiamos precio ahora: solo “marcamos” el renglón con la variable
            d.AjusteVariableId = varSel.Id;
            d.VariableValorUsado = varSel.ValorActual;
            // coeficiente guardado con 6 decimales por estabilidad
            d.VariableCoef = Math.Round(
                varSel.ValorActual == 0 ? 0 : d.PrecioUnitario / varSel.ValorActual,
                6, MidpointRounding.AwayFromZero
            );
        }

        await Toast("Variable asignada a todos los ítems. Guardá el presupuesto para conservarla.", "info");
    }






    protected void Cancelar()
    {

        if (!string.IsNullOrWhiteSpace(ReturnUrl))
            Nav.NavigateTo(ReturnUrl!);
        else
            Nav.NavigateTo("/presupuestos");

    }




    //protected void OnDetalleChanged(PresupuestoDetalleDto detalle, decimal valor, string campo)
    //{
    //    if (campo == nameof(detalle.Cantidad))
    //        detalle.Cantidad = valor;
    //    else if (campo == nameof(detalle.PrecioUnitario))
    //        detalle.PrecioUnitario = valor;

    //    CalcularTotales();
    //}

    //protected void OnDetalleChanged(PresupuestoDetalleDto detalle, object valor, string campo)
    //{
    //    if (campo == nameof(detalle.Cantidad))
    //        detalle.Cantidad = Convert.ToDecimal(valor);
    //    else if (campo == nameof(detalle.PrecioUnitario))
    //        detalle.PrecioUnitario = Convert.ToDecimal(valor);
    //    else if (campo == nameof(detalle.IvaAlicuotaId))
    //        detalle.IvaAlicuotaId = Convert.ToInt32(valor); // ✅ Nuevo

    //    CalcularTotales();
    //}

    //protected void OnDetalleChanged(PresupuestoDetalleDto detalle, int valor, string campo)
    //{
    //    if (campo == nameof(detalle.IvaAlicuotaId))
    //        detalle.IvaAlicuotaId = valor;

    //    CalcularTotales();
    //}

    //protected void OnDetalleChanged(PresupuestoDetalleDto detalle, decimal valor, string campo)
    //{
    //    if (campo == nameof(detalle.Cantidad))
    //        detalle.Cantidad = valor;
    //    else if (campo == nameof(detalle.PrecioUnitario))
    //        detalle.PrecioUnitario = valor;

    //    CalcularTotales();
    //}


    protected void OnDetalleChanged(PresupuestoDetalleDto detalle, int valor, string campo)
    {
        if (EstaBloqueadoDetalles()) return;

        if (campo == nameof(PresupuestoDetalleDto.IvaAlicuotaId))
            detalle.IvaAlicuotaId = valor;

        CalcularTotales();
    }

    protected void OnDetalleChanged(PresupuestoDetalleDto detalle, decimal valor, string campo)
    {
        if (EstaBloqueadoDetalles()) return;

        if (campo == nameof(PresupuestoDetalleDto.Cantidad))
            detalle.Cantidad = valor;
        else if (campo == nameof(PresupuestoDetalleDto.PrecioUnitario))
            detalle.PrecioUnitario = valor;

        CalcularTotales();
    }

    // para Descripción
    protected void OnDetalleDescripcionChanged(PresupuestoDetalleDto det, string? valor)
    {
        if (EstaBloqueadoDetalles()) return;
        det.Descripcion = valor ?? string.Empty;
        // no recalcular totales
    }



}
