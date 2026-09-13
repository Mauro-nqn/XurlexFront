namespace IurixBlazor.Shared.Dtos
{
    public class ParteProcesoDto
    {
        public int Id { get; set; }

        public int PersonaId { get; set; }
        public string NombrePersona { get; set; } = string.Empty;

        public int CaracterIntervencionId { get; set; }
        public string NombreCaracter { get; set; } = string.Empty;
    }
}
