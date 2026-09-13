namespace IurixBlazor.Shared.Dtos
{
    public class ProcuracionRunAllResultDto
    {
        public int Procesados { get; set; }
        public int ConNovedades { get; set; }
        public int SinNovedades { get; set; }
        public List<(int ProcesoJudicialId, string Mensaje)> Errores { get; set; } = new();
    }
}
