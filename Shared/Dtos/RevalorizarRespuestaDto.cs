namespace IurixBlazor.Shared.Dtos
{
    public record RevalorizarRespuestaDto(

        bool aplicar, 
        List<RevalCambioDto> cambios
        
        );
}
