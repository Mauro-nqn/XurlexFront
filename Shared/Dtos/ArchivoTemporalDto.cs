namespace IurixBlazor.Shared.Dtos
{
    //public class ArchivoTemporalDto
    //{
    //    public string Nombre { get; set; } = "";
    //    public string TipoMime { get; set; } = "";
    //    public string Extension { get; set; } = "";
    //    public byte[] Contenido { get; set; } = Array.Empty<byte>();
    //}

    public class ArchivoTemporalDto
    {
        public int? Id { get; set; } = 0; // Cuando se carga desde BD
        public int MovimientoId { get; set; } = 0;

        public string Nombre { get; set; } = "";
        public string TipoMime { get; set; } = "";
        public string Extension { get; set; } = "";

        public DateTime? FechaSubida { get; set; } // Puede no estar cargada aún

        public int? UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = "";

        public byte[] Contenido { get; set; } = Array.Empty<byte>(); // Si es archivo nuevo desde el input

        //public bool EsTemporal => Contenido?.Length > 0; // Útil para lógica de descarga o guardado

        public bool ContenidoCargado => Contenido?.Length > 0;

        // Precalculado para mostrar vista previa
        public string? DataUrlBase64 { get; set; }
    }

}
