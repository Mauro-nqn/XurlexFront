namespace IurixBlazor.Shared.Dtos
{
    public class ProcuracionRunResultDto
    {
        public int ProcesoJudicialId { get; set; }

        // Para mostrar en UI
        public string ExpedienteUi { get; set; } = "";   // ej: "EXP 546043/2021"
        public string Caratula { get; set; } = "";

        // Métricas
        public int Nuevos { get; set; }                  // cuántas actuaciones nuevas se agregaron
        public int Total { get; set; }                   // total vistas en Dextra en este run

        // Útil para debug/descargas
        public List<string> ArchivosGuardados { get; set; } = new();
    }
}
