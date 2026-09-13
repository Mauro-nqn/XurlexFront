using IurixBlazor.Pages;
using IurixBlazor.Shared.Enums;

namespace IurixBlazor.Shared.Dtos
{
    public class FacturaDto
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public int PuntoVentaId { get; set; }
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
        public string? PersonaDomicilio { get; set; }

        public string? PersonaProvincia { get; set; }

        public int CondicionIVAReceptorId { get; set; } // id interno base datos

        public int? CondicionIvaReceptorCodigoAfip { get; set; } //  código AFIP

        public decimal Neto { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }        
        
        public string? Observaciones { get; set; }

        // AFIP
        public int CbteTipo { get; set; }
        public int DocTipo { get; set; }
        public string? DocNro { get; set; }
        public string MonId { get; set; } = "PES";
        public decimal MonCotiz { get; set; } = 1m;
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

        //En AFIP, una nota de crédito/débito debe incluir el comprobante de referencia
        public int? CbteReferenciaTipo { get; set; }
        public int? CbteReferenciaNumero { get; set; }

        //  Nuevos campos AFIP
        public string CAE { get; set; } = string.Empty;
        public DateTime? CAEFchVto { get; set; }

        // Detalles
        public List<FacturaDetalleDto> Detalles { get; set; } = new List<FacturaDetalleDto>();
        public List<AlicIvaDto> IvaDiscriminado { get; set; } = new List<AlicIvaDto>();

        //  NUEVO: Solo para cálculo y envío a AFIP
        public List<AlicIvaAfipDto> IvaDiscriminadoAfip { get; set; } = new();

        // Para vincular presupuestos (simple: ids)
        public List<int> PresupuestoIds { get; set; } = new();

        // (opcional) granular
        public List<FacturaPresupuestoDto>? VinculosPresupuesto { get; set; }
    }
}
