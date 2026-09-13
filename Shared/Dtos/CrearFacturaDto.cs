using IurixBlazor.Shared.Enums;

namespace IurixBlazor.Shared.Dtos
{
    public class CrearFacturaDto
    {
        public int Numero { get; set; }
        //public int Prefijo { get; set; }
        public int PuntoVentaId { get; set; } //el que usamos prefijo de factura

        public int PuntoVentaNumero { get; set; }
        public string? PuntoVentaDescripcion { get; set; }

        public int TipoComprobanteId { get; set; }
        public string? TipoComprobanteDescripcion { get; set; }
        public string? TipoComprobanteLetra { get; set; }
        public DateTime Fecha { get; set; }
        public int PersonaId { get; set; }
        public string? PersonaNombre { get; set; }
        public string? PersonaApellido { get; set; }
        public string? PersonaRazonSocial { get; set; }
        public string? PersonaCUIT { get; set; }
        public string? Observaciones { get; set; }
        public int CondicionIVAReceptorId { get; set; } // id interno base datos
        public int? CondicionIvaReceptorCodigoAfip { get; set; } //  código AFIP

        // AFIP
        public int CbteTipo { get; set; }
        public int DocTipo { get; set; }
        public string? DocNro { get; set; }
        public string MonId { get; set; } = "PES";
        public decimal MonCotiz { get; set; } = 1;
        public ConceptoFactura Concepto { get; set; } = ConceptoFactura.Servicio;

        // Comprobante asociado para Nota de Crédito / Nota de Débito
        public int? CbteAsocTipo { get; set; }
        public int? CbteAsocPtoVta { get; set; }
        public long? CbteAsocNro { get; set; }
        public string? CbteAsocCuit { get; set; }
        public DateTime? CbteAsocFecha { get; set; }


        public FormaVenta FormaVenta { get; set; } = FormaVenta.Contado;

        //  NUEVO: Usuario emisor
        public int UsuarioId { get; set; }

        public string? CAE { get; set; } = string.Empty;
        public DateTime? CAEFchVto { get; set; }

        public decimal Neto { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }

        // Detalles
        public List<CrearFacturaDetalleDto> Detalles { get; set; } = new List<CrearFacturaDetalleDto>();
        public List<AlicIvaDto> IvaDiscriminado { get; set; } = new List<AlicIvaDto>();


        // vinculación presupuesto
        public List<int> PresupuestoIds { get; set; } = new(); // simple
        public List<FacturaPresupuestoDto>? VinculosPresupuesto { get; set; } // opcional granular

        public List<AlicIvaAfipDto> IvaDiscriminadoAfip { get; set; } = new();
    }
}
