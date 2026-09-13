//using System.Security.Cryptography;
//using System.Security.Cryptography.X509Certificates;
//using System.Text;

//namespace IurixBlazor.Services
//{
//    public class CertService
//    {
//        /// <summary>
//        /// Genera una clave privada RSA en formato PEM.
//        /// </summary>
//        public Task<string> GenerarClavePrivadaAsync()
//        {
//            return Task.Run(() =>
//            {
//                using (RSA rsa = RSA.Create(2048))
//                {
//                    var privateKey = rsa.ExportRSAPrivateKey();
//                    return ConvertToPem(privateKey, "RSA PRIVATE KEY");
//                }
//            });
//        }




//        public Task<string> GenerarCSRAsync(string clavePrivadaPem, string cuit, string razonSocial, string nombreSistema)
//        {
//            return Task.Run(() =>
//            {
//                using (RSA rsa = ImportPrivateKeyFromPem(clavePrivadaPem))
//                {
//                    string subjectString = $"C=AR, O={razonSocial}, CN={nombreSistema}, serialNumber=CUIT {cuit}";
//                    var subject = new X500DistinguishedName(subjectString);

//                    var request = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

//                    // Exportar en formato DER y luego envolver en PEM
//                    byte[] csrBytes = request.CreateSigningRequest();

//                    string base64 = Convert.ToBase64String(csrBytes, Base64FormattingOptions.InsertLineBreaks);
//                    return $"-----BEGIN CERTIFICATE REQUEST-----\n{base64}\n-----END CERTIFICATE REQUEST-----\n";
//                }
//            });
//        }


//        public Task<string> GenerarCSRBase64Async(string clavePrivadaPem, string cuit, string razonSocial, string nombreSistema)
//        {
//            System.Diagnostics.Debug.WriteLine($"Clave: {clavePrivadaPem}, cuit {cuit}, razon social {razonSocial}, nombre sistema {nombreSistema}");
//            return Task.Run(() =>
//            {
//                using (RSA rsa = ImportPrivateKeyFromPem(clavePrivadaPem))
//                {
//                    string subjectString = $"C=AR, O={razonSocial}, CN={nombreSistema}, serialNumber=CUIT {cuit}";

//                    var subject = new X500DistinguishedName(subjectString);

//                    var request = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

//                    // Generar CSR en formato DER
//                    byte[] csrBytes = request.CreateSigningRequest();

//                    // ✅ Devolver solo el contenido Base64, sin cabeceras PEM
//                    return Convert.ToBase64String(csrBytes, Base64FormattingOptions.None);
//                }
//            });
//        }



//        /// <summary>
//        /// Convierte datos binarios a formato PEM con saltos de línea estándar.
//        /// </summary>
//        private string ConvertToPem(byte[] data, string type)
//        {
//            StringBuilder sb = new StringBuilder();
//            sb.AppendLine($"-----BEGIN {type}-----");
//            string base64 = Convert.ToBase64String(data, Base64FormattingOptions.InsertLineBreaks);

//            foreach (var line in SplitByLength(base64, 64))
//                sb.AppendLine(line);

//            sb.AppendLine($"-----END {type}-----");
//            return sb.ToString();
//        }

//        /// <summary>
//        /// Divide texto en líneas de longitud fija (estándar PEM = 64 caracteres).
//        /// </summary>
//        private IEnumerable<string> SplitByLength(string input, int length)
//        {
//            for (int i = 0; i < input.Length; i += length)
//                yield return input.Substring(i, Math.Min(length, input.Length - i));
//        }

//        /// <summary>
//        /// Importa una clave privada RSA desde un string PEM.
//        /// </summary>
//        private RSA ImportPrivateKeyFromPem(string privateKeyPem)
//        {
//            byte[] privateKeyBytes = Convert.FromBase64String(privateKeyPem
//                .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
//                .Replace("-----END RSA PRIVATE KEY-----", "")
//                .Replace("\n", "").Replace("\r", "").Trim());

//            RSA rsa = RSA.Create();
//            rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
//            return rsa;
//        }
//    }
//}


using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

public class CertService
{
    /// <summary>
    /// Genera una clave privada RSA en formato PEM.
    /// </summary>
    public Task<string> GenerarClavePrivadaAsync()
    {
        return Task.Run(() =>
        {
            using (RSA rsa = RSA.Create(2048))
            {
                var privateKey = rsa.ExportRSAPrivateKey();
                return ConvertToPem(privateKey, "RSA PRIVATE KEY");
            }
        });
    }

 


    //ESTA FUNCION DEVUELVE EL ARCHIVO CSR CODIFICADO EN BASE64
    //HAY QUE DECODIFICARLO PARA VER EL CERTIFICADO QUE HAY QUE SUBIR AFIP

    //public Task<string> GenerarCSRAsync(string clavePrivadaPem, string cuit, string razonSocial, string nombreSistema)
    //{
    //    return Task.Run(() =>
    //    {
    //        using (RSA rsa = ImportPrivateKeyFromPem(clavePrivadaPem))
    //        {
    //            // ✅ Subject en formato AFIP
    //            string subjectString = $"C=AR, O={razonSocial}, CN={nombreSistema}, serialNumber=CUIT {cuit}";
    //            var subject = new X500DistinguishedName(subjectString);

    //            // Crear el request
    //            var request = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

    //            // Exportar DER PKCS#10
    //            byte[] csrBytes = request.CreateSigningRequest();

    //            // Convertir a PEM con saltos de línea CRLF (Windows-friendly)
    //            string base64 = Convert.ToBase64String(csrBytes, Base64FormattingOptions.InsertLineBreaks);
    //            return $"-----BEGIN CERTIFICATE REQUEST-----\r\n{base64}\r\n-----END CERTIFICATE REQUEST-----\r\n";
    //        }
    //    });
    //}



    public Task<string> GenerarCSRAsync(string clavePrivadaPem, string cuit, string razonSocial, string nombreSistema)
    {
        return Task.Run(() =>
        {
            using (RSA rsa = ImportPrivateKeyFromPem(clavePrivadaPem))
            {
                // Subject en formato requerido por AFIP
                string subjectString = $"C=AR, O={razonSocial}, CN={nombreSistema}, serialNumber=CUIT {cuit}";
                var subject = new X500DistinguishedName(subjectString);

                // Generar el PKCS#10 CSR en formato DER
                var request = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                byte[] csrBytes = request.CreateSigningRequest();

                // Convertir DER -> Base64 (insertando saltos de línea cada 64 caracteres)
                string base64 = Convert.ToBase64String(csrBytes, Base64FormattingOptions.InsertLineBreaks);

                // Generar contenido PEM completo que AFIP acepta directamente
                string pem = "-----BEGIN CERTIFICATE REQUEST-----\r\n" +
                             base64 + "\r\n" +
                             "-----END CERTIFICATE REQUEST-----\r\n";

                return pem; // ✅ Este texto será el contenido final del archivo .csr
            }
        });
    }







    /// <summary>
    /// Importa una clave privada RSA desde un string PEM.
    /// </summary>
    private RSA ImportPrivateKeyFromPem(string privateKeyPem)
    {
        byte[] privateKeyBytes = Convert.FromBase64String(privateKeyPem
            .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
            .Replace("-----END RSA PRIVATE KEY-----", "")
            .Replace("\n", "")
            .Replace("\r", "")
            .Trim());

        RSA rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
        return rsa;
    }

    /// <summary>
    /// Convierte datos binarios a formato PEM con saltos de línea estándar.
    /// </summary>
    private string ConvertToPem(byte[] data, string type)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"-----BEGIN {type}-----");
        string base64 = Convert.ToBase64String(data, Base64FormattingOptions.InsertLineBreaks);

        sb.AppendLine(base64);
        sb.AppendLine($"-----END {type}-----");
        return sb.ToString();
    }
}


