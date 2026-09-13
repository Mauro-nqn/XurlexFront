namespace IurixBlazor.Shared.Dtos
{
    public class AlicIvaDto
    {
        public int Id { get; set; }
        public int AlicuotaId { get; set; } // Código AFIP (5=21%, 4=10.5%, etc.)
        public decimal BaseImp { get; set; }
        public decimal Importe { get; set; }
    }
}
