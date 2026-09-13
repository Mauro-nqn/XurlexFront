//using IurixBlazor.Shared.Dtos;

//public class EditorInfoService
//{
//    public EditorInfoDto? UltimaEdicion { get; private set; }

//    public void Guardar(string? titulo, string? contenido)
//    {
//        UltimaEdicion = new EditorInfoDto
//        {
//            Titulo = titulo,
//            ContenidoHtml = contenido
//        };
//    }

//    public EditorInfoDto? ObtenerYLimpiar()
//    {
//        var temp = UltimaEdicion;
//        UltimaEdicion = null; // limpiar para no reutilizar datos viejos
//        return temp;
//    }
//}


using IurixBlazor.Shared.Dtos;

public class EditorInfoService
{
    private readonly Dictionary<string, EditorInfoDto> _ediciones = new();

    public void Guardar(string clave, string? titulo, string? contenidoHtml)
    {
        _ediciones[clave] = new EditorInfoDto
        {
            Titulo = titulo,
            ContenidoHtml = contenidoHtml,
            UltimaModificacion = DateTime.UtcNow
        };
    }

    public EditorInfoDto? Obtener(string clave)
    {
        _ediciones.TryGetValue(clave, out var dto);
        return dto;
    }

    public void Limpiar(string clave)
    {
        _ediciones.Remove(clave);
    }

    public bool TieneCambios(string clave)
    {
        return _ediciones.ContainsKey(clave);
    }
}

