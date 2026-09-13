using IurixBlazor.Shared.Enums;

namespace IurixBlazor.Shared.Dtos
{
    public class CrearCircunscripcionDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int JurisdiccionId { get; set; }

        public string? DextraCiudadExacta { get; set; }
        public CiudadDextra? CiudadDextra { get; set; }
    }
}
