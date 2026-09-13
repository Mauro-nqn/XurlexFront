namespace IurixBlazor.Shared.Dtos
{
    public class FeCAEResponseDto
    {
        public string CAE { get; set; } = string.Empty;
        public DateTime? CAEFchVto { get; set; }
        public long UltimoComprobante { get; set; }
        public int CondicionIVAReceptorId { get; set; }
    }
}
