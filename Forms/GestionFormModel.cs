using System.ComponentModel.DataAnnotations;



namespace IurixBlazor.Forms
{
    

    public class GestionFormModel : IValidatableObject
    {



        // --- Campos comunes (obligatorios) ---
        [Display(Name = "Responsable")]
        [Required(ErrorMessage = "Responsable es obligatorio.")]
        public int? ResponsableId { get; set; }

        [Display(Name = "Cliente")]
        [Required(ErrorMessage = "Cliente es obligatorio.")]
        public int? PersonaId { get; set; }

        [Required(ErrorMessage = "Tipo de gestión es obligatorio.")]
        public string? TipoGestion { get; set; } // "Judicial" | "Extrajudicial"





        // Opcionales
        public int? PresupuestoId { get; set; }
        public int? GrupoId { get; set; }

        // --- Judicial ---
        public string? PJ_Caratula { get; set; }
        public string? PJ_NumeroExpediente { get; set; }
        public int? PJ_TipoProcesoId { get; set; }
        //  para mostrar/ocultar los campos de apremio
        public bool PJ_EsApremio { get; set; }

        public int? PJ_DomicilioConstituidoId { get; set; } //ver si es opcional
        public int? PJ_DomicilioElectronicoId { get; set; } //ver si es opcinal
        public int? PJ_JurisdiccionId { get; set; }
        public int? PJ_CircunscripcionId { get; set; }
        public int? PJ_JuzgadoId { get; set; }
        public int? PJ_SecretariaId { get; set; }
        public int? PJ_EstadoId { get; set; }

        // --- Extrajudicial ---
        public string? EX_Tipo { get; set; }      // obligatorio
        public string? EX_Materia { get; set; }   // obligatorio
        public int? EX_EstadoId { get; set; }     // opcional

        //Nuevos campos Proceso
        public string? PJ_NumeroCertificadoDeuda { get; set; }
        public decimal? PJ_MontoDemanda { get; set; }
        public DateTime? PJ_FechaEmisionCertificado { get; set; }
        public DateTime? PJ_FechaLiquidacion { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext _)
        {
            if (TipoGestion is null) yield break;

            if (TipoGestion == "Judicial")
            {
                if (string.IsNullOrWhiteSpace(PJ_Caratula))
                    yield return new ValidationResult("Carátula es obligatoria.", new[] { nameof(PJ_Caratula) });

                //Campo optativo
                //if (string.IsNullOrWhiteSpace(PJ_NumeroExpediente))
                //    yield return new ValidationResult("Número de expediente es obligatorio.", new[] { nameof(PJ_NumeroExpediente) });

                if (!PJ_TipoProcesoId.HasValue)
                    yield return new ValidationResult("Tipo de proceso es obligatorio.", new[] { nameof(PJ_TipoProcesoId) });

                if (!PJ_DomicilioConstituidoId.HasValue)
                    yield return new ValidationResult("Domicilio constituido es obligatorio.", new[] { nameof(PJ_DomicilioConstituidoId) });

                if (!PJ_DomicilioElectronicoId.HasValue)
                    yield return new ValidationResult("Domicilio electrónico es obligatorio.", new[] { nameof(PJ_DomicilioElectronicoId) });

                if (!PJ_JurisdiccionId.HasValue)
                    yield return new ValidationResult("Jurisdicción es obligatoria.", new[] { nameof(PJ_JurisdiccionId) });

                //Campos optativos

                //if (!PJ_CircunscripcionId.HasValue)
                //    yield return new ValidationResult("Circunscripción es obligatoria.", new[] { nameof(PJ_CircunscripcionId) });

                //if (!PJ_JuzgadoId.HasValue)
                //    yield return new ValidationResult("Juzgado es obligatorio.", new[] { nameof(PJ_JuzgadoId) });

                //if (!PJ_SecretariaId.HasValue)
                //    yield return new ValidationResult("Secretaría es obligatoria.", new[] { nameof(PJ_SecretariaId) });

                if (!PJ_EstadoId.HasValue)
                    yield return new ValidationResult("Estado judicial es obligatorio.", new[] { nameof(PJ_EstadoId) });


            }
            else if (TipoGestion == "Extrajudicial")
            {
                if (string.IsNullOrWhiteSpace(EX_Tipo))
                    yield return new ValidationResult("Tipo (extrajudicial) es obligatorio.", new[] { nameof(EX_Tipo) });

                if (string.IsNullOrWhiteSpace(EX_Materia))
                    yield return new ValidationResult("Materia (extrajudicial) es obligatoria.", new[] { nameof(EX_Materia) });

                // EX_EstadoId es opcional -> no se valida
            }
        }
    }

}
