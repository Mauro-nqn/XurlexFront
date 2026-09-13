namespace IurixBlazor.Shared.Dtos
{
    public class IvaAlicuotaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }

        public int CodigoAfip { get; set; }
    }
}
