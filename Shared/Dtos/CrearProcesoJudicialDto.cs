using System.Text.Json.Serialization;

namespace IurixBlazor.Shared.Dtos
{
    public class CrearProcesoJudicialDto
    {
        [JsonIgnore] public int? Id { get; set; }  // solo cliente
        public string Caratula { get; set; } = string.Empty;
        public int TipoProcesoId { get; set; }
        public string NumeroExpediente { get; set; } = string.Empty;
        public int? DomicilioConstituidoId { get; set; }
        public int? DomicilioElectronicoId { get; set; }

        //  NUEVOS
        public int? JurisdiccionId { get; set; }
        public int? CircunscripcionId { get; set; }
        public int? JuzgadoId { get; set; }
        public int? SecretariaId { get; set; }
        public string Instancia { get; set; } = string.Empty;
        public int EstadoId { get; set; }
    }
}
