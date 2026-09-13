namespace IurixBlazor.Shared.Dtos
{
    public class CrearEmailConAdjuntoDto
    {
        public string Destinatario { get; set; } = "";
        public string Asunto { get; set; } = "";
        public string CuerpoHtml { get; set; } = "";
        public List<AdjuntoDto> Adjuntos { get; set; } = new();
    }
}
