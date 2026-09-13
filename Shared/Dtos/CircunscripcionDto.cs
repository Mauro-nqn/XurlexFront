using IurixBlazor.Pages;
using IurixBlazor.Shared.Enums;

namespace IurixBlazor.Shared.Dtos
{
    public class CircunscripcionDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int JurisdiccionId { get; set; }
        public string JurisdiccionNombre { get; set; } = string.Empty;
        public string? DextraCiudadExacta { get; set; }
        public CiudadDextra? CiudadDextra { get; set; }
    }
}
