namespace IurixBlazor.Shared.Dtos
{
    public class CrearParteProcesoDto
    {
        public int? ProcesoJudicialId { get; set; }
        public int? ProcesoExtrajudicialId { get; set; }

        public int PersonaId { get; set; }
        public int CaracterIntervencionId { get; set; }
    }
}
