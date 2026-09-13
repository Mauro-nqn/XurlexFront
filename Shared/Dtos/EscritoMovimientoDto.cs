namespace IurixBlazor.Shared.Dtos
{
    public class EscritoMovimientoDto
    {
        public int Id { get; set; }

        public int MovimientoId { get; set; }

        public byte[] ContenidoComprimido { get; set; } = Array.Empty<byte>();

        public DateTime FechaCreacion { get; set; }

        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
    }
}
