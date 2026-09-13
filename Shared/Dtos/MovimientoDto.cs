namespace IurixBlazor.Shared.Dtos
{
    public class MovimientoDto
    {
        public int Id { get; set; }

        public int? ProcesoJudicialId { get; set; }
        public int? ProcesoExtrajudicialId { get; set; }

        public string Detalle { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }

        public string? DescripcionManual { get; set; }
        public string? Titulo { get; set; }

        public int? EscritoBaseId { get; set; }
        public string? EscritoBaseTitulo { get; set; }

        public bool TieneEscritoPersonalizado { get; set; }
        public int CantidadArchivos { get; set; }

        public int? AgendaId { get; set; }

       
        //  HTML del escrito personalizado
        public string? EscritoPersonalizadoHtml { get; set; }
        public string? EscritoPersonalizadoTitulo { get; set; }
        public int? UsuarioId { get; set; }

        //  Nuevo: archivos adjuntos
        public List<ArchivoMovimientoDto> Archivos { get; set; } = new();


        public bool TieneAgenda { get; set; }
        public DateTime? AgendaFechaAgendadaUtc { get; set; }  // opcional, por si querés tooltip


        public bool AgendaGoogleRegistrado { get; set; } = false;
        public string? AgendaGoogleHtmlLink { get; set; }
        public DateTimeOffset? AgendaGoogleLastSyncUtc { get; set; }


    }
}
