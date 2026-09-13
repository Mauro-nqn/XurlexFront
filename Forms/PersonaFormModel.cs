using IurixBlazor.Shared.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace IurixBlazor.Forms
{
    

    public class PersonaFormModel : IValidatableObject
    {
        [Required(ErrorMessage = "El tipo de persona es obligatorio.")]
        public TipoPersona Tipo { get; set; }

        // Física
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }

        // Jurídica
        public string? RazonSocial { get; set; }

        // Identificación
        [RegularExpression(@"^\d{7,8}$", ErrorMessage = "DNI debe tener 7 u 8 dígitos.")]
        public string? DNI { get; set; }

        // CUIT validado por algoritmo en onblur (en el form), acá opcionalmente solo 11 dígitos:
        //[RegularExpression(@"^\d{11}$", ErrorMessage = "CUIT debe tener 11 dígitos.")]
        [RegularExpression(@"^(\d{11}|\d{2}-\d{8}-\d)$", ErrorMessage = "CUIT debe tener 11 dígitos.")]
        public string? CUIT { get; set; }

        public int? ClasificacionId { get; set; }
        public int? CondicionIVAId { get; set; }   // requerida si hay CUIT (en física y jurídica)

        // Contacto
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email no es válido.")]
        // Alternativa: [EmailAddress(ErrorMessage = "Email no es válido.")]
        public string? Email { get; set; }

        public string? Nacionalidad { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
        public string? EstadoCivil { get; set; }
        public string? CondicionIIBB { get; set; }
        public string? Domicilio { get; set; }
        public string? Ciudad { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Provincia { get; set; }
        public string? Telefono { get; set; }
        public string? Celular { get; set; }
        public string? Observacion { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
        {
            // Normalizar vacíos a null para la lógica
            var dni = string.IsNullOrWhiteSpace(DNI) ? null : DNI;
            var cuit = string.IsNullOrWhiteSpace(CUIT) ? null : CUIT;

            if (Tipo == TipoPersona.Fisica)
            {
                if (string.IsNullOrWhiteSpace(Nombre))
                    yield return new ValidationResult("Nombre es obligatorio.", new[] { nameof(Nombre) });
                if (string.IsNullOrWhiteSpace(Apellido))
                    yield return new ValidationResult("Apellido es obligatorio.", new[] { nameof(Apellido) });

                // Reglas DNI/CUIT (uno u otro, al menos uno)
                if (dni is null && cuit is null)
                    yield return new ValidationResult("Debe ingresar DNI o CUIT.", new[] { nameof(DNI), nameof(CUIT) });
                if (dni is not null && cuit is not null)
                    yield return new ValidationResult("No puede completar DNI y CUIT a la vez.", new[] { nameof(DNI), nameof(CUIT) });

                // Si hay CUIT, Condición IVA obligatoria
                if (cuit is not null && !CondicionIVAId.HasValue)
                    yield return new ValidationResult("Debe seleccionar Condición IVA si informa CUIT.", new[] { nameof(CondicionIVAId) });
            }
            else // Jurídica
            {
                if (string.IsNullOrWhiteSpace(RazonSocial))
                    yield return new ValidationResult("Razón Social es obligatoria.", new[] { nameof(RazonSocial) });

                // Jurídica: CUIT obligatorio, DNI no corresponde
                if (cuit is null)
                    yield return new ValidationResult("CUIT es obligatorio para persona jurídica.", new[] { nameof(CUIT) });
                if (!string.IsNullOrWhiteSpace(DNI))
                    yield return new ValidationResult("No debe informar DNI para persona jurídica.", new[] { nameof(DNI) });

                // Condición IVA obligatoria
                if (!CondicionIVAId.HasValue)
                    yield return new ValidationResult("Debe seleccionar Condición IVA.", new[] { nameof(CondicionIVAId) });
            }
        }
    }

}
