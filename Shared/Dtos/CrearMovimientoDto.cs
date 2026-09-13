namespace IurixBlazor.Shared.Dtos
{
    public class CrearMovimientoDto
    {
        public int? ProcesoJudicialId { get; set; }
        public int? ProcesoExtrajudicialId { get; set; }

        public string Detalle { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public string? DescripcionManual { get; set; }
        public string? Titulo { get; set; }

        public int? EscritoBaseId { get; set; }

        public int? AgendaId { get; set; }

        //  HTML del escrito personalizado
        public string? EscritoPersonalizadoHtml { get; set; }
        public string? EscritoPersonalizadoTitulo { get; set; }
        public int UsuarioId { get; set; }

        //  Archivos adjuntos
        public List<CrearArchivoMovimientoDto>? Archivos { get; set; } = new();

        public List<int> ArchivosEliminarIds { get; set; } = new();

    }
}
