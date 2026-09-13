namespace IurixBlazor.Shared.Dtos
{
    public class DextraVerifyResultDto
    {
        public bool Ok { get; set; }
        public string? Note { get; set; }
        public DateTimeOffset CheckedAt { get; set; }
    }
}
