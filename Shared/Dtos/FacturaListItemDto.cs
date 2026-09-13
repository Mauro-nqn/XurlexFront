namespace IurixBlazor.Shared.Dtos
{
    public class FacturaListItemDto
    {
        public int Id { get; set; }

        // Números crudos; armás el "0001-00001234" en el front
        public int Numero { get; set; }
        public int? PuntoVentaId { get; set; }
        public int? PuntoVentaNumero { get; set; }
        public string? PuntoVentaDescripcion { get; set; }

        public int CbteTipo { get; set; }
        public string? TipoComprobanteDescripcion { get; set; }
        public string? TipoComprobanteLetra { get; set; }

        public int? PersonaId { get; set; }
        public string? PersonaNombre { get; set; }
        public string? PersonaApellido { get; set; }
        public string? PersonaRazonSocial { get; set; }

        public int? UsuarioId { get; set; }
        public string? UsuarioNombre { get; set; }   // o Email/Username

        public DateTime Fecha { get; set; } // UTC
        public decimal Total { get; set; }
    }

}
