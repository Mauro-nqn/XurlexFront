namespace IurixBlazor.Shared.Dtos
{
    public class ArchivoMovimientoDto
    {
        public int Id { get; set; }

        public int MovimientoId { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string TipoMime { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;


        public DateTime FechaSubida { get; set; }

        public int? UsuarioId { get; set; }
        public string? UsuarioNombre { get; set; } = string.Empty;
    }
}
