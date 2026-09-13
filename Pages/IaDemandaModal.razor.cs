using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Iademandas;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;

public class IaDemandaModalBase : ComponentBase
{
    public bool Visible { get; set; }

    public DemandaEjecutivaRequestDto modelo { get; set; } = new DemandaEjecutivaRequestDto
    {
        Abogado = new AbogadoDto(),
        Cliente = new ParteDto(),
        Deudor = new ParteDto(),
        Certificado = new CertificadoDeudaDto()
    };

    [Parameter] public EventCallback<string> OnDemandaGenerada { get; set; }

    [Inject] public IHttpClientFactory HttpClientFactory { get; set; } = default!;
    [Inject] public GestionService GestionService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    [Inject] public ProcesoJudicialService ProcesoJudicialService { get; set; } = default!;


    [Inject] public UsuarioService UsuarioService { get; set; } = default!;
    [Inject] public DomicilioService DomicilioService { get; set; } = default!;
    [Inject] public ParteProcesoService PartesService { get; set; } = default!;
    [Inject] public PersonaService PersonaService { get; set; } = default!;

    [Inject] public CertificadoApremioService CertificadoApremioService { get; set; } = default!;


    protected IBrowserFile? CertificadoArchivo;
    protected string? CertificadoMensaje;

    protected int? ProcesoJudicialId { get; set; }

    protected HttpClient Api => HttpClientFactory.CreateClient("Api");



    private static DateTime? ParseFecha(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        if (DateTime.TryParseExact(
                valor,
                "yyyy-MM-dd",                 // formato que usás en el modelo
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dt))
        {
            return dt;
        }

        return null;
    }


    // 👇 ahora recibimos el ID de la GESTIÓN
    public async Task Abrir(int gestionId)
    {
        Visible = true;
        CertificadoMensaje = null;



        // 1) Traer gestión
        var gestion = await GestionService.ObtenerPorIdAsync(gestionId);
        if (gestion is null)
        {
            await JS.InvokeVoidAsync("mostrarToast", "No se encontró la gestión.", "error");
            Visible = false;
            StateHasChanged();
            return;
        }

        // 2) Abogado = Responsable (usuario)
        var usuarioResp = await UsuarioService.ObtenerUsuarioPorIdAsync(gestion.ResponsableId);
        modelo.Abogado ??= new AbogadoDto();

        if (usuarioResp != null)
        {
            modelo.Abogado.Nombre = usuarioResp.Nombre;
            modelo.Abogado.Apellido= usuarioResp.Apellido;
            modelo.Abogado.Matricula = usuarioResp.Matricula;
            modelo.Abogado.Cuit = usuarioResp.CUIT;
            modelo.Abogado.CondicionIva = usuarioResp.CondicionIvaNombre;
            var cargoTexto = usuarioResp.Cargo.ToString(); // 👈 enum → string

            modelo.Abogado.Cargo = string.IsNullOrWhiteSpace(cargoTexto)
                ? "abogado"
                : cargoTexto;
        }

        // 3) Determinar tipo de proceso
        bool esJudicial = gestion.ProcesoJudicial is not null;
        var pj = gestion.ProcesoJudicial;
        var pe = gestion.ProcesoExtrajudicial;

        int? procesoId = esJudicial ? pj?.Id : pe?.Id;
        if (procesoId is null)
        {
            await JS.InvokeVoidAsync("mostrarToast", "La gestión no tiene proceso asociado.", "error");
            return;
        }

        // 4) Domicilios desde ProcesoJudicial (solo si es judicial)
        if (esJudicial && pj is not null)
        {
            if (pj.DomicilioConstituidoId.HasValue)
            {
                var domProc = await DomicilioService.ObtenerPorIdAsync(pj.DomicilioConstituidoId.Value);
                if (domProc is not null)
                    modelo.Abogado.DomicilioProcesal = domProc.Descripcion;
                    modelo.Abogado.Ciudad = domProc.Ciudad;
                    modelo.Abogado.Provincia = domProc.Provincia;
            }

            if (pj.DomicilioElectronicoId.HasValue)
            {
                var domElec = await DomicilioService.ObtenerPorIdAsync(pj.DomicilioElectronicoId.Value);
                if (domElec is not null)
                    modelo.Abogado.DomicilioElectronico = domElec.Descripcion;
            }
        }

        // 5) Partes del proceso
        List<ParteProcesoDto> partes = esJudicial
            ? await PartesService.ObtenerPorProcesoJudicialAsync(procesoId.Value)
            : await PartesService.ObtenerPorProcesoExtrajudicialAsync(procesoId.Value);

        var actores = partes
            .Where(p => p.NombreCaracter.Equals("Actor", StringComparison.OrdinalIgnoreCase)
                     || p.NombreCaracter.Equals("Acreedor", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var demandados = partes
            .Where(p => p.NombreCaracter.Equals("Demandado", StringComparison.OrdinalIgnoreCase)
                     || p.NombreCaracter.Equals("Deudor", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // 6) Cliente (actor)
        modelo.Cliente ??= new ParteDto();
        var actor = actores.FirstOrDefault();
        if (actor != null)
        {
            var personaActor = await PersonaService.ObtenerPersonaPorIdAsync(actor.PersonaId);

            modelo.Cliente.Nombre = actor.NombrePersona;
            if (personaActor != null)
            {
                modelo.Cliente.Domicilio = personaActor.Domicilio;
                modelo.Cliente.Ciudad = personaActor.Ciudad;
                modelo.Cliente.Provincia = personaActor.Provincia;
                modelo.Cliente.Cuit = personaActor.CUIT;
                modelo.Cliente.Dni = personaActor.DNI;
                // si más adelante agregás Ciudad / Provincia
            }
        }

        // 7) Deudor (demandado)
        modelo.Deudor ??= new ParteDto();
        var demandado = demandados.FirstOrDefault();
        if (demandado != null)
        {
            var personaDem = await PersonaService.ObtenerPersonaPorIdAsync(demandado.PersonaId);

            modelo.Deudor.Nombre = demandado.NombrePersona;
            if (personaDem != null)
            {
                modelo.Deudor.Domicilio = personaDem.Domicilio;
                modelo.Deudor.Ciudad = personaDem.Ciudad;
                modelo.Deudor.Provincia = personaDem.Provincia;
                modelo.Deudor.Cuit = personaDem.CUIT;
                modelo.Deudor.Dni = personaDem.DNI;
            }
        }

        // 8) Certificado: valores por defecto
        modelo.Certificado ??= new CertificadoDeudaDto
        {
            Monto = 0m,
            FechaCertificado = DateTime.Today.ToString("yyyy-MM-dd"),
            FechaLiquidacion = DateTime.Today.ToString("yyyy-MM-dd"),
            OrganismoEmisor = "Subsecretaría de Administración Municipal de Ingresos Públicos",
            Dependencia = "Secretaría de Economía y Hacienda de la Municipalidad de Neuquén"
        };


        // Cuando determinás que es judicial:
        if (esJudicial && gestion.ProcesoJudicial != null)
        {
            ProcesoJudicialId = gestion.ProcesoJudicial.Id;
        }

        StateHasChanged();
    }

    //protected async Task Generar()
    //{
    //    try
    //    {
    //        if (ProcesoJudicialId.HasValue)
    //        {
    //            var updateDto = new ActualizarCertificadoProcesoJudicialDto
    //            {
    //                NumeroCertificadoDeuda = modelo.Certificado.Numero,
    //                MontoDemanda = modelo.Certificado.Monto,
    //                FechaEmisionCertificado = ParseFecha(modelo.Certificado.FechaCertificado),
    //                FechaLiquidacion = ParseFecha(modelo.Certificado.FechaLiquidacion)
    //            };

    //            await ProcesoJudicialService.ActualizarCertificadoAsync(
    //                ProcesoJudicialId.Value,
    //                updateDto);
    //        }





    //        var resp = await Api.PostAsJsonAsync("api/ia/demandas/ejecutiva", modelo);

    //        if (resp.IsSuccessStatusCode)
    //        {
    //            var dto = await resp.Content.ReadFromJsonAsync<DemandaEjecutivaResponseDto>();

    //            if (dto != null && !string.IsNullOrWhiteSpace(dto.BorradorDemanda))
    //            {
    //                // 👉 Avisamos al editor que inserte el texto generado
    //                await OnDemandaGenerada.InvokeAsync(dto.BorradorDemanda);
    //            }
    //            else
    //            {
    //                await JS.InvokeVoidAsync("mostrarToast",
    //                    "La IA no devolvió contenido para la demanda.", "error");
    //            }
    //        }
    //        else
    //        {
    //            var txt = await resp.Content.ReadAsStringAsync();
    //            await JS.InvokeVoidAsync("mostrarToast", $"Error IA:\n{txt}", "error");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        await JS.InvokeVoidAsync("mostrarToast",
    //            $"Error al generar la demanda: {ex.Message}", "error");
    //    }
    //    finally
    //    {
    //        Cerrar();
    //    }
    //}

    protected async Task Generar()
    {
        try
        {
            // 1) Actualizar datos del certificado en el Proceso Judicial
            if (ProcesoJudicialId.HasValue)
            {
                var updateDto = new ActualizarCertificadoProcesoJudicialDto
                {
                    NumeroCertificadoDeuda = modelo.Certificado.Numero,
                    MontoDemanda = modelo.Certificado.Monto,
                    FechaEmisionCertificado = ParseFecha(modelo.Certificado.FechaCertificado),
                    FechaLiquidacion = ParseFecha(modelo.Certificado.FechaLiquidacion)
                };

                try
                {
                    await ProcesoJudicialService.ActualizarCertificadoAsync(
                        ProcesoJudicialId.Value,
                        updateDto);

                    //  Avisar que se actualizaron los datos del certificado
                    await JS.InvokeVoidAsync("mostrarToast",
                        "Datos del certificado actualizados en el proceso judicial.",
                        "success");
                }
                catch (Exception exAct)
                {
                    //  No cortamos la generación de la demanda, solo avisamos
                    await JS.InvokeVoidAsync("mostrarToast",
                        $"No se pudieron actualizar los datos del certificado en el proceso judicial: {exAct.Message}",
                        "warning");
                }
            }

            // 2) Generar la demanda con IA
            var resp = await Api.PostAsJsonAsync("api/ia/demandas/ejecutiva", modelo);

            if (resp.IsSuccessStatusCode)
            {
                var dto = await resp.Content.ReadFromJsonAsync<DemandaEjecutivaResponseDto>();

                if (dto != null && !string.IsNullOrWhiteSpace(dto.BorradorDemanda))
                {
                    await OnDemandaGenerada.InvokeAsync(dto.BorradorDemanda);
                }
                else
                {
                    await JS.InvokeVoidAsync("mostrarToast",
                        "La IA no devolvió contenido para la demanda.",
                        "error");
                }
            }
            else
            {
                var txt = await resp.Content.ReadAsStringAsync();
                await JS.InvokeVoidAsync("mostrarToast", $"Error IA:\n{txt}", "error");
            }
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("mostrarToast",
                $"Error al generar la demanda: {ex.Message}", "error");
        }
        finally
        {
            Cerrar();
        }
    }




    // 📌 Selección de archivo
    protected void OnCertificadoPdfSeleccionado(InputFileChangeEventArgs e)
    {
        CertificadoArchivo = e.File;
        CertificadoMensaje = $"Archivo seleccionado: {CertificadoArchivo?.Name}";
    }

    // 📌 Llamar al backend para parsear + guardar en ProcesoJudicial
    protected async Task ProcesarCertificadoPdf()
    {
        if (CertificadoArchivo is null)
        {
            CertificadoMensaje = "Primero seleccione un archivo PDF.";
            StateHasChanged();
            return;
        }

        if (ProcesoJudicialId is null)
        {
            CertificadoMensaje = "No se determinó proceso judicial para asociar el certificado.";
            StateHasChanged();
            return;
        }

        try
        {
            var dto = await CertificadoApremioService.ParseAsync(CertificadoArchivo, ProcesoJudicialId.Value);

            if (dto != null)
            {
                // rellenar el modelo del modal
                if (!string.IsNullOrWhiteSpace(dto.Numero))
                    modelo.Certificado.Numero = dto.Numero;

                if (dto.Monto.HasValue)
                    modelo.Certificado.Monto = dto.Monto.Value;

                if (dto.FechaEmision.HasValue)
                    modelo.Certificado.FechaCertificado = dto.FechaEmision.Value.ToString("yyyy-MM-dd");

                if (dto.FechaLiquidacion.HasValue)
                    modelo.Certificado.FechaLiquidacion = dto.FechaLiquidacion.Value.ToString("yyyy-MM-dd");

                // opcional: guardar el monto en letras que viene del certificado
                if (!string.IsNullOrWhiteSpace(dto.MontoEnLetras))
                    modelo.Certificado.TextoCompleto = dto.MontoEnLetras; // o un campo que elijas

                CertificadoMensaje = "Certificado leído y asociado al proceso correctamente.";
            }
            else
            {
                CertificadoMensaje = "No se pudieron leer datos del certificado.";
            }
        }
        catch (Exception ex)
        {
            CertificadoMensaje = $"Error al procesar certificado: {ex.Message}";
        }

        StateHasChanged();
    }





    public void Cerrar()
    {
        Visible = false;
        StateHasChanged();
    }



}



