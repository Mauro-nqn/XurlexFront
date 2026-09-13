namespace IurixBlazor.Shared.Dtos
{
    public class CrearEmailDto
    {
        public string Destinatario { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string Cuerpo { get; set; } = string.Empty;
    }
}
