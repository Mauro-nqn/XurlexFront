namespace IurixBlazor.Shared.Dtos
{
    public class CrearArchivoMovimientoDto
    {
        public int MovimientoId { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string TipoMime { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;

        public byte[] Contenido { get; set; } = Array.Empty<byte>();

        public int UsuarioId { get; set; }
    }
}
