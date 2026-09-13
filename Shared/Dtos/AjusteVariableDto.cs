namespace IurixBlazor.Shared.Dtos
{
    public class AjusteVariableDto
    {
        public int Id { get; set; }
        public string Clave { get; set; } = default!;
        public string Nombre { get; set; } = default!;
        public decimal ValorActual { get; set; }
        public bool Activa { get; set; }
        public DateTimeOffset ActualizadoUtc { get; set; }
    }
}
