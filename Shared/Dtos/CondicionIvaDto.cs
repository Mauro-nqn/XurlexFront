namespace IurixBlazor.Shared.Dtos
{
    public class CondicionIvaDto
    {

        public int Id { get; set; }

        public string? Nombre { get; set; } // Ej: "Responsable Inscripto"
        public int? CodigoAfip { get; set; } // Ej: 1 = RI, 5 = CF
        public decimal? AlicuotaGeneral { get; set; } // Por si querés setearlo por default


    }
}
