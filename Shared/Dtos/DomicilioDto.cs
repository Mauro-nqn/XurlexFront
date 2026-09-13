namespace IurixBlazor.Shared.Dtos
{
    public class DomicilioDto
    {       
        
     public int Id { get; set; }
     public string Descripcion { get; set; } = string.Empty;

     public TipoDomicilio Tipo { get; set; }

     public string? Ciudad { get; set; }
     
     public string? Provincia { get; set; }


    }
}
