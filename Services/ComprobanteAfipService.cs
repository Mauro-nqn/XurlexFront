namespace IurixBlazor.Services
{
    //public class ComprobanteAfipService
    //{
    //    public (int CbteTipo, string Descripcion) DeterminarComprobante(int codIvaEmisor, int codIvaReceptor, bool esNotaCredito = false, bool esNotaDebito = false)
    //    {
    //        // Facturas estándar
    //        if (!esNotaCredito && !esNotaDebito)
    //        {
    //            if (codIvaEmisor == 1 && codIvaReceptor == 1)
    //                return (1, "Factura A");
    //            if (codIvaEmisor == 1 && codIvaReceptor == 5)
    //                return (6, "Factura B");
    //            return (11, "Factura C");
    //        }

    //        // Notas de crédito
    //        if (esNotaCredito)
    //        {
    //            if (codIvaEmisor == 1 && codIvaReceptor == 1)
    //                return (3, "Nota de Crédito A");
    //            if (codIvaEmisor == 1 && codIvaReceptor == 5)
    //                return (8, "Nota de Crédito B");
    //            return (13, "Nota de Crédito C");
    //        }

    //        // Notas de débito
    //        if (esNotaDebito)
    //        {
    //            if (codIvaEmisor == 1 && codIvaReceptor == 1)
    //                return (2, "Nota de Débito A");
    //            if (codIvaEmisor == 1 && codIvaReceptor == 5)
    //                return (7, "Nota de Débito B");
    //            return (12, "Nota de Débito C");
    //        }

    //        throw new InvalidOperationException("Tipo de comprobante no válido");
    //    }
    //}

    //public class ComprobanteAfipService
    //{
    //    private readonly TipoComprobanteService _tipoComprobanteService;

    //    public ComprobanteAfipService(TipoComprobanteService tipoComprobanteService)
    //    {
    //        _tipoComprobanteService = tipoComprobanteService;
    //    }

    //    public async Task<(int CbteTipo, string Descripcion, string Letra)> DeterminarComprobanteAsync(
    //        int codIvaEmisor, int codIvaReceptor, bool esNotaCredito = false, bool esNotaDebito = false)
    //    {
    //        int cbteTipo;

    //        // Facturas estándar
    //        if (!esNotaCredito && !esNotaDebito)
    //        {
    //            if (codIvaEmisor == 1 && codIvaReceptor == 1) cbteTipo = 1;   // Factura A
    //            else if (codIvaEmisor == 1 && codIvaReceptor == 5) cbteTipo = 6;  // Factura B
    //            else cbteTipo = 11; // Factura C
    //        }
    //        // Notas de crédito
    //        else if (esNotaCredito)
    //        {
    //            if (codIvaEmisor == 1 && codIvaReceptor == 1) cbteTipo = 3;   // NC A
    //            else if (codIvaEmisor == 1 && codIvaReceptor == 5) cbteTipo = 8;  // NC B
    //            else cbteTipo = 13; // NC C
    //        }
    //        // Notas de débito
    //        else if (esNotaDebito)
    //        {
    //            if (codIvaEmisor == 1 && codIvaReceptor == 1) cbteTipo = 2;   // ND A
    //            else if (codIvaEmisor == 1 && codIvaReceptor == 5) cbteTipo = 7;  // ND B
    //            else cbteTipo = 12; // ND C
    //        }
    //        else
    //        {
    //            throw new InvalidOperationException("Tipo de comprobante no válido");
    //        }

    //        // ✅ Obtener info del catálogo (API / BD)
    //        var tipoComprobante = await _tipoComprobanteService.ObtenerPorCodigoAsync(cbteTipo)
    //            ?? throw new InvalidOperationException($"No se encontró el tipo de comprobante con código {cbteTipo}");

    //        return (cbteTipo, tipoComprobante.Descripcion, tipoComprobante.Letra);
    //    }
    //}

    public class ComprobanteAfipService
    {
        private readonly TipoComprobanteService _tipoComprobanteService;

        // Ajustá estos conjuntos a tus códigos AFIP reales
        private static readonly HashSet<int> ReceptorGrupoA = new() { 1 /* RI */, 4 /* Exento? si corresponde A en tu lógica */, 6 /* Monotributo */ };
        private static readonly HashSet<int> ReceptorGrupoB = new() { 5 /* Consumidor Final */, /* otros si aplica */ };

        public ComprobanteAfipService(TipoComprobanteService tipoComprobanteService)
            => _tipoComprobanteService = tipoComprobanteService;

        public async Task<(int CbteTipo, string Descripcion, string Letra)> DeterminarComprobanteAsync(
            int codIvaEmisor,
            int codIvaReceptor,
            bool esNotaCredito = false,
            bool esNotaDebito = false,
            int? cbteBaseReferencia = null // opcional: si tenés una factura base seleccionada
        )
        {
            if (esNotaCredito && esNotaDebito)
                throw new InvalidOperationException("No puede ser Nota de Crédito y Débito a la vez.");

            int cbteTipo;

            if (esNotaCredito || esNotaDebito)
            {
                // Si te pasan una base explícita (1/6/11), usala; si no, calculá primero la factura base
                var baseTipo = cbteBaseReferencia ?? FacturaDe(codIvaEmisor, codIvaReceptor);
                cbteTipo = esNotaCredito ? NotaCreditoDe(baseTipo) : NotaDebitoDe(baseTipo);
            }
            else
            {
                cbteTipo = FacturaDe(codIvaEmisor, codIvaReceptor);
            }

            var tipoComprobante = await _tipoComprobanteService.ObtenerPorCodigoAsync(cbteTipo)
                ?? throw new InvalidOperationException($"No se encontró el tipo de comprobante con código {cbteTipo}");

            return (cbteTipo, tipoComprobante.Descripcion, tipoComprobante.Letra);
        }

        private static int FacturaDe(int codIvaEmisor, int codIvaReceptor)
        {
            var emisorEsRI = codIvaEmisor == 1;

            // Emisor NO es RI => siempre C
            if (!emisorEsRI)
                return 11; // Factura C

            // Emisor RI: decide por grupo del receptor
            if (ReceptorGrupoA.Contains(codIvaReceptor))
                return 1; // Factura A

            if (ReceptorGrupoB.Contains(codIvaReceptor))
                return 6; // Factura B

            // Fallback seguro para RI -> receptor “no mapeado”: B (ajustá si tu negocio requiere otra cosa)
            return 6;
        }

        private static int NotaCreditoDe(int baseCbte) => baseCbte switch
        {
            1 => 3,   // A -> NC A
            6 => 8,   // B -> NC B
            11 => 13,  // C -> NC C
            _ => throw new InvalidOperationException($"Tipo base {baseCbte} no soportado para NC.")
        };

        private static int NotaDebitoDe(int baseCbte) => baseCbte switch
        {
            1 => 2,   // A -> ND A
            6 => 7,   // B -> ND B
            11 => 12,  // C -> ND C
            _ => throw new InvalidOperationException($"Tipo base {baseCbte} no soportado para ND.")
        };
    }


}
