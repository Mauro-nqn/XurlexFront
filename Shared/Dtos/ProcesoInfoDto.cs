namespace IurixBlazor.Shared.Dtos
{
    public class ProcesoInfoDto
    {
        public int Id { get; set; }

        //Judicial o Extrajudicial
        public string? Tipo { get; set; }

        // Judicial
        public string? Caratula { get; set; }
        public string? NumeroExpediente { get; set; }

        public string? TipoProceso { get; set; }
        

        // Extrajudicial
        public string? Materia { get; set; }
        public string? TipoProcesoExt { get; set; }

        //  NUEVO: cliente asociado a la gestión
        public int? PersonaId { get; set; }
        public string PersonaNombre { get; set; } = "";

        public int? JuzgadoAplicacionDextraId { get; set; }
        public string? JuzgadoDextraAplicacionExacta { get; set; }
    }

}
