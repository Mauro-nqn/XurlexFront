using System.Text.Json.Serialization;
using System.Collections.Generic;


namespace IurixBlazor.Shared.Iademandas
{



    public class AbogadoDto
    {
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty; // Responsable desde Gestion y consultamos con ese id en Usuario
        
        [JsonPropertyName("apellido")]
        public string Apellido{ get; set; } = string.Empty; // Responsable desde Gestion y consultamos con ese id en Usuario

        [JsonPropertyName("matricula")]
        public string Matricula { get; set; } = string.Empty; // Responsable desde Gestion y consultamos con ese id en Usuario

        [JsonPropertyName("cuit")] // Responsable desde Gestion y consultamos con ese id en Usuario
        public string? Cuit { get; set; } // Responsable desde Gestion y consultamos con ese id en Usuario

        [JsonPropertyName("condicion_iva")] // Responsable desde Gestion y consultamos con ese id en Usuario
        public string? CondicionIva { get; set; }

        [JsonPropertyName("domicilio_procesal")] //gestion, proceso judicial, domicilio procesal
        public string? DomicilioProcesal { get; set; }

        [JsonPropertyName("domicilio_electronico")] //gestion, proceso judicial, domicilio electronico
        public string? DomicilioElectronico { get; set; }

        [JsonPropertyName("ciudad")] //gestion, proceso judicial, domicilio procesal, ciudad
        public string? Ciudad { get; set; }

        [JsonPropertyName("provincia")] //gestion, proceso judicial, domicilio procesal, ciudad
        public string? Provincia { get; set; }

        [JsonPropertyName("cargo")] //tomamos cargo del modelo usuario Dto
        public string Cargo { get; set; } = "abogada";
    }

    public class ParteDto
    {
        [JsonPropertyName("nombre")] // tomamos de Partes, Persona actor Nombre y Apellido o Razon Social - caracter actor
        public string Nombre { get; set; } = string.Empty;
        
        // para el caso del demandado igual pero el caracter demandado

        [JsonPropertyName("domicilio")] // tomamos de Partes, Persona - Domicilio actor
        public string? Domicilio { get; set; }

        //para el caso del demandado igual pero el caracter demandado

        [JsonPropertyName("ciudad")] // tomamos de Partes, Persona- ciudad - caracter actor
        public string? Ciudad { get; set; }
        //para el caso del demandado igual pero el caracter demandado

        [JsonPropertyName("provincia")] // tomamos de Partes, Persona - provincia - caracter actor
        public string? Provincia { get; set; }
        //para el caso del demandado igual pero el caracter demandado

        [JsonPropertyName("cuit")] // tomamos de Partes, Persona - cuit si tiene  actor
        public string? Cuit { get; set; }
        //para el caso del demandado igual pero el caracter demandado

        [JsonPropertyName("dni")] // tomamos de Partes, Persona - dni si tiene actor
        public string? Dni { get; set; }
        //para el caso del demandado igual pero el caracter demandado
    }

    public class CertificadoDeudaDto
    {
        // Monto total de la deuda según certificado
        [JsonPropertyName("monto")] //
        public decimal Monto { get; set; }

        // Fecha de emisión del certificado
        [JsonPropertyName("fecha")]
        public string FechaCertificado { get; set; } = string.Empty; // yyyy-MM-dd

        // Fecha hasta la cual está liquidada la deuda
        [JsonPropertyName("fecha_liquidacion")]
        public string? FechaLiquidacion { get; set; }

        [JsonPropertyName("tasa_interes")]
        public string? TasaInteres { get; set; }

        [JsonPropertyName("numero")]
        public string? Numero { get; set; }

        [JsonPropertyName("organismo_emisor")]
        public string? OrganismoEmisor { get; set; }

        [JsonPropertyName("dependencia")]
        public string? Dependencia { get; set; }

        [JsonPropertyName("texto_completo")]
        public string? TextoCompleto { get; set; }
    }

    public class DemandaEjecutivaRequestDto
    {
        [JsonPropertyName("abogado")]
        public AbogadoDto Abogado { get; set; } = new();

        [JsonPropertyName("cliente")]
        public ParteDto Cliente { get; set; } = new();

        [JsonPropertyName("deudor")]
        public ParteDto Deudor { get; set; } = new();

        [JsonPropertyName("certificado")]
        public CertificadoDeudaDto Certificado { get; set; } = new();

        [JsonPropertyName("jurisdiccion")]
        public string Jurisdiccion { get; set; } = string.Empty;

        [JsonPropertyName("juez_competente")]
        public string? JuezCompetente { get; set; }

        [JsonPropertyName("tipo_credito")]
        public string? TipoCredito { get; set; }

        [JsonPropertyName("observaciones")]
        public string? Observaciones { get; set; }
    }

    public class DemandaEjecutivaResponseDto
    {
        [JsonPropertyName("borrador_demanda")]
        public string BorradorDemanda { get; set; } = string.Empty;

        [JsonPropertyName("secciones")]
        public Dictionary<string, string> Secciones { get; set; } = new();

        [JsonPropertyName("avisos")]
        public List<string> Avisos { get; set; } = new();
    }
}

