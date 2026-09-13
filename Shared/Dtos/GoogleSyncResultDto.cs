namespace IurixBlazor.Shared.Dtos
{
    public sealed class GoogleSyncResultDto
    {
        public bool ok { get; set; }
        public int Id { get; set; }
        public string? GoogleEventId { get; set; }
        public string? GoogleHtmlLink { get; set; }
        public bool GoogleRegistrado { get; set; }
        public DateTimeOffset? GoogleLastSyncUtc { get; set; }

        public int? GoogleUsuarioId { get; set; }        //  nuevo
        public string? GoogleEmailSnapshot { get; set; } // opcional
    }
}
