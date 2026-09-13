using IurixBlazor.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace IurixBlazor.Shared.Dtos
{
    public class CrearJuzgadoDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        public string? Fuero { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una circunscripción")]
        public int? CircunscripcionId { get; set; }

        // Persistencia:
        public string? DextraAplicacionExacta { get; set; }
        public AplicacionDextra? AplicacionDextra { get; set; }  // enum opcional

        // NUEVO: id numérico del enum para el binding del select
        public int? AplicacionDextraId { get; set; }
    }
}
