using IurixBlazor.Shared.Enums;

namespace IurixBlazor.Shared.Dtos
{
    public class JuzgadoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Fuero { get; set; }
        public int CircunscripcionId { get; set; }
        public string CircunscripcionNombre { get; set; } = string.Empty;

        public string? DextraAplicacionExacta { get; set; }
        public AplicacionDextra? AplicacionDextra { get; set; }  // enum opcional

        // NUEVO: id numérico del enum para el binding del select
        public int? AplicacionDextraId { get; set; }
    }
}
