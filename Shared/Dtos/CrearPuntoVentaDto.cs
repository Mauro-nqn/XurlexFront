namespace IurixBlazor.Shared.Dtos
{
    public class CrearPuntoVentaDto
    {
        public int Numero { get; set; }
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        public bool UsaNumeracionLocal { get; set; } = false; // opcional
    }
}
