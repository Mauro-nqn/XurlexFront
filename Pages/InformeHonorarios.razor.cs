using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using System.Text;





    public class InformeHonorariosBase : ComponentBase
    {
    [Inject] protected InformeHonorariosService HonorariosService { get; set; } = default!;

    [Inject] protected UsuarioService UsuarioService { get; set; } = default!;

    [Inject] protected IJSRuntime JS { get; set; } = default!;

    public bool VerDashboard = false;
    public List<HonorarioMensualDto> DashboardMensual = new();
    public List<HonorarioMensualProfesionalDto> DashboardMensualProfesional = new();


    public InformeHonorariosFiltroDto Filtro = new()
    {
        FechaDesde = DateTime.Today.AddMonths(-1),
        FechaHasta = DateTime.Today,
        IncluirIngresosSinImputar = true
    };

    public List<LineaHonorarioDto>? Lineas;

    public List<UsuarioDto> Abogados = new();
    public int? UsuarioSeleccionadoId;

    protected override async Task OnInitializedAsync()
    {
        // Cargar lista de profesionales para el combo
        Abogados = await UsuarioService.ObtenerUsuariosAsync();
    }

    public async Task Buscar()
    {
        // Si hay profesional seleccionado, lo pasamos al filtro; si no, null = todos
        Filtro.AbogadoId = UsuarioSeleccionadoId;

        Lineas = await HonorariosService.ObtenerInformeAsync(Filtro);
    }


    public async Task ExportarExcel()
    {
        if (Lineas is null || !Lineas.Any())
            return;

        var sb = new StringBuilder();

        // Cabecera
        sb.AppendLine("Fecha,Recibo,Cliente,Profesional,Presupuesto,ImporteImputado,HonorariosProfesional");

        foreach (var l in Lineas.OrderBy(l => l.FechaCredito))
        {
            string fecha = l.FechaCredito.ToString("yyyy-MM-dd");
            string recibo = l.ReciboId?.ToString() ?? "";
            string cliente = EscapeCsv(l.ClienteNombre);
            string profesional = EscapeCsv(l.AbogadoNombre ?? "");
            string presupuesto = EscapeCsv($"{l.PresupuestoId?.ToString() ?? ""} {l.NombrePresupuesto ?? ""}");
            string importeImputado = l.ImporteImputado.ToString("0.00", CultureInfo.InvariantCulture);
            string honorarioProf = l.ImporteParaAbogado.ToString("0.00", CultureInfo.InvariantCulture);

            sb.AppendLine($"{fecha},{recibo},{cliente},{profesional},{presupuesto},{importeImputado},{honorarioProf}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var base64 = Convert.ToBase64String(bytes);

        await JS.InvokeVoidAsync("downloadFileFromBytes", "honorarios.csv", base64);
    }

    public static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // Si tiene comas, comillas o saltos de línea, encapsular entre comillas dobles
        bool mustQuote = value.Contains(',') || value.Contains('"') || value.Contains('\n');
        if (mustQuote)
        {
            value = value.Replace("\"", "\"\""); // doblar comillas
            return $"\"{value}\"";
        }
        return value;
    }


    public async Task ImprimirPdf()
    {
        await JS.InvokeVoidAsync("printDiv", "panelInforme", "Informe de honorarios");
    }


    public void MostrarDashboard()
    {
        if (Lineas is null || !Lineas.Any())
            return;

        // Apagamos si ya estaba, para togglear
        VerDashboard = !VerDashboard;
        if (!VerDashboard)
            return;

        // 1) Tomamos solo líneas con distribución válida (honorarios asignados)
        var lineasVal = Lineas
            .Where(l => !l.EsIngresoSinImputar &&
                        !l.EsSinDistribucion &&
                        l.AbogadoId != null)
            .ToList();

        // 2) Resumen mensual total (sumando todos los profesionales)
        DashboardMensual = lineasVal
            .GroupBy(l => new { l.FechaCredito.Year, l.FechaCredito.Month })
            .Select(g => new HonorarioMensualDto
            {
                Anio = g.Key.Year,
                Mes = g.Key.Month,
                TotalHonorarios = g.Sum(x => x.ImporteParaAbogado)
            })
            .OrderBy(x => x.Anio).ThenBy(x => x.Mes)
            .ToList();

        // 3) Resumen mensual por profesional
        DashboardMensualProfesional = lineasVal
            .GroupBy(l => new { l.FechaCredito.Year, l.FechaCredito.Month, l.AbogadoId, l.AbogadoNombre })
            .Select(g => new HonorarioMensualProfesionalDto
            {
                Anio = g.Key.Year,
                Mes = g.Key.Month,
                AbogadoId = g.Key.AbogadoId,
                AbogadoNombre = g.Key.AbogadoNombre ?? "(sin nombre)",
                TotalHonorarios = g.Sum(x => x.ImporteParaAbogado)
            })
            .OrderBy(x => x.Anio).ThenBy(x => x.Mes).ThenBy(x => x.AbogadoNombre)
            .ToList();
    }
}

