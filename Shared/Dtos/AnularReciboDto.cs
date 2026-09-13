namespace IurixBlazor.Shared.Dtos
{
    public class AnularReciboDto
    {
        public int UsuarioId { get; set; }
        public string? Motivo { get; set; }

        public bool ForzarDesimputar { get; set; }  // nuevo
    }

}
