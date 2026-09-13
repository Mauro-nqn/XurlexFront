namespace IurixBlazor.Shared.Dtos
{
    public class MovimientoTtsDto
    {
        public int MovimientoId { get; set; }
        public string Texto { get; set; } = string.Empty;

        public int CantidadAdjuntos { get; set; }


        public bool TieneEscrito { get; set; }
        public int? EscritoId { get; set; }
        public string? EscritoTitulo { get; set; }
    }
}
