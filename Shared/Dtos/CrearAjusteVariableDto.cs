namespace IurixBlazor.Shared.Dtos
{
    public class CrearAjusteVariableDto
    {
        public string Clave { get; set; } = default!;
        public string Nombre { get; set; } = default!;
        public decimal ValorActual { get; set; }
        public bool Activa { get; set; } = true;
    }
}
