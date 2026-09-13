using System.Text.Json.Serialization;

namespace IurixBlazor.Shared.Dtos
{
    public class DistribucionDetallePatchDto
    {
        public int Id { get; set; }

        public int DistribucionHonorarioId { get; set; }      

        public int UsuarioId { get; set; }        

        public decimal Porcentaje { get; set; }

        public string? Concepto { get; set; } // opcional para agregar concepto del porcentaje que corresponde al profesional
    }
}
