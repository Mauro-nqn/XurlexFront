namespace IurixBlazor.Shared.Dtos
{
    public class MarcarEnviadoDto
    {
        public bool Ok { get; set; } = true;     // por si querés forzar false en algún caso
        public string? Proveedor { get; set; }   // opcional: "gmail"
        public string? MessageId { get; set; }   // opcional: si más adelante lo tenés
    }
}
