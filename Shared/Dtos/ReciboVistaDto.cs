namespace IurixBlazor.Shared.Dtos
{
    public class ReciboVistaDto
    {
        public ReciboDto Recibo { get; set; } = default!;
        public PersonaDto Cliente { get; set; } = default!;
        public List<ImputacionDetalleDto> Imputaciones { get; set; } = new();
        public decimal TotalImputado { get; set; }
        public decimal SaldoResultante { get; set; } // opcional


        public bool Anulado { get; set; }
        public DateTime? FechaAnulacion { get; set; }
        public string? MotivoAnulacion { get; set; }


        //  Flags útiles
        public bool TieneImputaciones => TotalImputado > 0;
        public bool EstaCompleto => SaldoResultante <= 0 && !Anulado;
        public bool PuedeImputar => !Anulado && SaldoResultante > 0;
        public bool PuedeAnular => !Anulado;

        /// <summary>Eliminar solo si no está anulado y no tiene imputaciones.</summary>
        public bool PuedeEliminar => !Anulado && TotalImputado == 0;
        /// <summary>Solo tiene sentido deshacer anulación si está anulado.</summary>
        public bool PuedeDeshacerAnulacion => Anulado;

    }
}
