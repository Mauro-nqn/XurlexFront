using System;

namespace IurixBlazor.Shared.Dtos
{
    public class AfipAuthDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }

        // Homologación
        public string RSA_PKEY { get; set; } = string.Empty;
        public string CERT_X509 { get; set; } = string.Empty;
        public string TA { get; set; } = string.Empty;
        public string Sign { get; set; } = string.Empty;
        public DateTime? TokenExpiration { get; set; }

        // Producción
        public string RSA_PKEY_PROD { get; set; } = string.Empty;
        public string CERT_X509_PROD { get; set; } = string.Empty;
        public string TA_PROD { get; set; } = string.Empty;
        public string Sign_PROD { get; set; } = string.Empty;
        public DateTime? TokenExpiration_PROD { get; set; }


        // ✅ NUEVO: vigencias extraídas del certificado
        public DateTime? CertValidFrom { get; set; }        // Homologación - emisión
        public DateTime? CertValidTo { get; set; }          // Homologación - vencimiento
        public DateTime? CertValidFrom_PROD { get; set; }   // Producción - emisión
        public DateTime? CertValidTo_PROD { get; set; }     // Producción - vencimiento
    }
}
