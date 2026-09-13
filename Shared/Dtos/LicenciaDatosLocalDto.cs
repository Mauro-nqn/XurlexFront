namespace IurixBlazor.Shared.Dtos
{
    public class LicenciaDatosLocalDto
    {
        public string EstudioNombre { get; set; } = "";
        public string Clave { get; set; } = "";
        public DateTime ValidoHasta { get; set; }
        public int Id { get; set; }
        public string DispositivoId { get; set; } = "";
        //public int MaxUsuariosLocales { get; set; }
        //public int MaxDispositivos { get; set; }
        public string Estado { get; set; } = "";
    }

}
