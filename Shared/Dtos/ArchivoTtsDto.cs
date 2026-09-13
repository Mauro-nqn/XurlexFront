namespace IurixBlazor.Shared.Dtos
{
    public class ArchivoTtsDto
    {
        public int ArchivoId { get; set; }
        public bool Ok { get; set; }
        public string Texto { get; set; } = "";
        public bool EsOcr { get; set; }
        public string? Error { get; set; }
    }

}
