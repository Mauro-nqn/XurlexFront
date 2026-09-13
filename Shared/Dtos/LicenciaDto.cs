using System;
using System.Text.Json.Serialization;

namespace IurixBlazor.Shared.Dtos
{


    public class LicenciaDto
    {
        public string? Clave { get; set; }
        public string? DispositivoId { get; set; }

        public string DispositivoSesionId { get; set; } = string.Empty;

        public DateTime FechaExpiracion { get; set; }

        public string? ApiUrl { get; set; }

        public int? Puerto { get; set; }

        public string? IpLocal { get; set; }

        [JsonPropertyName("idLicencia")]
        public int IdLicencia { get; set; }
        public string EstudioNombre { get; set; } = string.Empty;
        public DateTime ValidoHasta { get; set; }

        public int Id { get; set; }

        public string CUIT { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

    }
}