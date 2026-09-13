namespace IurixBlazor.Shared.Dtos
{
    public record RevalCambioDto(
        int Id, 
        string Descripcion, 
        decimal Antes, 
        decimal Despues, 
        string? Variable, 
        decimal? ValorActual);
}
