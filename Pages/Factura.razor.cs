using IurixBlazor.Pages;
using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using System.Text.Json;

public class FacturasBase : ComponentBase
{
    [Inject] protected FacturaService FacturaService { get; set; } = default!;
    [Inject] protected PersonaService PersonaService { get; set; } = default!;
    [Inject] protected UsuarioService UsuarioService { get; set; } = default!;
    [Inject] protected PuntoVentaService PuntoVentaService { get; set; } = default!;

    [Inject] public IvaAlicuotaService IvaAlicuotaService { get; set; } = default!;
    [Inject] protected AfipAuthService AfipAuthService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    [Inject] protected AfipFacturaService AfipFacturaService { get; set; } = default!;
        
    [Inject] protected ComprobanteAfipService ComprobanteAfipService { get; set; } = default!;

    [Inject] protected PresupuestoService PresupuestoService { get; set; } = default!;


    protected FacturaDto Factura { get; set; } = new();
    protected List<FacturaDetalleDto> Detalles { get; set; } = new();

    protected List<PuntoVentaDto> PuntosVenta { get; set; } = new();
    protected List<IvaAlicuotaDto> IvaAlicuotas { get; set; } = new();
    protected List<PersonaDto> ListaClientes { get; set; } = new();


    // Datos Emisor
    protected string UsuarioEmisorNombre { get; set; } = "";
    protected string UsuarioEmisorCuit { get; set; } = "";
    protected string UsuarioEmisorCondicion { get; set; } = "";
    protected int UsuarioEmisorCondicionCodigo { get; set; }
    protected int UsuarioId { get; set; }


    protected List<UsuarioDto> Usuarios = new();
    protected UsuarioDto? UsuarioSeleccionado;
    protected bool UsuarioTieneCertificados = true;


    // Datos Cliente Seleccionado
    protected string ClienteSeleccionadoNombre { get; set; } = "";
    protected string ClienteSeleccionadoApellido { get; set; } = "";
    protected string ClienteSeleccionadoDoc { get; set; } = "";
    protected string ClienteSeleccionadoDocLabel { get; set; } = "";
    protected string ClienteSeleccionadoCondicion { get; set; } = "";
    protected bool ClienteEsPersonaFisica { get; set; } = false;
    protected bool MostrarModalClientes { get; set; } = false;

    //ver si reemplazamos con tipo comprobante
    protected string OperacionSeleccionada { get; set; } = "Factura";


    //Muestra confirmación en Punto Venta 0
    protected bool MostrarConfirmacionPtoVenta0 { get; set; } = false;


    protected string MensajeExito { get; set; } = "";
    protected bool FacturaCreadaExito { get; set; } = false;

    protected string letraComprobante { get; set; } = "";


    protected bool EsFacturaA => Factura.TipoComprobanteLetra == "A";
    protected bool EsFacturaB => Factura.TipoComprobanteLetra == "B";
    protected bool EsFacturaC => Factura.TipoComprobanteLetra == "C";


    protected bool MostrarModalComprobanteAsociado { get; set; }
    protected List<FacturaDto> ComprobantesAsociables { get; set; } = new();
    protected FacturaDto? ComprobanteAsociadoSeleccionado { get; set; }

    protected bool EsNotaCredito => OperacionSeleccionada == "NotaCredito";
    protected bool EsNotaDebito => OperacionSeleccionada == "NotaDebito";
    protected bool RequiereComprobanteAsociado => EsNotaCredito || EsNotaDebito;




    protected bool MostrarModalPresupuestos { get; set; }
    protected List<PresupuestoResumenDto> PresupuestosCliente { get; set; } = new();


    const decimal EPS = 0.01m;

    //protected decimal GetTasa(int ivaAlicuotaId)
    //{
    //    var ali = IvaAlicuotas.FirstOrDefault(a => a.Id == ivaAlicuotaId);
    //    return ali?.Porcentaje ?? 0m; // 21, 10.5, 0, etc.
    //}

    protected decimal GetTasa(int ivaAlicuotaId)
    {
        var alic = IvaAlicuotas.First(a => a.Id == ivaAlicuotaId);
        return alic.Porcentaje; // 21, 10.5, 0, etc. (NO uses d.IvaPorcentaje si viene 0)
    }



    protected static string SoloNumeros(string? s) => new string((s ?? "").Where(char.IsDigit).ToArray());




    protected bool MostrarModalFiscales { get; set; }
    

    protected ManualFiscalesDto DatosManuales { get; set; } = new();

    private bool _cargarDatosFiscalesManuales;
    protected bool CargarDatosFiscalesManuales
    {
        get => _cargarDatosFiscalesManuales;
        set
        {
            _cargarDatosFiscalesManuales = value;
            OnToggleDatosManuales(value);
        }
    }




    protected override async Task OnInitializedAsync()
    {
        PuntosVenta = await PuntoVentaService.ObtenerTodosAsync();

        IvaAlicuotas = await IvaAlicuotaService.ObtenerTodasAsync();
        //  Setear alícuota por defecto (21%) al iniciar
        foreach (var item in Detalles)
        {
            var ivaDefault = IvaAlicuotas.FirstOrDefault(i => i.Porcentaje == 21);
            if (ivaDefault != null)
                item.IvaAlicuotaId = ivaDefault.Id;
        }

        var usuarioIdStr = await JS.InvokeAsync<string>("localStorage.getItem", "usuario_id");

        if (string.IsNullOrWhiteSpace(usuarioIdStr) || !int.TryParse(usuarioIdStr, out var usuarioId))
        {
            // Manejar el error (por ejemplo, redirigir al login o mostrar mensaje)
            System.Diagnostics.Debug.WriteLine("⚠️ ID de usuario no válido en localStorage");
            return;
        }

        var usuario = await UsuarioService.ObtenerUsuarioPorIdAsync(usuarioId);
        if (usuario == null)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Usuario con ID {usuarioId} no encontrado");
            return;
        }

        UsuarioId = usuario.Id;
        UsuarioEmisorNombre = usuario.Nombre ?? string.Empty;
        UsuarioEmisorCuit = usuario.CUIT ?? string.Empty;
        UsuarioEmisorCondicion = usuario.CondicionIvaNombre ?? string.Empty;
        UsuarioEmisorCondicionCodigo = usuario.CondicionIvaCodigoAfip;
        //  Consultar certificados en AFIPAuth
        var afipAuth = await AfipAuthService.ObtenerPorUsuarioId(usuario.Id);

        UsuarioTieneCertificados = afipAuth != null &&
                                   !string.IsNullOrEmpty(afipAuth.RSA_PKEY_PROD) &&
                                   !string.IsNullOrEmpty(afipAuth.CERT_X509_PROD);


        // Cargar todos los usuarios
        Usuarios = await UsuarioService.ObtenerUsuariosAsync();

        //// Inicializar factura
        //Factura = new FacturaDto
        //{
        //    Fecha = DateTime.Now,
        //    MonId = "PES",
        //    MonCotiz = 1m,
        //    Concepto = ConceptoFactura.Servicio // Valor por defecto
        //};

        PuntosVenta = await PuntoVentaService.ObtenerTodosAsync();


        //  arranque limpio unificado
            InitFactura();
    }

    //protected override async Task OnInitializedAsync()
    //{
    //    PuntosVenta = await PuntoVentaService.ObtenerTodosAsync();
    //    IvaAlicuotas = await IvaAlicuotaService.ObtenerTodasAsync();

    //    var usuarioIdStr = await JS.InvokeAsync<string>("localStorage.getItem", "usuario_id");
    //    if (string.IsNullOrWhiteSpace(usuarioIdStr) || !int.TryParse(usuarioIdStr, out var usuarioId))
    //    {
    //        System.Diagnostics.Debug.WriteLine("⚠️ ID de usuario no válido en localStorage");
    //        return;
    //    }

    //    var usuario = await UsuarioService.ObtenerUsuarioPorIdAsync(usuarioId);
    //    if (usuario == null)
    //    {
    //        System.Diagnostics.Debug.WriteLine($"⚠️ Usuario con ID {usuarioId} no encontrado");
    //        return;
    //    }

    //    await CargarDatosEmisor(usuario);
    //    Usuarios = await UsuarioService.ObtenerUsuariosAsync();

    //    // ✅ arranque limpio unificado
    //    InitFactura();
    //}




    //METODO PARA ABRIR MODAL NOTA CREDITO O DEBITO - FACTURA ASOCIADA 

    //protected async Task AbrirModalComprobanteAsociado()
    //{
    //    if (Factura.PersonaId <= 0)
    //    {
    //        await Toast("Primero seleccioná un cliente.", "warning");
    //        return;
    //    }

    //    ComprobantesAsociables =
    //        await FacturaService.ObtenerComprobantesAsociablesPorPersonaAsync(
    //            Factura.PersonaId,
    //            OperacionSeleccionada);

    //    if (!ComprobantesAsociables.Any())
    //    {
    //        await Toast("No se encontraron facturas asociables para este cliente.", "warning");
    //        return;
    //    }

    //    MostrarModalComprobanteAsociado = true;
    //}

    protected async Task AbrirModalComprobanteAsociado()
    {
        if (Factura.PersonaId <= 0)
        {
            await Toast("Primero seleccioná un cliente.", "warning");
            return;
        }

        ComprobantesAsociables =
            await FacturaService.ObtenerComprobantesAsociablesPorPersonaAsync(
                Factura.PersonaId,
                OperacionSeleccionada);

        await JS.InvokeVoidAsync(
            "console.log",
            $"Comprobantes asociados encontrados: {ComprobantesAsociables.Count}");

        MostrarModalComprobanteAsociado = true;
        await InvokeAsync(StateHasChanged);
    }



    protected void SeleccionarComprobanteAsociado(FacturaDto comprobante)
    {
        ComprobanteAsociadoSeleccionado = comprobante;

        Factura.CbteAsocTipo = comprobante.CbteTipo;
        Factura.CbteAsocPtoVta = comprobante.PuntoVentaNumero;
        Factura.CbteAsocNro = comprobante.Numero;
        Factura.CbteAsocCuit = UsuarioEmisorCuit;
        Factura.CbteAsocFecha = comprobante.Fecha;

        if (EsNotaCredito)
        {
            Detalles.Clear();

            foreach (var d in comprobante.Detalles)
            {
                Detalles.Add(new FacturaDetalleDto
                {
                    Descripcion = d.Descripcion,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    IvaAlicuotaId = d.IvaAlicuotaId
                });
            }

            CalcularTotales();
        }

        if (EsNotaDebito)
        {
            Detalles.Clear();

            Detalles.Add(new FacturaDetalleDto
            {
                Descripcion = $"Ajuste / diferencia sobre comprobante {comprobante.PuntoVentaNumero:0000}-{comprobante.Numero:00000000}",
                Cantidad = 1,
                PrecioUnitario = 0,
                IvaAlicuotaId = IvaAlicuotas.FirstOrDefault(i => i.Porcentaje == 21)?.Id
                                ?? IvaAlicuotas.FirstOrDefault()?.Id
                                ?? 0
            });

            CalcularTotales();
        }

        MostrarModalComprobanteAsociado = false;
        StateHasChanged();
    }


    private void InitFactura()
    {
        // Estado base de una factura nueva
        Factura = new FacturaDto
        {
            Fecha = DateTime.Now,
            MonId = "PES",
            MonCotiz = 1m,
            Concepto = ConceptoFactura.Servicio
        };

        // Lista que usa la UI
        Detalles = new List<FacturaDetalleDto>();

        // (opcional) Agregá un renglón en blanco con IVA 21%
        AgregarItem();

        // Limpiar totales/auxiliares UI
        letraComprobante = null!;
        Factura.Neto = 0;
        Factura.Iva = 0;
        Factura.Total = 0;
        Factura.IvaDiscriminado = new();
        Factura.IvaDiscriminadoAfip = new();
        Factura.FormaVenta = FormaVenta.CuentaCorriente;
    }





    private void ResetFactura(bool mantenerCliente = false, bool mantenerPtoVta = true)
    {
        // Guardá lo que quieras preservar
        var personaId = mantenerCliente ? Factura.PersonaId : 0;
        var docTipo = mantenerCliente ? Factura.DocTipo : 0;
        var docNro = mantenerCliente ? Factura.DocNro : null;
        var condReceptorId = mantenerCliente ? Factura.CondicionIVAReceptorId : 0;
        var condReceptorAfip = mantenerCliente ? Factura.CondicionIvaReceptorCodigoAfip : (int?)null;

        var ptoVtaId = mantenerPtoVta ? Factura.PuntoVentaId : 0;
        var concepto = Factura.Concepto;
        var monId = Factura.MonId;
        var monCotiz = Factura.MonCotiz;

        // Reinicio completo
        InitFactura();

        // Restauro seleccionados
        Factura.PersonaId = personaId;
        Factura.DocTipo = docTipo;
        Factura.DocNro = docNro;
        Factura.CondicionIVAReceptorId = condReceptorId;
        Factura.CondicionIvaReceptorCodigoAfip = condReceptorAfip;

        Factura.PuntoVentaId = ptoVtaId;
        Factura.Concepto = concepto;
        Factura.MonId = monId;
        Factura.MonCotiz = monCotiz;

        // Campos “de salida AFIP” se limpian sí o sí
        Factura.CAE = null!;
        Factura.CAEFchVto = null;
        Factura.Numero = 0;

        // UI del cliente
        // Limpiar datos de cliente si no los mantenemos
        if (!mantenerCliente)
        {
            ClienteSeleccionadoNombre = "";
            ClienteSeleccionadoApellido = "";
            ClienteSeleccionadoDoc = "";
            ClienteSeleccionadoDocLabel = "";
            ClienteSeleccionadoCondicion = "";
            ClienteEsPersonaFisica = false;
        }

        StateHasChanged();
    }





    protected Task Toast(string msg, string type = "info")
    => JS.InvokeVoidAsync("mostrarToast", msg, type).AsTask();


    //protected async Task OnUsuarioEmisorChanged(ChangeEventArgs e)
    //{
    //    var nuevoUsuarioId = int.Parse(e.Value!.ToString()!);
    //    UsuarioSeleccionado = Usuarios.FirstOrDefault(u => u.Id == nuevoUsuarioId);

    //    if (UsuarioSeleccionado != null)
    //        await CargarDatosEmisor(UsuarioSeleccionado);


    //}

    //protected async Task OnUsuarioEmisorChanged(ChangeEventArgs e)
    //{
    //    var nuevoUsuarioId = int.Parse(e.Value!.ToString()!);
    //    UsuarioSeleccionado = Usuarios.FirstOrDefault(u => u.Id == nuevoUsuarioId);

    //    if (UsuarioSeleccionado != null)
    //    {
    //        await CargarDatosEmisor(UsuarioSeleccionado);

    //        // ✅ Si ya hay cliente seleccionado, recalcular comprobante
    //        if (Factura.PersonaId > 0) // Ya hay cliente cargado
    //        {
    //            var codReceptor = Factura.CondicionIVAReceptorId;
    //            bool esNotaCredito = OperacionSeleccionada == "NotaCredito";
    //            bool esNotaDebito = OperacionSeleccionada == "NotaDebito";

    //            var (cbteTipo, descripcion, letra) = await ComprobanteAfipService.DeterminarComprobanteAsync(
    //                UsuarioEmisorCondicionCodigo, // Nueva condición IVA emisor
    //                codReceptor,
    //                esNotaCredito,
    //                esNotaDebito
    //            );

    //            Factura.CbteTipo = cbteTipo;
    //            Factura.TipoComprobanteId = cbteTipo;
    //            Factura.TipoComprobanteDescripcion = descripcion;
    //            Factura.TipoComprobanteLetra = letra;

    //            letraComprobante = letra;
    //        }
    //    }
    //}

    protected async Task OnUsuarioEmisorChanged(ChangeEventArgs e)
    {
        var nuevoUsuarioId = int.Parse(e.Value!.ToString()!);
        UsuarioSeleccionado = Usuarios.FirstOrDefault(u => u.Id == nuevoUsuarioId);

        if (UsuarioSeleccionado != null)
        {
            await CargarDatosEmisor(UsuarioSeleccionado); // acá seteás UsuarioEmisorCondicionCodigo
            await RecalcularComprobanteAsync();           //  si ya hay cliente, recalcula
        }
    }


    protected async Task CargarDatosEmisor(UsuarioDto usuario)
    {
        UsuarioId = usuario.Id;
        UsuarioEmisorNombre = usuario.Nombre ?? string.Empty;
        UsuarioEmisorCuit = usuario.CUIT ?? string.Empty;
        UsuarioEmisorCondicion = usuario.CondicionIvaNombre;
        UsuarioEmisorCondicionCodigo = usuario.CondicionIvaCodigoAfip;


        //  Consultar certificados en AFIPAuth
        var afipAuth = await AfipAuthService.ObtenerPorUsuarioId(usuario.Id);

        UsuarioTieneCertificados = afipAuth != null &&
                                   !string.IsNullOrEmpty(afipAuth.RSA_PKEY_PROD) &&
                                   !string.IsNullOrEmpty(afipAuth.CERT_X509_PROD);


    }



    protected async Task BuscarCliente()
    {
        // Obtener listado de personas desde el servicio
        var personas = await PersonaService.ObtenerPersonasAsync();
        System.Diagnostics.Debug.WriteLine(System.Text.Json.JsonSerializer.Serialize(personas));
        if (personas == null || !personas.Any())
        {
            // Mostrar mensaje si no hay clientes
            Console.WriteLine("⚠️ No se encontraron clientes.");
            return;
        }

        // 🔹 Aquí podrías implementar un modal para seleccionar un cliente.
        // Por ahora, seleccionamos el primero como ejemplo (deberías reemplazarlo por UI modal)
        var cliente = personas.First(); // ❗ En producción, reemplazar por un selector modal.

        if (cliente != null)
        {
            //ClienteSeleccionadoNombre = cliente.RazonSocial;
            //ClienteSeleccionadoCuit = cliente.CUIT;

            if (!string.IsNullOrEmpty(cliente.RazonSocial))
            {
                ClienteSeleccionadoNombre = cliente.RazonSocial;
            }
            else
            {
                ClienteSeleccionadoNombre = $"{cliente.Nombre} {cliente.Apellido}".Trim();
            }
            //OTRA OPCION CON OPERADOR TERNARIO
            //    ClienteSeleccionadoNombre =
            //!string.IsNullOrEmpty(cliente.RazonSocial)
            //    ? cliente.RazonSocial
            //    : $"{cliente.Nombre} {cliente.Apellido}".Trim();

            if (!string.IsNullOrEmpty(cliente.CUIT))
            {
                ClienteSeleccionadoDoc = cliente.DNI;
            }
            else
            {
                ClienteSeleccionadoDoc = cliente.CUIT;
            }


            ClienteSeleccionadoCondicion = cliente.CondicionIvaNombre;

            Factura.CondicionIVAReceptorId = cliente.CondicionIVAId ?? 0;
            Factura.PersonaId = cliente.Id;

            // Ajustar tipo comprobante automáticamente según IVA
            Factura.TipoComprobanteDescripcion =
                cliente.CondicionIvaNombre == "Responsable Inscripto" ? "Factura A" : "Factura C";
        }
    }



    protected async Task AbrirModalClientes()
    {
        ListaClientes = await PersonaService.ObtenerPersonasAsync();
        if (ListaClientes != null && ListaClientes.Any())
        {
            MostrarModalClientes = true;
        }
        else
        {
            Console.WriteLine("⚠️ No se encontraron clientes.");
        }
    }

    protected void CerrarModalClientes()
    {
        MostrarModalClientes = false;
    }


    //protected void SeleccionarCliente(PersonaDto cliente)
    //{
    //    // Cliente: Razón Social o Nombre + Apellido
    //    ClienteSeleccionadoNombre =
    //        (!string.IsNullOrWhiteSpace(cliente.RazonSocial) && cliente.CondicionIvaNombre != "Consumidor Final")
    //        ? cliente.RazonSocial
    //        : $"{cliente.Nombre} {cliente.Apellido}";

    //    // Documento: CUIT o DNI según corresponda
    //    if (!string.IsNullOrWhiteSpace(cliente.CUIT) && cliente.CondicionIvaNombre != "Consumidor Final")
    //    {
    //        ClienteSeleccionadoDoc = cliente.CUIT;
    //        ClienteSeleccionadoDocLabel = "CUIT";
    //    }
    //    else
    //    {
    //        ClienteSeleccionadoDoc = cliente.DNI;
    //        ClienteSeleccionadoDocLabel = "DNI";
    //    }

    //    ClienteSeleccionadoCondicion = cliente.CondicionIvaNombre;
    //    Factura.CondicionIVAReceptorId = cliente.CondicionIVAId ?? 0;
    //    Factura.PersonaId = cliente.Id;

    //    // Ajustar tipo de comprobante automáticamente
    //    Factura.TipoComprobanteDescripcion =
    //        cliente.CondicionIvaNombre == "Responsable Inscripto" ? "Factura A" : "Factura C";

    //    MostrarModalClientes = false;
    //}


    //protected async Task SeleccionarCliente(PersonaDto cliente)
    //{
    //    ClienteSeleccionadoNombre =
    //        (!string.IsNullOrWhiteSpace(cliente.RazonSocial) && cliente.CondicionIvaNombre != "Consumidor Final")
    //        ? cliente.RazonSocial
    //        : $"{cliente.Nombre} {cliente.Apellido}";

    //    // Documento y tipo AFIP
    //    if (!string.IsNullOrWhiteSpace(cliente.CUIT))
    //    {
    //        ClienteSeleccionadoDoc = cliente.CUIT;
    //        ClienteSeleccionadoDocLabel = "CUIT";
    //        Factura.DocTipo = 80; // CUIT
    //        Factura.DocNro = cliente.CUIT;
    //    }
    //    else
    //    {
    //        ClienteSeleccionadoDoc = cliente.DNI;
    //        ClienteSeleccionadoDocLabel = "DNI";
    //        Factura.DocTipo = 96; // DNI
    //        Factura.DocNro = cliente.DNI;
    //    }

    //    ClienteSeleccionadoCondicion = cliente.CondicionIvaNombre;
    //    Factura.CondicionIVAReceptorId = cliente.CondicionIVAId ?? 0;
    //    Factura.CondicionIvaReceptorCodigoAfip = cliente.CondicionIvaCodigoAfip; //pasamos id afip
    //    Factura.PersonaId = cliente.Id;

    //    // ✅ Determinar tipo comprobante AFIP cruzando condiciones IVA

    //    var codReceptor = cliente.CondicionIvaCodigoAfip ?? 0;
    //    bool esNotaCredito = OperacionSeleccionada == "NotaCredito";
    //    bool esNotaDebito = OperacionSeleccionada == "NotaDebito";

    //    //var (cbteTipo, descripcion) = ComprobanteAfipService.DeterminarComprobante(
    //    //    UsuarioEmisorCondicionCodigo,
    //    //    codReceptor,
    //    //    esNotaCredito,
    //    //    esNotaDebito
    //    //);
    //    var (cbteTipo, descripcion, letra) = await ComprobanteAfipService.DeterminarComprobanteAsync(
    //        UsuarioEmisorCondicionCodigo,
    //        codReceptor,
    //        esNotaCredito,
    //        esNotaDebito
    //    );

    //    //Factura.CbteTipo = cbteTipo;
    //    //Factura.TipoComprobanteDescripcion = descripcion;

    //    Factura.CbteTipo = cbteTipo;
    //    Factura.TipoComprobanteId = cbteTipo;
    //    Factura.TipoComprobanteDescripcion = descripcion;
    //    Factura.TipoComprobanteLetra = letra;

    //    // ✅ Guardamos la letra en una variable global
    //    letraComprobante = letra;

    //    MostrarModalClientes = false;
    //}


    //protected async Task SeleccionarCliente(PersonaDto cliente)
    //{
    //    ClienteSeleccionadoNombre =
    //        (!string.IsNullOrWhiteSpace(cliente.RazonSocial) && cliente.CondicionIvaNombre != "Consumidor Final")
    //        ? cliente.RazonSocial
    //        : $"{cliente.Nombre} {cliente.Apellido}";

    //    if (!string.IsNullOrWhiteSpace(cliente.CUIT))
    //    {
    //        ClienteSeleccionadoDoc = cliente.CUIT;
    //        ClienteSeleccionadoDocLabel = "CUIT";
    //        Factura.DocTipo = 80; // CUIT
    //        Factura.DocNro = cliente.CUIT;
    //    }
    //    else
    //    {
    //        ClienteSeleccionadoDoc = cliente.DNI;
    //        ClienteSeleccionadoDocLabel = "DNI";
    //        Factura.DocTipo = 96; // DNI
    //        Factura.DocNro = cliente.DNI;
    //    }

    //    Factura.PersonaId = cliente.Id;
    //    Factura.CondicionIVAReceptorId = cliente.CondicionIVAId ?? 0;
    //    Factura.CondicionIvaReceptorCodigoAfip = cliente.CondicionIvaCodigoAfip; // ✅

    //    await RecalcularComprobanteAsync(); // ✅
    //    MostrarModalClientes = false;
    //}

    //protected async Task SeleccionarCliente(PersonaDto cliente)
    //{
    //    ClienteSeleccionadoNombre =
    //        (!string.IsNullOrWhiteSpace(cliente.RazonSocial) && cliente.CondicionIvaNombre != "Consumidor Final")
    //        ? cliente.RazonSocial
    //        : $"{cliente.Nombre} {cliente.Apellido}";

    //    var cuitNum = SoloNumeros(cliente.CUIT);
    //    var dniNum = SoloNumeros(cliente.DNI);

    //    if (!string.IsNullOrWhiteSpace(cuitNum))
    //    {
    //        ClienteSeleccionadoDoc = cuitNum;
    //        ClienteSeleccionadoDocLabel = "CUIT";
    //        Factura.DocTipo = 80;         // CUIT
    //        Factura.DocNro = cuitNum;    // sin guiones/puntos
    //    }
    //    else if (!string.IsNullOrWhiteSpace(dniNum))
    //    {
    //        ClienteSeleccionadoDoc = dniNum;
    //        ClienteSeleccionadoDocLabel = "DNI";
    //        Factura.DocTipo = 96;         // DNI
    //        Factura.DocNro = dniNum;     // sin puntos
    //    }
    //    //else
    //    //{
    //    //    // Consumidor final sin DNI
    //    //    ClienteSeleccionadoDoc = "0";
    //    //    ClienteSeleccionadoDocLabel = "S/D";
    //    //    Factura.DocTipo = 99;         // CF
    //    //    Factura.DocNro = "0";
    //    //}

    //    Factura.PersonaId = cliente.Id;
    //    Factura.CondicionIVAReceptorId = cliente.CondicionIVAId ?? 0;
    //    Factura.CondicionIvaReceptorCodigoAfip = cliente.CondicionIvaCodigoAfip;

    //    await RecalcularComprobanteAsync();
    //    MostrarModalClientes = false;
    //}

    protected async Task SeleccionarCliente(PersonaDto cliente)
    {
        // Heurística simple: si NO hay Razón Social o es Consumidor Final => física
        ClienteEsPersonaFisica = string.IsNullOrWhiteSpace(cliente.RazonSocial)
                                 || cliente.CondicionIvaNombre == "Consumidor Final";

        if (ClienteEsPersonaFisica)
        {
            // Guardá por separado
            ClienteSeleccionadoNombre = (cliente.Nombre ?? "").Trim();
            ClienteSeleccionadoApellido = (cliente.Apellido ?? "").Trim();
            ClienteSeleccionadoCondicion = cliente.CondicionIvaNombre ?? "";
        }
        else
        {
            // Jurídica: usar Razón Social en "Nombre" para la UI; Apellido vacío
            ClienteSeleccionadoNombre = (cliente.RazonSocial ?? "").Trim();
            ClienteSeleccionadoApellido = "";
            ClienteSeleccionadoCondicion = cliente.CondicionIvaNombre ?? "";
        }

        // Documento
        var cuitNum = SoloNumeros(cliente.CUIT);
        var dniNum = SoloNumeros(cliente.DNI);

        if (!string.IsNullOrWhiteSpace(cuitNum))
        {
            ClienteSeleccionadoDoc = cuitNum;
            ClienteSeleccionadoDocLabel = "CUIT";
            Factura.DocTipo = 80;         // CUIT
            Factura.DocNro = cuitNum;    // sin guiones/puntos
        }
        else if (!string.IsNullOrWhiteSpace(dniNum))
        {
            ClienteSeleccionadoDoc = dniNum;
            ClienteSeleccionadoDocLabel = "DNI";
            Factura.DocTipo = 96;         // DNI
            Factura.DocNro = dniNum;     // sin puntos
        }
        else
        {
            // (opcional) fallback para CF sin documento
            // ClienteSeleccionadoDoc = "0";
            // ClienteSeleccionadoDocLabel = "S/D";
            // Factura.DocTipo = 99;
            // Factura.DocNro  = "0";
        }

        // IDs/condiciones
        Factura.PersonaId = cliente.Id;
        Factura.CondicionIVAReceptorId = cliente.CondicionIVAId ?? 0;
        Factura.CondicionIvaReceptorCodigoAfip = cliente.CondicionIvaCodigoAfip;

        await RecalcularComprobanteAsync();
        MostrarModalClientes = false;
    }

    protected string ClienteDisplay =>
    ClienteEsPersonaFisica
        ? string.Join(" ", new[] { ClienteSeleccionadoNombre, ClienteSeleccionadoApellido }
            .Where(s => !string.IsNullOrWhiteSpace(s)))
        : (ClienteSeleccionadoNombre ?? ""); // Razón social




    protected async Task OnOperacionChanged(ChangeEventArgs e)
    {
        OperacionSeleccionada = e.Value?.ToString() ?? "Factura";

        ComprobanteAsociadoSeleccionado = null;
        Factura.CbteAsocTipo = null;
        Factura.CbteAsocPtoVta = null;
        Factura.CbteAsocNro = null;
        Factura.CbteAsocCuit = null;
        Factura.CbteAsocFecha = null;

        await RecalcularComprobanteAsync(); // ✅
    }



    protected async Task RecalcularComprobanteAsync()
    {
        // Si falta info, no recalculamos todavía
        if (UsuarioEmisorCondicionCodigo == 0 || Factura.PersonaId <= 0)
            return;

        var codReceptorAfip = Factura.CondicionIvaReceptorCodigoAfip ?? 0;
        bool esNotaCredito = OperacionSeleccionada == "NotaCredito";
        bool esNotaDebito = OperacionSeleccionada == "NotaDebito";

        var (cbteTipo, descripcion, letra) = await ComprobanteAfipService.DeterminarComprobanteAsync(
            UsuarioEmisorCondicionCodigo,   // Emisor (AFIP)
            codReceptorAfip,                // Receptor (AFIP) ✅
            esNotaCredito,
            esNotaDebito
        );

        Factura.CbteTipo = cbteTipo;
        Factura.TipoComprobanteId = cbteTipo;
        Factura.TipoComprobanteDescripcion = descripcion;
        Factura.TipoComprobanteLetra = letra;
        letraComprobante = letra;

        CalcularTotales(); // <- importante, cambia la interpretación neto/final
        //StateHasChanged();

        StateHasChanged(); // por las dudas forzá render
    }






    protected async Task AbrirModalPresupuestos()
    {
        PresupuestosCliente = await PresupuestoService.BuscarParaFacturarAsync(Factura.PersonaId);
        MostrarModalPresupuestos = true;
    }


    protected void CerrarModalPresupuestos() => MostrarModalPresupuestos = false;


    //protected async Task ImportarPresupuesto(int presupuestoId)
    //{
    //    var pre = await PresupuestoService.ObtenerPorIdAsync(presupuestoId);
    //    var det = await PresupuestoService.ObtenerDetallesAsync(presupuestoId);

    //    // limpiamos y volcamos ítems
    //    //Detalles.Clear();
    //    //foreach (var d in det)
    //    //{
    //    //    var iva = IvaAlicuotas.FirstOrDefault(x => x.Porcentaje == (d.IvaPorcentaje ?? 21))
    //    //              ?? IvaAlicuotas.First(i => i.Porcentaje == 21);

    //    //    Detalles.Add(new FacturaDetalleDto
    //    //    {
    //    //        Descripcion = d.Descripcion,
    //    //        Cantidad = d.Cantidad,
    //    //        PrecioUnitario = d.PrecioUnitario,   // si tu Factura A usa neto y el ppto tiene final, descomponer aquí
    //    //        IvaAlicuotaId = iva.Id
    //    //    });

    //    Detalles.Clear();

    //    foreach (var d in det)
    //    {
    //        // 1) Buscar la alícuota por Id del presupuesto (fallback 21%)
    //        var iva = IvaAlicuotas.FirstOrDefault(x => x.Id == d.IvaAlicuotaId)
    //                  ?? IvaAlicuotas.First(x => x.Porcentaje == 21);

    //        // 2) Si tu Factura A usa precio NETO y el presupuesto guarda precio FINAL,
    //        //    descomponé a neto. Si el presupuesto ya guarda neto, dejá tal cual.
    //        var tasa = (iva.Porcentaje / 100m);
    //        decimal unit = d.PrecioUnitario;

    //        if (EsFacturaA)
    //        {
    //            // ❗ Usa UNA de estas opciones según cómo guardes tus precios de presupuesto:
    //            // a) Si el presupuesto guarda PRECIO FINAL (con IVA):
    //            unit = Math.Round(d.PrecioUnitario / (1m + tasa), 2, MidpointRounding.AwayFromZero);

    //            // b) Si el presupuesto ya guarda PRECIO NETO, entonces:
    //            // unit = d.PrecioUnitario;
    //        }
    //        // Para Factura B/C, se usa el precio final tal cual:
    //        // else { unit = d.PrecioUnitario; }

    //        Detalles.Add(new FacturaDetalleDto
    //        {
    //            Descripcion = d.Descripcion,
    //            Cantidad = d.Cantidad,
    //            PrecioUnitario = unit,
    //            IvaAlicuotaId = iva.Id
    //        });

    //}

    //    Factura.PresupuestoIds ??= new();
    //    Factura.PresupuestoIds.Clear();
    //    Factura.PresupuestoIds.Add(presupuestoId);

    //    await RecalcularComprobanteAsync();
    //    CalcularTotales();

    //    MostrarModalPresupuestos = false;
    //}



    //protected async Task ImportarPresupuesto(int presupuestoId)
    //{
    //    // Aseguramos que EsFacturaA/EsFacturaB/EsFacturaC estén definidos
    //    await RecalcularComprobanteAsync();
    //    var esA = EsFacturaA;

    //    var pre = await PresupuestoService.ObtenerPorIdAsync(presupuestoId);
    //    var det = await PresupuestoService.ObtenerDetallesAsync(presupuestoId);

    //    Detalles.Clear();

    //    foreach (var d in det)
    //    {
    //        // Buscar alícuota por Id (fallback 21%)
    //        var iva = IvaAlicuotas.FirstOrDefault(x => x.Id == d.IvaAlicuotaId)
    //                  ?? IvaAlicuotas.First(x => x.Porcentaje == 21);

    //        var tasa = (iva.Porcentaje / 100m);

    //        // En presupuesto guardás NETO. 
    //        // A: se deja neto; B/C: se convierte a final.
    //        decimal unit = d.PrecioUnitario;
    //        if (!esA) // Factura B o C
    //            unit = Math.Round(d.PrecioUnitario * (1m + tasa), 2, MidpointRounding.AwayFromZero);

    //        Detalles.Add(new FacturaDetalleDto
    //        {
    //            Descripcion = d.Descripcion,
    //            Cantidad = d.Cantidad,
    //            PrecioUnitario = unit,
    //            IvaAlicuotaId = iva.Id
    //        });
    //    }

    //    // Vincular el presupuesto a la factura
    //    Factura.PresupuestoIds ??= new();
    //    Factura.PresupuestoIds.Clear();
    //    Factura.PresupuestoIds.Add(presupuestoId);

    //    // Recalcular totales con la lógica de la factura
    //    CalcularTotales();

    //    MostrarModalPresupuestos = false;
    //}


    protected async Task ImportarPresupuesto(int presupuestoId)
    {
        await RecalcularComprobanteAsync();
        var esA = EsFacturaA;

        // ⬇️ Traer SOLO pendientes
        var pendientes = await PresupuestoService.ObtenerPendientesPorDetalleAsync(presupuestoId);
        Detalles.Clear();

      

        foreach (var p in pendientes.Where(x => x.CantidadPendiente > 0m))
        {
            var iva = IvaAlicuotas.FirstOrDefault(x => x.Id == p.IvaAlicuotaId)
                      ?? IvaAlicuotas.First(x => x.Porcentaje == 21);
            var tasa = iva.Porcentaje / 100m;

            // ⚠️ Siempre neto en la línea
            var unitNeto = p.PrecioUnitarioPresupuesto;
            var unitFinal = Math.Round(unitNeto * (1m + tasa), 2, MidpointRounding.AwayFromZero);

            Detalles.Add(new FacturaDetalleDto
            {
                Descripcion = p.Descripcion,
                Cantidad = p.CantidadPendiente,
                PrecioUnitario = unitNeto,              // ← SIEMPRE NETO
                IvaAlicuotaId = iva.Id,
                PresupuestoDetalleId = p.PresupuestoDetalleId,

                // Campos de referencia/visualización
                PrecioPresNeto = unitNeto,
                TasaIvaPres = tasa,
                PrecioPresFinal = unitFinal,
                CantidadPendiente = p.CantidadPendiente,

                PermitirRenegociar = false
            });
        }

        // vincular el presupuesto (si querés para tracking visual)
        Factura.PresupuestoIds ??= new();
        Factura.PresupuestoIds.Clear();
        Factura.PresupuestoIds.Add(presupuestoId);

        CalcularTotales();
        MostrarModalPresupuestos = false;

        // fuerza re-render si el modal no provoca ciclo de render
        await InvokeAsync(StateHasChanged);
    }



    protected void OnCantidadInput(FacturaDetalleDto item, ChangeEventArgs e)
    {
        var raw = (e.Value?.ToString() ?? "").Replace(',', '.');
        if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var nuevaCant)) return;

        if (item.PresupuestoDetalleId.HasValue)
        {
            if (nuevaCant > item.CantidadPendiente + EPS) nuevaCant = item.CantidadPendiente;
            if (nuevaCant < 0m) nuevaCant = 0m;
        }

        item.Cantidad = Math.Round(nuevaCant, 2, MidpointRounding.AwayFromZero);
        CalcularTotales();
    }

    protected void OnPrecioInput(FacturaDetalleDto item, ChangeEventArgs e)
    {
        if (item.EsDePresupuesto && !item.PermitirRenegociar) return; // 👈 bloqueado

        var raw = (e.Value?.ToString() ?? "").Replace(',', '.');
        if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var nuevoPrecio)) return;

        item.PrecioUnitario = nuevoPrecio;
        CalcularTotales();
    }

    protected void OnIvaChanged(FacturaDetalleDto item, ChangeEventArgs e)
    {
        if (item.EsDePresupuesto && !item.PermitirRenegociar) return; // 👈 bloqueado
        if (int.TryParse(e.Value?.ToString(), out var ivaId))
        {
            item.IvaAlicuotaId = ivaId;
            CalcularTotales();
        }
    }




    protected void ToggleRenegociar(FacturaDetalleDto item)
    {
        item.PermitirRenegociar = !item.PermitirRenegociar;

        if (item.PermitirRenegociar)
        {
            // Al desbloquear, congelamos el importe final objetivo
            var tasa = GetTasa(item.IvaAlicuotaId) / 100m;
            var esA = EsFacturaA;

            var precioFinalActual = esA
                ? Math.Round(item.PrecioUnitario * (1m + tasa), 2, MidpointRounding.AwayFromZero)
                : item.PrecioUnitario;

            item.ImporteObjetivoFinal = Math.Round(item.Cantidad * precioFinalActual, 2, MidpointRounding.AwayFromZero);
            item.CantidadBase = item.Cantidad;
            item.PrecioBase = item.PrecioUnitario;
        }

        StateHasChanged();
    }


  

    protected decimal ClampCantidad(FacturaDetalleDto item, decimal cant)
    {
        const decimal EPS = 0.01m;
        if (item.EsDePresupuesto)
        {
            if (cant > item.CantidadPendiente + EPS) cant = item.CantidadPendiente;
            if (cant < 0m) cant = 0m;
        }
        return Math.Round(cant, 2, MidpointRounding.AwayFromZero);
    }

    //protected void OnIvaChanged(FacturaDetalleDto item, ChangeEventArgs e)
    //{
    //    if (int.TryParse(e.Value?.ToString(), out var ivaId))
    //    {
    //        item.IvaAlicuotaId = ivaId;
    //        CalcularTotales();
    //    }
    //}

































    protected async Task ConfirmarFacturar()
    {
        if (Factura.FormaVenta == FormaVenta.CuentaCorriente)
        {
            // Aviso: se cargará/migrará a CC
            var ok = await JS.InvokeAsync<bool>("confirm", "Se registrará el saldo en la cuenta corriente (sin duplicar). ¿Deseás continuar?");
            if (!ok) return;
        }
        else
        {
            // Aviso: contado
            var ok = await JS.InvokeAsync<bool>("confirm", "Venta contado: no se cargará en cuenta corriente. ¿Deseás continuar?");
            if (!ok) return;
        }

        await SolicitarCAE(); // o GuardarFactura (PV=0) si no AFIP
    }












































    protected void AgregarItem()
    {
        var ivaPorDefecto = IvaAlicuotas.FirstOrDefault(i => i.Porcentaje == 21)?.Id //Tomamos el 21% por defecto
                            ?? IvaAlicuotas.FirstOrDefault()?.Id
                            ?? 0;
        Detalles.Add(new FacturaDetalleDto
        {
            Cantidad = 1,
            PrecioUnitario = 0,
            IvaAlicuotaId =ivaPorDefecto
        });
        CalcularTotales();
    }



    protected void EliminarItem(FacturaDetalleDto item)
    {
        Detalles.Remove(item);
        CalcularTotales();
    }

    //protected void CalcularTotales()
    //{
    //    Factura.Neto = Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
    //    Factura.Iva = Detalles.Sum(d => (d.Cantidad * d.PrecioUnitario) * (d.IvaAlicuotaId == 1 ? 0.21m : d.IvaAlicuotaId == 2 ? 0.105m : 0));
    //    Factura.Total = Factura.Neto + Factura.Iva;
    //}

    //protected void CalcularTotales()
    //{
    //    Factura.Neto = Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

    //    Factura.Iva = Detalles.Sum(d =>
    //        (d.Cantidad * d.PrecioUnitario) *
    //        (d.IvaAlicuotaId == 1 ? 0.21m :
    //         d.IvaAlicuotaId == 2 ? 0.105m : 0));

    //    Factura.Total = Factura.Neto + Factura.Iva;

    //    // Opcional: IVA discriminado por alícuota
    //    Factura.IvaDiscriminado = Detalles
    //        .GroupBy(d => d.IvaAlicuotaId)
    //        .Select(g => new AlicIvaDto
    //        {
    //            Id = g.Key,
    //            BaseImp = g.Sum(d => d.Cantidad * d.PrecioUnitario),
    //            Importe = g.Sum(d =>
    //                (d.Cantidad * d.PrecioUnitario) *
    //                (g.Key == 1 ? 0.21m : g.Key == 2 ? 0.105m : 0))
    //        }).ToList();
    //}

    //protected void OnIvaChanged(FacturaDetalleDto item, ChangeEventArgs e)
    //{
    //    item.IvaAlicuotaId = int.Parse(e.Value.ToString()!);
    //    CalcularTotales(); // 🔄 Recalcular totales e IVA discriminado
    //}




    //protected void CalcularTotales()
    //{
    //    Factura.Neto = Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

    //    Factura.Iva = Detalles.Sum(d =>
    //    {
    //        var alicuota = IvaAlicuotas.FirstOrDefault(a => a.Id == d.IvaAlicuotaId);
    //        return alicuota != null
    //            ? (d.Cantidad * d.PrecioUnitario) * (alicuota.Porcentaje / 100)
    //            : 0;
    //    });

    //    Factura.Total = Factura.Neto + Factura.Iva;

    //    // IVA discriminado con Código AFIP
    //    Factura.IvaDiscriminado = Detalles
    //        .GroupBy(d => d.IvaAlicuotaId)
    //        .Select(g =>
    //        {
    //            var alicuota = IvaAlicuotas.FirstOrDefault(a => a.Id == g.Key);
    //            var baseImp = g.Sum(x => x.Cantidad * x.PrecioUnitario);
    //            return new AlicIvaDto
    //            {
    //                AlicuotaId = alicuota?.CodigoAfip ?? 0, // ✅ Para AFIP
    //                BaseImp = baseImp,
    //                Importe = alicuota != null ? baseImp * (alicuota.Porcentaje / 100) : 0
    //            };
    //        })
    //        .ToList();
    //}

    //protected void CalcularTotales()
    //{
    //    decimal neto = 0m, iva = 0m, total = 0m;

    //    // Para armar IVA discriminado
    //    var acumulados = new Dictionary<int, (decimal BaseImp, decimal Iva)>();

    //    foreach (var d in Detalles)
    //    {
    //        var tasaPct = GetTasa(d.IvaAlicuotaId);      // 21, 10.5, 0, etc.
    //        var tasa = tasaPct / 100m;
    //        var qty = d.Cantidad;
    //        var unit = d.PrecioUnitario;

    //        decimal baseLinea, ivaLinea, totalLinea;

    //        if (EsFacturaA)
    //        {
    //            // unit = NETO
    //            baseLinea = qty * unit;
    //            ivaLinea = Math.Round(baseLinea * tasa, 2, MidpointRounding.AwayFromZero);
    //            totalLinea = baseLinea + ivaLinea;
    //        }
    //        else if (EsFacturaB)
    //        {
    //            // unit = FINAL (IVA incluido)
    //            totalLinea = qty * unit;
    //            if (tasa > 0)
    //            {
    //                // Descomponer final -> neto + iva
    //                baseLinea = Math.Round(totalLinea / (1m + tasa), 2, MidpointRounding.AwayFromZero);
    //                ivaLinea = Math.Round(totalLinea - baseLinea, 2, MidpointRounding.AwayFromZero);
    //            }
    //            else
    //            {
    //                baseLinea = totalLinea; // exento o 0%
    //                ivaLinea = 0m;
    //            }
    //        }
    //        else // Factura C
    //        {
    //            // No se discrimina IVA
    //            baseLinea = qty * unit; // tratamos como precio final sin IVA
    //            ivaLinea = 0m;
    //            totalLinea = baseLinea;
    //        }

    //        neto += baseLinea;
    //        iva += ivaLinea;
    //        total += totalLinea;

    //        // Acumular por alícuota para el discriminado/AFIP
    //        if (!acumulados.ContainsKey(d.IvaAlicuotaId))
    //            acumulados[d.IvaAlicuotaId] = (0m, 0m);

    //        var current = acumulados[d.IvaAlicuotaId];
    //        acumulados[d.IvaAlicuotaId] = (
    //            Math.Round(current.BaseImp + baseLinea, 2, MidpointRounding.AwayFromZero),
    //            Math.Round(current.Iva + ivaLinea, 2, MidpointRounding.AwayFromZero)
    //        );
    //    }

    //    Factura.Neto = Math.Round(neto, 2, MidpointRounding.AwayFromZero);
    //    Factura.Iva = Math.Round(iva, 2, MidpointRounding.AwayFromZero);
    //    Factura.Total = Math.Round(total, 2, MidpointRounding.AwayFromZero);

    //    // IVA discriminado (para tu UI / libros)
    //    Factura.IvaDiscriminado = acumulados
    //        .Select(kv =>
    //        {
    //            var alicuota = IvaAlicuotas.FirstOrDefault(a => a.Id == kv.Key);
    //            return new AlicIvaDto
    //            {
    //                AlicuotaId = alicuota?.Id ?? 0,           // tu Id interno
    //                BaseImp = kv.Value.BaseImp,
    //                Importe = kv.Value.Iva
    //            };
    //        })
    //        .ToList();

    //    // AFIP (usa Código AFIP)
    //    Factura.IvaDiscriminadoAfip = acumulados
    //        .Select(kv =>
    //        {
    //            var alicuota = IvaAlicuotas.First(a => a.Id == kv.Key);
    //            return new AlicIvaAfipDto
    //            {
    //                Id = alicuota.CodigoAfip, // 5:21%; 4:10.5%; 3:Exento, etc.
    //                BaseImp = kv.Value.BaseImp,
    //                Importe = kv.Value.Iva
    //            };
    //        })
    //        .ToList();

    //    // Si no hay IVA, enviar exento para AFIP (requerido)
    //    if (!Factura.IvaDiscriminadoAfip.Any())
    //    {
    //        Factura.IvaDiscriminadoAfip.Add(new AlicIvaAfipDto
    //        {
    //            Id = 3, // Exento
    //            BaseImp = Factura.Neto,
    //            Importe = 0m
    //        });
    //    }
    //}










    //protected void CalcularTotales()
    //{
    //    // Acumulación cruda (sin redondear) por alícuota
    //    var acumuladosRaw = new Dictionary<int, (decimal BaseImp, decimal Iva)>();

    //    decimal netoRaw = 0m, ivaRaw = 0m, totalRaw = 0m;

    //    foreach (var d in Detalles)
    //    {
    //        var tasa = GetTasa(d.IvaAlicuotaId) / 100m;
    //        var qty = d.Cantidad;
    //        var unit = d.PrecioUnitario;

    //        decimal baseLinea, ivaLinea, totalLinea;

    //        if (EsFacturaA)
    //        {
    //            // PrecioUnitario = NETO
    //            baseLinea = qty * unit;
    //            ivaLinea = baseLinea * tasa;     // sin redondear
    //            totalLinea = baseLinea + ivaLinea; // sin redondear
    //        }
    //        else if (EsFacturaB)
    //        {
    //            // PrecioUnitario = FINAL (IVA inc.)
    //            totalLinea = qty * unit;
    //            if (tasa > 0m)
    //            {
    //                baseLinea = totalLinea / (1m + tasa); // sin redondear
    //                ivaLinea = totalLinea - baseLinea;   // sin redondear
    //            }
    //            else
    //            {
    //                baseLinea = totalLinea;
    //                ivaLinea = 0m;
    //            }
    //        }
    //        else // C
    //        {
    //            // No se discrimina IVA
    //            baseLinea = qty * unit; // tratamos como final
    //            ivaLinea = 0m;
    //            totalLinea = baseLinea;
    //        }


    //        // 👇 AQUI
    //        //d.TotalConIva = Math.Round(totalLinea, 2, MidpointRounding.AwayFromZero);

    //        netoRaw += baseLinea;
    //        ivaRaw += ivaLinea;
    //        totalRaw += totalLinea;

    //        if (!acumuladosRaw.TryGetValue(d.IvaAlicuotaId, out var acc))
    //            acc = (0m, 0m);
    //        acc.BaseImp += baseLinea; // sin redondear
    //        acc.Iva += ivaLinea;  // sin redondear
    //        acumuladosRaw[d.IvaAlicuotaId] = acc;
    //    }

    //    // Redondeo final
    //    Factura.Neto = Math.Round(netoRaw, 2, MidpointRounding.AwayFromZero);
    //    Factura.Iva = Math.Round(ivaRaw, 2, MidpointRounding.AwayFromZero);
    //    Factura.Total = Math.Round(totalRaw, 2, MidpointRounding.AwayFromZero);

    //    // IVA discriminado (UI/libros) – redondeo por alícuota
    //    Factura.IvaDiscriminado = acumuladosRaw.Select(kv =>
    //    {
    //        var alicuota = IvaAlicuotas.First(a => a.Id == kv.Key);
    //        return new AlicIvaDto
    //        {
    //            AlicuotaId = alicuota.Id,
    //            BaseImp = Math.Round(kv.Value.BaseImp, 2, MidpointRounding.AwayFromZero),
    //            Importe = Math.Round(kv.Value.Iva, 2, MidpointRounding.AwayFromZero)
    //        };
    //    }).ToList();

    //    // AFIP (CódigoAfip)
    //    Factura.IvaDiscriminadoAfip = acumuladosRaw.Select(kv =>
    //    {
    //        var alicuota = IvaAlicuotas.First(a => a.Id == kv.Key);
    //        return new AlicIvaAfipDto
    //        {
    //            Id = alicuota.CodigoAfip, // p.ej. 5: 21%, 4: 10.5%
    //            BaseImp = Math.Round(kv.Value.BaseImp, 2, MidpointRounding.AwayFromZero),
    //            Importe = Math.Round(kv.Value.Iva, 2, MidpointRounding.AwayFromZero)
    //        };
    //    }).ToList();

    //    // Si no hay IVA, AFIP pide línea de Exento
    //    if (!Factura.IvaDiscriminadoAfip.Any())
    //    {
    //        Factura.IvaDiscriminadoAfip.Add(new AlicIvaAfipDto
    //        {
    //            Id = 3, // Exento
    //            BaseImp = Factura.Neto,
    //            Importe = 0m
    //        });
    //    }
    //}






















    //protected void CalcularTotales()
    //{
    //    var acumuladosRaw = new Dictionary<int, (decimal BaseImp, decimal Iva)>();
    //    decimal netoRaw = 0m, ivaRaw = 0m, totalRaw = 0m;

    //    foreach (var d in Detalles)
    //    {
    //        var tasaBase = GetTasa(d.IvaAlicuotaId) / 100m;

    //        // Para Factura C, la tasa es 0 (no corresponde IVA)
    //        var tasa = EsFacturaC ? 0m : tasaBase;

    //        var qty = d.Cantidad;
    //        var unit = d.PrecioUnitario;       // <-- SIEMPRE NETO

    //        var baseLinea = qty * unit;
    //        var ivaLinea = baseLinea * tasa;  // sin redondear
    //        var totalLinea = baseLinea + ivaLinea;

    //        netoRaw += baseLinea;
    //        ivaRaw += ivaLinea;
    //        totalRaw += totalLinea;

    //        if (!acumuladosRaw.TryGetValue(d.IvaAlicuotaId, out var acc))
    //            acc = (0m, 0m);
    //        acc.BaseImp += baseLinea;
    //        acc.Iva += ivaLinea;
    //        acumuladosRaw[d.IvaAlicuotaId] = acc;
    //    }

    //    // Redondeo final
    //    Factura.Neto = Math.Round(netoRaw, 2, MidpointRounding.AwayFromZero);
    //    Factura.Iva = Math.Round(ivaRaw, 2, MidpointRounding.AwayFromZero);
    //    Factura.Total = Math.Round(totalRaw, 2, MidpointRounding.AwayFromZero);

    //    // Arreglo de IVA discriminado SOLO para A (para UI y para AFIP)
    //    if (EsFacturaA)
    //    {
    //        Factura.IvaDiscriminado = acumuladosRaw.Select(kv =>
    //        {
    //            var alicuota = IvaAlicuotas.First(a => a.Id == kv.Key);
    //            return new AlicIvaDto
    //            {
    //                AlicuotaId = alicuota.Id,
    //                BaseImp = Math.Round(kv.Value.BaseImp, 2, MidpointRounding.AwayFromZero),
    //                Importe = Math.Round(kv.Value.Iva, 2, MidpointRounding.AwayFromZero)
    //            };
    //        }).ToList();

    //        Factura.IvaDiscriminadoAfip = acumuladosRaw.Select(kv =>
    //        {
    //            var alicuota = IvaAlicuotas.First(a => a.Id == kv.Key);
    //            return new AlicIvaAfipDto
    //            {
    //                Id = alicuota.CodigoAfip,
    //                BaseImp = Math.Round(kv.Value.BaseImp, 2, MidpointRounding.AwayFromZero),
    //                Importe = Math.Round(kv.Value.Iva, 2, MidpointRounding.AwayFromZero)
    //            };
    //        }).ToList();

    //        if (!Factura.IvaDiscriminadoAfip.Any())
    //        {
    //            Factura.IvaDiscriminadoAfip.Add(new AlicIvaAfipDto
    //            {
    //                Id = 3, // Exento
    //                BaseImp = Factura.Neto,
    //                Importe = 0m
    //            });
    //        }
    //    }
    //    else
    //    {
    //        // En B y C no mostrar detalle por alícuotas
    //        Factura.IvaDiscriminado = new List<AlicIvaDto>();
    //        Factura.IvaDiscriminadoAfip = new List<AlicIvaAfipDto>();
    //    }
    //}
















    protected void CalcularTotales()
    {
        var acumuladosRaw = new Dictionary<int, (decimal BaseImp, decimal Iva)>();
        decimal netoRaw = 0m, ivaRaw = 0m, totalRaw = 0m;

        foreach (var d in Detalles)
        {
            var tasaBase = GetTasa(d.IvaAlicuotaId) / 100m;

            // Para Factura C, la tasa es 0 (no corresponde IVA)
            var tasa = EsFacturaC ? 0m : tasaBase;

            var qty = d.Cantidad;
            var unit = d.PrecioUnitario;       // <-- SIEMPRE NETO

            var baseLinea = qty * unit;
            var ivaLinea = baseLinea * tasa;   // sin redondear
            var totalLinea = baseLinea + ivaLinea;

            netoRaw += baseLinea;
            ivaRaw += ivaLinea;
            totalRaw += totalLinea;

            if (!acumuladosRaw.TryGetValue(d.IvaAlicuotaId, out var acc))
                acc = (0m, 0m);

            acc.BaseImp += baseLinea;
            acc.Iva += ivaLinea;
            acumuladosRaw[d.IvaAlicuotaId] = acc;
        }

        // Redondeo final
        Factura.Neto = Math.Round(netoRaw, 2, MidpointRounding.AwayFromZero);
        Factura.Iva = Math.Round(ivaRaw, 2, MidpointRounding.AwayFromZero);
        Factura.Total = Math.Round(totalRaw, 2, MidpointRounding.AwayFromZero);

        // =========================
        // 1) IVA discriminado (UI)
        // =========================
        if (EsFacturaA)
        {
            // Lo seguís mostrando igual que antes
            Factura.IvaDiscriminado = acumuladosRaw.Select(kv =>
            {
                var alicuota = IvaAlicuotas.First(a => a.Id == kv.Key);
                return new AlicIvaDto
                {
                    AlicuotaId = alicuota.Id,
                    BaseImp = Math.Round(kv.Value.BaseImp, 2, MidpointRounding.AwayFromZero),
                    Importe = Math.Round(kv.Value.Iva, 2, MidpointRounding.AwayFromZero)
                };
            }).ToList();
        }
        else
        {
            // En B y C no mostramos desglose al usuario
            Factura.IvaDiscriminado = new List<AlicIvaDto>();
        }

        // =============================================
        // 2) IVA discriminado para AFIP (A y B con IVA)
        // =============================================
        Factura.IvaDiscriminadoAfip = new List<AlicIvaAfipDto>();

        // Solo tiene sentido mandar AlicIva si hay IVA > 0 y NO es C
        var tieneIva = Factura.Iva > 0;

        if (!EsFacturaC && tieneIva && acumuladosRaw.Any())
        {
            // Generamos por alícuota a partir de acumuladosRaw
            foreach (var kv in acumuladosRaw)
            {
                var baseImp = Math.Round(kv.Value.BaseImp, 2, MidpointRounding.AwayFromZero);
                var iva = Math.Round(kv.Value.Iva, 2, MidpointRounding.AwayFromZero);

                if (baseImp == 0 && iva == 0) continue;

                var alicuota = IvaAlicuotas.First(a => a.Id == kv.Key);

                Factura.IvaDiscriminadoAfip.Add(new AlicIvaAfipDto
                {
                    Id = alicuota.CodigoAfip, // código AFIP (5 = 21%, 4 = 10.5, etc.)
                    BaseImp = baseImp,
                    Importe = iva
                });
            }

            // Seguridad: si por algún motivo quedó vacío, mandamos todo al 21%
            if (!Factura.IvaDiscriminadoAfip.Any())
            {
                Factura.IvaDiscriminadoAfip.Add(new AlicIvaAfipDto
                {
                    Id = 5, // 21%
                    BaseImp = Factura.Neto,
                    Importe = Factura.Iva
                });
            }
        }
        else
        {
            // C o sin IVA -> AFIP no exige AlicIva si ImpIVA = 0
            // Podés dejar Factura.IvaDiscriminadoAfip vacío.
        }
    }






















    //Modal para datos Afip manuales al guardar factura



    // --- Eventos UI ---
    protected void OnToggleDatosManuales(bool check)
    {
        if (check)
            AbrirModal();
        else
            DatosManuales = new();
    }

    protected void AbrirModal() => MostrarModalFiscales = true;

    protected void CerrarModal()
    {
        MostrarModalFiscales = false;
        StateHasChanged();
    }

    protected void OnGuardarDatosManuales()
    {
        // El EditForm validó DataAnnotations; cerramos si pasa
        MostrarModalFiscales = false;
        StateHasChanged();
    }

    // --- Validaciones ---
    protected string? ValidarCaeSoloDigitos(string? cae)
    {
        if (string.IsNullOrWhiteSpace(cae)) return null; // vacío permitido si no cargan CAE
        return cae.All(char.IsDigit) ? null : "El CAE debe ser solo números";
    }

    protected bool FormularioManualValido()
    {
        // Si cargan CAE, pedimos 14 dígitos + vto + número
        if (!string.IsNullOrWhiteSpace(DatosManuales.CAE))
        {
            if (ValidarCaeSoloDigitos(DatosManuales.CAE) is not null) return false;
            if (DatosManuales.CAE!.Length != 14) return false;
            if (DatosManuales.CAEFchVto is null) return false;
            if (DatosManuales.Numero is null || DatosManuales.Numero <= 0) return false;
        }
        return true;
    }
















    //protected async Task SolicitarCAE()
    //{
    //    Factura.Detalles = Detalles;
    //    var auth = await AfipFacturaService.ObtenerTokenAsync(); // token + sign
    //    var cae = await AfipFacturaService.SolicitarCaeAsync(Factura);
    //    Factura.Cae = long.Parse(cae.CAE);
    //    Factura.FechaVencimientoCae = cae.CAEFchVto;
    //    await FacturaService.CrearAsync(Factura);
    //}

    protected async Task SolicitarCAE()
    {
        Factura.Detalles = Detalles;
        Factura.UsuarioId = UsuarioId; // ✅ Pasar el usuario emisor al backend


        // ✅ Construir datos obligatorios antes de AFIP
        await PrepararFacturaParaAfip();

        // 🔎 Validación rápida: evitar enviar datos incompletos
        if (string.IsNullOrEmpty(Factura.PersonaCUIT) && Factura.DocTipo == 80)
        {
            Console.WriteLine("❌ Error: PersonaCUIT requerido para DocTipo=80");
            return;
        }

        // ✅ Obtener el punto de venta para validar el Número
        var puntoVenta = await PuntoVentaService.ObtenerPorIdAsync(Factura.PuntoVentaId);
        if (puntoVenta == null)
        {
            Console.WriteLine("❌ Error: No se encontró el Punto de Venta seleccionado.");
            return;
        }

        // ⚠️ Validar si el número del punto de venta es 0
        if (puntoVenta.Numero == 0)
        {
            MostrarConfirmacionPtoVenta0 = true;
            return;
        }


        // 🔹 Facturación normal con AFIP
        await FacturarConAfip();

        //var cae = await AfipFacturaService.SolicitarCaeAsync(Factura);
        //Factura.Cae = long.Parse(cae.CAE);
        //Factura.FechaVencimientoCae = cae.CAEFchVto;

        //await FacturaService.CrearAsync(Factura);
    }







    //private void PrepararFacturaParaAfip()
    //{
    //    // ✅ Asegurar que el CUIT/DNI del receptor está bien
    //    Factura.PersonaCUIT = Factura.DocTipo == 80 ? Factura.DocNro : null;

    //    // ✅ Usar CbteTipo también como TipoComprobanteId para AFIP
    //    Factura.TipoComprobanteId = Factura.CbteTipo;

    //    // ✅ Normalizar Concepto (1=Productos, 2=Servicios, 3=Ambos)
    //    if (Factura.Concepto == 0)
    //        Factura.Concepto = ConceptoFactura.Servicio;

    //    // ✅ Confirmar moneda y cotización
    //    Factura.MonId = "PES";
    //    Factura.MonCotiz = 1;

    //    // ✅ Confirmar IVA discriminado
    //    if (Factura.IvaDiscriminado == null || !Factura.IvaDiscriminado.Any())
    //    {
    //        Factura.IvaDiscriminado = Detalles
    //            .GroupBy(d => d.IvaAlicuotaId)
    //            .Select(g => new AlicIvaDto
    //            {
    //                Id = g.Key,
    //                BaseImp = g.Sum(d => d.Cantidad * d.PrecioUnitario),
    //                Importe = g.Sum(d =>
    //                    (d.Cantidad * d.PrecioUnitario) *
    //                    (g.Key == 1 ? 0.21m :
    //                     g.Key == 2 ? 0.105m : 0))
    //            }).ToList();
    //    }
    //}

    private async Task PrepararFacturaParaAfip()
    {
        // ✅ Asegurar que el CUIT/DNI del receptor está bien
        Factura.PersonaCUIT = Factura.DocTipo == 80 ? Factura.DocNro : null;

        // ✅ Usar CbteTipo también como TipoComprobanteId para AFIP
        Factura.TipoComprobanteId = Factura.CbteTipo;

        // ✅ Completar letra del comprobante desde la tabla de tipos
       
        Factura.TipoComprobanteLetra = letraComprobante;

        

        if (ClienteEsPersonaFisica)
        {
            Factura.PersonaNombre = ClienteSeleccionadoNombre;   // solo nombre
            Factura.PersonaApellido = ClienteSeleccionadoApellido; // solo apellido
                                                                   // Si manejás PersonaRazonSocial en el DTO:
                                                                   // Factura.PersonaRazonSocial = null;
        }
        else
        {
            // Jurídica: va todo en Razón Social (o en PersonaNombre si solo tenés ese campo)
            // Si tu DTO tiene PersonaRazonSocial:
            Factura.PersonaRazonSocial = ClienteSeleccionadoNombre;
            Factura.PersonaNombre = null;
            Factura.PersonaApellido = null;

            // Si tu DTO SOLO tiene PersonaNombre/Apellido, podés setear:
            //Factura.PersonaNombre = ClienteSeleccionadoNombre; // Razón Social completa
            //Factura.PersonaApellido = "";                      // vacío
        }

        //Agregamos Detalles de la factura
        Factura.Detalles = Detalles;

        // ✅ Normalizar Concepto (1=Productos, 2=Servicios, 3=Ambos)
        if (Factura.Concepto == 0)
            Factura.Concepto = ConceptoFactura.Servicio;

        // ✅ Confirmar moneda y cotización
        Factura.MonId = "PES";
        Factura.MonCotiz = 1;





       
        CalcularTotales();

        // ✅ Completar Punto de Venta desde el backend
        var puntoVenta = await PuntoVentaService.ObtenerPorIdAsync(Factura.PuntoVentaId);
        if (puntoVenta != null)
        {
            Factura.PuntoVentaNumero = puntoVenta.Numero;         // Número real para AFIP
            Factura.PuntoVentaDescripcion = puntoVenta.Descripcion; // Opcional para mostrar en UI
        }
        else
        {
            Console.WriteLine("⚠️ No se encontró el Punto de Venta con Id " + Factura.PuntoVentaId);
        }

       


        // Ya calculado por CalcularTotales()
        var ivaAfip = Factura.IvaDiscriminadoAfip
            .Select(x => new AlicIvaAfipDto { Id = x.Id, BaseImp = x.BaseImp, Importe = x.Importe })
            .ToList();
    }




    //protected async Task GuardarFacturaSinAfip()
    //{

    //    //ver que completar del dto antes de enviar

    //    MostrarConfirmacionPtoVenta0 = false;
    //    await FacturaService.CrearAsync(Factura); // Guarda en DB sin AFIP
    //    Nav.NavigateTo("/facturas");

    //    // Si NO navegás y querés emitir otra al toque en la misma vista:
    //    // ResetFactura(mantenerCliente: false, mantenerPtoVta: true);
    //}


    //protected async Task GuardarFacturaSinAfip()
    //{
    //    try
    //    {
    //        MostrarConfirmacionPtoVenta0 = false;

    //        // Asegurate que Factura.FormaVenta y Factura.PresupuestoIds estén completos
    //        // (y Detalles, Totales, etc. ya calculados)

    //        var crear = BuildCrearDtoDesdeFactura();
    //        await FacturaService.CrearSoloDbAsync(crear);
    //        //var creada = await FacturaService.CrearSoloDbAsync(Factura);

    //        // Mensaje (opcional)
    //        FacturaCreadaExito = true;
    //        MensajeExito = "✅ Factura guardada en el sistema (sin AFIP).";

    //        // Redirigir (o resetear el form si querés emitir otra)
    //        Nav.NavigateTo("/facturas");
    //    }
    //    catch (InvalidOperationException ex)
    //    {
    //        // Mensaje claro por conflicto (ej.: contado con imputaciones previas)
    //        FacturaCreadaExito = false;
    //        MensajeExito = $"❌ {ex.Message}";
    //        StateHasChanged();
    //    }
    //    catch (Exception ex)
    //    {
    //        FacturaCreadaExito = false;
    //        MensajeExito = $"❌ Error al guardar: {ex.Message}";
    //        StateHasChanged();
    //    }
    //}




    protected async Task GuardarFacturaSinAfip()
    {
        try
        {
            MostrarConfirmacionPtoVenta0 = false;

            // si el modal está abierto, validá:
            if (CargarDatosFiscalesManuales && !FormularioManualValido())
                throw new InvalidOperationException("Revisá los datos fiscales manuales.");


            var crear = BuildCrearDtoDesdeFactura();
            await FacturaService.CrearSoloDbAsync(crear);

            FacturaCreadaExito = true;
            MensajeExito = "✅ Factura guardada en el sistema (sin AFIP).";
            await Toast("✅ Factura guardada en el sistema (sin AFIP).", "success");
            Nav.NavigateTo("/facturas");
        }
        catch (InvalidOperationException ex)
        {
            FacturaCreadaExito = false;
            MensajeExito = $"❌ {ex.Message}";
            await Toast($"❌ {ex.Message}", "warning");
            StateHasChanged();
        }
        catch (Exception ex)
        {
            FacturaCreadaExito = false;
            MensajeExito = $"❌ Error al guardar: {ex.Message}";
            await Toast($"❌ Error al guardar: {ex.Message}", "error");
            StateHasChanged();
        }
    }



    //protected async Task FacturarConAfip()
    //{
    //    var jsonDebug = System.Text.Json.JsonSerializer.Serialize(Factura, new JsonSerializerOptions { WriteIndented = true });
    //    System.Diagnostics.Debug.WriteLine("📦 Factura lista para AFIP:\n" + jsonDebug);

    //    try
    //    {
    //        // 🔹 Autenticación y solicitud CAE a AFIP
    //        var cae = await AfipFacturaService.SolicitarCaeAsync(Factura);

    //        // ✅ Asignar datos del CAE a la factura
    //        Factura.CAE = cae.CAE;
    //        Factura.CAEFchVto = cae.CAEFchVto;

    //        // 🔹 Guardar en la base de datos
    //        await FacturaService.CrearAsync(Factura);

    //        // 🔹 Mostrar mensaje de éxito
    //        FacturaCreadaExito = true;
    //        MensajeExito = $"Factura creada en AFIP con éxito. CAE: {cae.CAE} " +
    //                       $"{(cae.CAEFchVto.HasValue ? $"(Vto: {cae.CAEFchVto.Value:dd/MM/yyyy})" : "(Sin vencimiento)")}";

    //        StateHasChanged(); // Refrescar UI
    //    }
    //    catch (Exception ex)
    //    {
    //        // Manejar error en UI (puedes agregar un Toast o alerta)
    //        FacturaCreadaExito = false;
    //        MensajeExito = $"❌ Error al crear factura: {ex.Message}";
    //    }
    //}






    //protected CrearFacturaDto BuildCrearDtoDesdeFactura()
    //{
    //    var dto = new CrearFacturaDto
    //    {
    //        Numero = Factura.Numero,
    //        PuntoVentaId = Factura.PuntoVentaId,
    //        PuntoVentaNumero = Factura.PuntoVentaNumero,
    //        PuntoVentaDescripcion = Factura.PuntoVentaDescripcion,
    //        TipoComprobanteId = Factura.TipoComprobanteId,
    //        Fecha = Factura.Fecha,
    //        PersonaId = Factura.PersonaId,
    //        Observaciones = Factura.Observaciones,

    //        CbteTipo = Factura.CbteTipo,
    //        DocTipo = Factura.DocTipo,
    //        DocNro = Factura.DocNro,
    //        MonId = Factura.MonId,
    //        MonCotiz = Factura.MonCotiz,
    //        Concepto = Factura.Concepto,

    //        FormaVenta = Factura.FormaVenta,
    //        UsuarioId = UsuarioId,

    //        // Totales (ya calculados en tu front)
    //        Neto = Factura.Neto,
    //        Iva = Factura.Iva,
    //        Total = Factura.Total,

    //        // Detalles → DTO de crear
    //        Detalles = Detalles.Select(d => new CrearFacturaDetalleDto
    //        {
    //            Descripcion = d.Descripcion,
    //            Cantidad = d.Cantidad,
    //            PrecioUnitario = d.PrecioUnitario,
    //            IvaAlicuotaId = d.IvaAlicuotaId
    //        }).ToList(),

    //        // Vínculos a presupuesto (tu VM debería tenerlos)
    //        PresupuestoIds = Factura.PresupuestoIds?.ToList() ?? new(),
    //        VinculosPresupuesto = Factura.VinculosPresupuesto
    //    };

    //    // Overrides manuales si el usuario tildó el toggle
    //    if (CargarDatosFiscalesManuales)
    //    {
    //        if (DatosManuales.PuntoVentaNumero is int pv) dto.PuntoVentaNumero = pv;
    //        if (DatosManuales.Numero is int nro) dto.Numero = nro;
    //        if (DatosManuales.Fecha is DateTime fch) dto.Fecha = fch;
    //        if (!string.IsNullOrWhiteSpace(DatosManuales.CAE)) dto.CAE = DatosManuales.CAE;
    //        if (DatosManuales.CAEFchVto is DateTime vto) dto.CAEFchVto = vto;
    //    }
    //    else
    //    {
    //        // Por si venís de AFIP en otro flujo
    //        dto.CAE = Factura.CAE;
    //        dto.CAEFchVto = Factura.CAEFchVto;
    //        dto.Numero = Factura.Numero;
    //    }

    //    return dto;
    //}













    protected CrearFacturaDto BuildCrearDtoDesdeFactura()
    {
        var dto = new CrearFacturaDto
        {
            Numero = Factura.Numero,
            PuntoVentaId = Factura.PuntoVentaId,
            PuntoVentaNumero = Factura.PuntoVentaNumero,
            PuntoVentaDescripcion = Factura.PuntoVentaDescripcion,
            TipoComprobanteId = Factura.TipoComprobanteId,
            Fecha = Factura.Fecha,
            PersonaId = Factura.PersonaId,
            Observaciones = Factura.Observaciones,

            CbteTipo = Factura.CbteTipo,
            DocTipo = Factura.DocTipo,
            DocNro = Factura.DocNro,
            MonId = Factura.MonId,
            MonCotiz = Factura.MonCotiz,
            Concepto = Factura.Concepto,

            FormaVenta = Factura.FormaVenta,
            UsuarioId = UsuarioId,
            // …campos de cabecera…
            Neto = Factura.Neto,
            Iva = Factura.Iva,
            Total = Factura.Total,
            Detalles = Detalles.Select(d => new CrearFacturaDetalleDto
            {
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                IvaAlicuotaId = d.IvaAlicuotaId,
                PresupuestoDetalleId = d.PresupuestoDetalleId
            }).ToList(),


            // 👇 Vincular por renglón
            VinculosPresupuesto = Detalles
            .Where(l => l.PresupuestoDetalleId.HasValue && l.Cantidad > 0)
            .Select(l => new FacturaPresupuestoDto
            {
                PresupuestoId = Factura.PresupuestoIds?.FirstOrDefault() ?? 0, // o guarda el id por línea si manejás varios
                PresupuestoDetalleId = l.PresupuestoDetalleId,
                CantidadFacturada = l.Cantidad,
                PrecioUnitarioFacturado = l.PrecioUnitario,
                // MontoFacturado: el backend lo recalcula; podés mandar 0
            })
            .ToList(),

            // 👇 Evitar el camino legacy
            PresupuestoIds = new()
        };

        // overrides manuales… (igual que ya tenés)

        if (CargarDatosFiscalesManuales)
        {
            if (DatosManuales.PuntoVentaNumero is int pv) dto.PuntoVentaNumero = pv;
            if (DatosManuales.Numero is int nro) dto.Numero = nro;
            if (DatosManuales.Fecha is DateTime fch) dto.Fecha = fch;
            if (!string.IsNullOrWhiteSpace(DatosManuales.CAE)) dto.CAE = DatosManuales.CAE;
            if (DatosManuales.CAEFchVto is DateTime vto) dto.CAEFchVto = vto;
        }
        else
        {
            // Por si venís de AFIP en otro flujo
            dto.CAE = Factura.CAE;
            dto.CAEFchVto = Factura.CAEFchVto;
            dto.Numero = Factura.Numero;
        }
        return dto;
    }





























    protected async Task FacturarConAfip()
    {
        await PrepararFacturaParaAfip();

        var jsonDebug = System.Text.Json.JsonSerializer.Serialize(Factura, new JsonSerializerOptions { WriteIndented = true });
        System.Diagnostics.Debug.WriteLine("📦 Factura lista para AFIP:\n" + jsonDebug);

        if (Factura.DocTipo == 80 || Factura.DocTipo == 96)
        {
            if (string.IsNullOrWhiteSpace(Factura.DocNro) || !Factura.DocNro.All(char.IsDigit))
                throw new InvalidOperationException("Documento del receptor inválido para AFIP.");
        }


        try
        {

            if (RequiereComprobanteAsociado)
            {
                if (!Factura.CbteAsocTipo.HasValue ||
                    !Factura.CbteAsocPtoVta.HasValue ||
                    !Factura.CbteAsocNro.HasValue)
                {
                    throw new InvalidOperationException("Debés seleccionar el comprobante asociado.");
                }
            }

            // 🔹 Solicitar CAE
            var cae = await AfipFacturaService.SolicitarCaeAsync(Factura);
            Factura.CAE = cae.CAE;
            Factura.CAEFchVto = cae.CAEFchVto;
            Factura.Numero = (int)cae.UltimoComprobante;
            Factura.CondicionIVAReceptorId = cae.CondicionIVAReceptorId;


            //  NUEVO: guardar en DB y actualizar estado de presupuestos
            var crear = BuildCrearDtoDesdeFactura();
            await FacturaService.CrearSoloDbAsync(crear);
            // 🔹 Guardar en DB (ya con CAE asignado)
            //await FacturaService.CrearSoloDbAsync(Factura);

            FacturaCreadaExito = true;
            MensajeExito = $"Factura creada en AFIP con éxito. CAE: {cae.CAE} " +
                           $"{(cae.CAEFchVto.HasValue ? $"(Vto: {cae.CAEFchVto.Value:dd/MM/yyyy})" : "(Sin vencimiento)")}";
            await Toast($"Factura creada en AFIP con éxito. CAE: {cae.CAE}" +
                        $"{(cae.CAEFchVto.HasValue ? $"(Vto: {cae.CAEFchVto.Value:dd/MM/yyyy})" : "(Sin vencimiento)")}", "success");

            //  Reset para siguiente factura
            //  LISTO PARA LA SIGUIENTE (conservar PV y moneda; cliente a elección)
            ResetFactura(mantenerCliente: false, mantenerPtoVta: true);



            StateHasChanged();
        }
        catch (Exception ex)
        {
            FacturaCreadaExito = false;
            MensajeExito = $"❌ Error al crear factura: {ex.Message}";
            await Toast($"❌ Error al crear factura: {ex.Message}", "error");
        }
    }




    protected void Cancelar() => Nav.NavigateTo("/facturas");
}
