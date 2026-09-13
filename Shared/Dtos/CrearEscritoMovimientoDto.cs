namespace IurixBlazor.Shared.Dtos
{
    public class CrearEscritoMovimientoDto
    {
        public int MovimientoId { get; set; }

        public byte[] ContenidoComprimido { get; set; } = Array.Empty<byte>();

        public int UsuarioId { get; set; }
    }
}
