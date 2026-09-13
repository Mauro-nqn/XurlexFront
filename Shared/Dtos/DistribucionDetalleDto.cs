

using IurixBlazor.Pages;
using System.Text.Json.Serialization;

namespace IurixBlazor.Shared.Dtos
{
    public class DistribucionDetalleDto
    {
        public int Id { get; set; }

        public int DistribucionHonorarioId { get; set; }

        [JsonIgnore] // 👈 evita el ciclo
        public DistribucionHonorarioDto? DistribucionHonorario { get; set; }
        

        public int UsuarioId { get; set; }
        public UsuarioDto? Usuario { get; set; }
        public decimal Porcentaje { get; set; }

        public string? Concepto { get; set; } // opcional para agregar concepto del porcentaje que corresponde al profesional


    }
}
