namespace IurixBlazor.Shared.Dtos
{
    public class ProcesoListadoItemDto
    {
        public int GestionId { get; set; }
        public string TipoGestion { get; set; } = "";

        public int ResponsableId { get; set; }
        public string ResponsableNombre { get; set; } = "";

        public int? GrupoId { get; set; }
        public string? GrupoNombre { get; set; }

        // “Identidad” del proceso
        public int? ProcesoJudicialId { get; set; }
        public int? ProcesoExtrajudicialId { get; set; }

        public string? Caratula { get; set; }
        public string? NumeroExpediente { get; set; }
        public string? TipoProcesoNombre { get; set; }

        public int? EstadoId { get; set; }
        public string? EstadoNombre { get; set; }

        public string? Radicacion { get; set; } // Ej: "Juris / Circ / Juz / Sec"
        public string? DemandadoResumen { get; set; } // si lo querés mostrar
    }

}
