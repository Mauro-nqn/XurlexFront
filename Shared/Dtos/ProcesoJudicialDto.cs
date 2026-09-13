namespace IurixBlazor.Shared.Dtos
{
    public class ProcesoJudicialDto
    {
        public int Id { get; set; }
        public string Caratula { get; set; } = string.Empty;
        public string NumeroExpediente { get; set; } = string.Empty;
        public string Instancia { get; set; } = string.Empty;
        public string EstadoNombre { get; set; } = string.Empty;


        public int TipoProcesoId { get; set; }

        //  NUEVOS
        public int? JurisdiccionId { get; set; }
        public int? CircunscripcionId { get; set; }
        public int? JuzgadoId { get; set; }
        public int? SecretariaId { get; set; }
        public int EstadoId { get; set; }
        public int? DomicilioConstituidoId { get; set; }
        public int? DomicilioElectronicoId { get; set; }



        //  Nuevos
        public string? NumeroCertificadoDeuda { get; set; }
        public decimal? MontoDemanda { get; set; }
        public DateTime? FechaEmisionCertificado { get; set; }
        public DateTime? FechaLiquidacion { get; set; }

    }
}
