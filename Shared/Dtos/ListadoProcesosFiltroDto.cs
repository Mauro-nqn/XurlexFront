namespace IurixBlazor.Shared.Dtos
{
    public class ListadoProcesosFiltroDto
    {
        // Core (5-6)
        public int? ResponsableId { get; set; }
        public int? GrupoId { get; set; }

        // Judicial (radicación)
        public int? JurisdiccionId { get; set; }
        public int? CircunscripcionId { get; set; }
        public int? JuzgadoId { get; set; }
        public int? SecretariaId { get; set; }

        // Tipo de Proceso (Judicial)
        public int? TipoProcesoId { get; set; }

        // Multi-estado (Judicial y/o Extrajudicial)
        public List<int> EstadoIds { get; set; } = new();

        // Texto (carátula contiene...)
        public string? CaratulaContiene { get; set; }

        // Demandado (depende tu modelo de Partes; ver nota abajo)
        //public string? DemandadoContiene { get; set; }

        public string? ParteNombreContiene { get; set; }     // texto libre
        public int? CaracterIntervencionId { get; set; }     // opcional: Demandado/Actor/etc.
        public int? PersonaParteId { get; set; }             // opcional: filtrar por persona exacta

        // (Opcional) limitar por tipo de gestión
        public string? TipoGestion { get; set; } // "Judicial"|"Extrajudicial"|null

        // (Opcional) paginado
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 30;
    }

}
