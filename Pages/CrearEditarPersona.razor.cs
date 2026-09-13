using IurixBlazor.Forms;
using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Net.Quic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;



namespace IurixBlazor.Pages
{
    public class CrearEditarPersonaBase : ComponentBase
    {
        [Inject] protected PersonaService PersonaService { get; set; } = default!;
        [Inject] protected CondicionIvaService CondicionIvaService { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected IJSRuntime JS { get; set; } = default!;

        [Inject] protected AfipAuthService AfipAuthService { get; set; } = default!;
        [Inject] protected UsuarioService UsuarioService { get; set; } = default!; // Para obtener usuario logueado

        [Parameter] public int? Id { get; set; }

        protected PersonaDto Persona { get; set; } = new();
        protected List<ClasificacionPersonaDto> Clasificaciones { get; set; } = new();
        protected List<CondicionIvaDto> CondicionesIVA { get; set; } = new();
        protected List<string> TiposPersona { get; set; } = Enum.GetNames(typeof(TipoPersona)).ToList();


        protected PersonaFormModel Form { get; set; } = new();




        protected string? CuitError { get; set; }



        // Flags de UI
        protected bool DisableDNI => Form.Tipo == TipoPersona.Fisica && !string.IsNullOrWhiteSpace(Form.CUIT);
        protected bool DisableCUIT => Form.Tipo == TipoPersona.Fisica && !string.IsNullOrWhiteSpace(Form.DNI);
        protected bool DebeCondicionIva =>
            Form.Tipo == TipoPersona.Juridica || !string.IsNullOrWhiteSpace(Form.CUIT);

        //[Parameter] public Guid WindowId { get; set; }


        protected EditContext EditContext { get; set; } = default!;

        protected bool _tieneCambios;


        public CrearEditarPersonaBase()
        {
            // 👇 IMPORTANTÍSIMO: que EditContext NUNCA sea null
            EditContext = new EditContext(Form);

            // Si quisieras, también podrías enganchar OnFieldChanged acá,
            // pero como necesitamos JS, lo hacemos en OnInitializedAsync.
        }






        protected override async Task OnInitializedAsync()
        {
            Clasificaciones = await PersonaService.ObtenerClasificacionesAsync();
            CondicionesIVA = await CondicionIvaService.ObtenerTodasAsync();

            if (Id.HasValue)
            {
                var personas = await PersonaService.ObtenerPersonasAsync();
                var persona = personas.FirstOrDefault(p => p.Id == Id) ?? new PersonaDto();

                // map DTO -> Form
                Form.Tipo = persona.Tipo;
                Form.Nombre = persona.Nombre;
                Form.Apellido = persona.Apellido;
                Form.RazonSocial = persona.RazonSocial;
                Form.DNI = string.IsNullOrWhiteSpace(persona.DNI) ? null : persona.DNI;
                Form.CUIT = string.IsNullOrWhiteSpace(persona.CUIT) ? null : persona.CUIT;
                Form.ClasificacionId = persona.ClasificacionId;
                Form.CondicionIVAId = persona.CondicionIVAId;
                Form.Email = string.IsNullOrWhiteSpace(persona.Email) ? null : persona.Email;
                Form.Nacionalidad = persona.Nacionalidad;
                Form.FechaNacimiento = persona.FechaNacimiento;
                Form.EstadoCivil = persona.EstadoCivil;
                Form.CondicionIIBB = persona.CondicionIIBB;
                Form.Domicilio = persona.Domicilio;
                Form.Ciudad = persona.Ciudad;
                Form.CodigoPostal = persona.CodigoPostal;
                Form.Provincia = persona.Provincia;
                Form.Telefono = persona.Telefono;
                Form.Celular = persona.Celular;
                Form.Observacion = persona.Observacion;

            }
            else
            {
               
                Form.Tipo = TipoPersona.Fisica;
                Form.ClasificacionId = Clasificaciones.FirstOrDefault(c => c.Nombre.Equals("Cliente", StringComparison.OrdinalIgnoreCase))?.Id
                                       ?? Clasificaciones.FirstOrDefault()?.Id;
                Form.CondicionIVAId = null; // En física sin CUIT, no es obligatoria

            }

            // 👇 Crear el EditContext sobre tu FormModel
            EditContext = new EditContext(Form);

            // 👇 Cada vez que cambia un campo, marcamos "hay cambios sin guardar"
            EditContext.OnFieldChanged += async (sender, e) =>
            {
                if (!_tieneCambios)
                {
                    _tieneCambios = true;
                    //await JS.InvokeVoidAsync("setUnsavedChanges", true);
                }
            };
            ;
        }



       

        protected void OnTipoChanged(ChangeEventArgs _)
        {
            if (Form.Tipo == TipoPersona.Juridica)
            {
                // Limpiar campos que no aplican
                Form.Nombre = null;
                Form.Apellido = null;
                Form.DNI = null;
            }
            else
            {
                // En física, Razón Social no aplica
                Form.RazonSocial = null;
                // Condición IVA solo será obligatoria si cargan CUIT
                if (string.IsNullOrWhiteSpace(Form.CUIT))
                    Form.CondicionIVAId = null;
            }
            StateHasChanged();
        }




        

        protected void FormatearCuitEnTiempoReal(ChangeEventArgs e)
        {
            var limpio = LimpiarCUIT(e.Value?.ToString() ?? "");
            if (limpio.Length <= 11)
                Form.CUIT = FormatearCUIT(limpio); // Muestra con guiones
        }



        protected void HandleCuitInput(ChangeEventArgs e)
        {
            var texto = e?.Value?.ToString() ?? string.Empty;

            // Normalizá a dígitos, recortá a 11
            var limpio = LimpiarCUIT(texto);
            if (limpio.Length > 11) limpio = limpio[..11];

            // Reaplicá formateo con guiones
            Form.CUIT = FormatearCUIT(limpio);

            OnCuitChanged(); // 👈 lo que te faltaba
        }




        protected void OnCuitChanged()
        {
            // limpiá errores al tipear
            CuitError = string.Empty;

            // si hay CUIT, podés bloquear DNI (opcional)
            var tieneCuit = !string.IsNullOrWhiteSpace(LimpiarCUIT(Form.CUIT ?? ""));
            //DisableDNI = tieneCuit;

            // si no querés bloquear, al menos podrías limpiar el DNI cuando haya CUIT:
            if (tieneCuit) Form.DNI = null;

            StateHasChanged(); // por si necesitás refrescar algo dependiente
        }



        protected void OnDniChanged(ChangeEventArgs _)
        {
            // Si ponen DNI en Física, limpiá CUIT y viceversa (UX)
            if (Form.Tipo == TipoPersona.Fisica && !string.IsNullOrWhiteSpace(Form.DNI))
            {
                Form.CUIT = null;
                CuitError = null;
            }

            // Automatically set CondicionIVA to "Consumidor Final"
            var consumidorFinalId = CondicionesIVA
                .FirstOrDefault(c => c.Nombre!.Equals("Consumidor Final", StringComparison.OrdinalIgnoreCase))?.Id;

            // Only set the value if it was found
            if (consumidorFinalId.HasValue)
            {
                Form.CondicionIVAId = consumidorFinalId.Value;
            }
        }



        protected void ValidarCuit(FocusEventArgs e)
        {
            var limpio = LimpiarCUIT(Persona.CUIT);
            if (!EsCuitValido(limpio) && !string.IsNullOrWhiteSpace(limpio))
                CuitError = "CUIT inválido. Verifique los 11 dígitos y el dígito verificador.";
            else
                CuitError = null;

            // Siempre guardar el CUIT sin guiones
            Persona.CUIT = limpio;
        }

     

        protected string LimpiarCUIT(string cuit) =>
            new string(cuit.Where(char.IsDigit).ToArray());

        protected string FormatearCUIT(string cuitPlano)
        {
            if (cuitPlano.Length == 11)
                return $"{cuitPlano[..2]}-{cuitPlano.Substring(2, 8)}-{cuitPlano[^1..]}";
            if (cuitPlano.Length > 2)
                return $"{cuitPlano[..2]}-{cuitPlano.Substring(2)}";
            return cuitPlano;
        }

        // Validador CUIT (algoritmo dígito verificador AFIP)
        protected bool EsCuitValido(string cuit)
        {
            if (string.IsNullOrWhiteSpace(cuit) || cuit.Length != 11) return false;
            var coef = new[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            var suma = cuit.Take(10).Select((c, i) => (c - '0') * coef[i]).Sum();
            var resto = 11 - (suma % 11);
            if (resto == 11) resto = 0;
            if (resto == 10) resto = 9;
            return (cuit[10] - '0') == resto;
        }




      



        protected async Task ConsultarAfip()
        {
            var cuitLimpio = LimpiarCUIT(Form.CUIT ?? "");
            if (string.IsNullOrWhiteSpace(cuitLimpio) || cuitLimpio.Length != 11)
            { await JS.InvokeVoidAsync("mostrarToast", "❌ Ingrese un CUIT válido de 11 dígitos.", "error"); return; }

            var usuarioIdStr = await JS.InvokeAsync<string>("localStorage.getItem", "usuario_id");
            int usuarioId = string.IsNullOrEmpty(usuarioIdStr) ? 0 : int.Parse(usuarioIdStr);

            var (datos, error) = await AfipAuthService.ConsultarConstanciaAsync(usuarioId, cuitLimpio);
            if (!string.IsNullOrEmpty(error))
            { await JS.InvokeVoidAsync("mostrarToast", $"❌ {error}", "error"); return; }
            if (datos == null)
            { await JS.InvokeVoidAsync("mostrarToast", "⚠️ No se pudo consultar AFIP.", "warning"); return; }

            Form.Tipo = datos.TipoPersona?.Equals("FISICA", StringComparison.OrdinalIgnoreCase) == true
                ? TipoPersona.Fisica : TipoPersona.Juridica;

            if (Form.Tipo == TipoPersona.Fisica)
            {
                Form.Nombre = datos.Nombre ?? Form.Nombre;
                Form.Apellido = datos.Apellido ?? Form.Apellido;
            }
            else
            {
                Form.RazonSocial = datos.RazonSocial ?? Form.RazonSocial;
            }

            Form.Domicilio = datos.Direccion ?? Form.Domicilio;
            Form.CodigoPostal = datos.CodPostal ?? Form.CodigoPostal;
            Form.Provincia = datos.DescripcionProvincia ?? Form.Provincia;
            Form.Ciudad = datos.Localidad ?? Form.Ciudad;

            Form.CondicionIVAId = datos.IdImpuesto switch
            {
                30 => CondicionesIVA.FirstOrDefault(c => c.Nombre!.Contains("Responsable", StringComparison.OrdinalIgnoreCase))?.Id,
                20 => CondicionesIVA.FirstOrDefault(c => c.Nombre!.Contains("Monotributo", StringComparison.OrdinalIgnoreCase))?.Id,
                32 => CondicionesIVA.FirstOrDefault(c => c.Nombre!.Contains("Exento", StringComparison.OrdinalIgnoreCase))?.Id,
                34 => CondicionesIVA.FirstOrDefault(c => c.Nombre!.Contains("No Alcanzado", StringComparison.OrdinalIgnoreCase))?.Id,
                _ => Form.CondicionIVAId
            };
        }





        protected async Task ConfirmarSalida(LocationChangingContext context)
        {
            if (!_tieneCambios)
                return; // no hay cambios → dejar navegar

            var confirmar = await JS.InvokeAsync<bool>(
                "confirm",
                "Hay cambios sin guardar en la persona. ¿Querés salir y perderlos?"
            );

            if (!confirmar)
            {
                context.PreventNavigation();
            }
        }



        protected async Task Guardar()
        {
            // Si llega acá, el form pasó validaciones de DataAnnotations/IValidatableObject
            if (Id.HasValue)
            {
                var patch = new PersonaPatchDto
                {
                    Tipo = Form.Tipo.ToString(),
                    Nombre = Form.Nombre,
                    Apellido = Form.Apellido,
                    RazonSocial = Form.RazonSocial,
                    DNI = Form.DNI,
                    CUIT = Form.CUIT,
                    ClasificacionId = Form.ClasificacionId,
                    CondicionIVAId = Form.CondicionIVAId,
                    Email = Form.Email,
                    Nacionalidad = Form.Nacionalidad,
                    FechaNacimiento = Form.FechaNacimiento,
                    EstadoCivil = Form.EstadoCivil,
                    CondicionIIBB = Form.CondicionIIBB,
                    Domicilio = Form.Domicilio,
                    Ciudad = Form.Ciudad,
                    CodigoPostal = Form.CodigoPostal,
                    Provincia = Form.Provincia,
                    Telefono = Form.Telefono,
                    Celular = Form.Celular,
                    Observacion = Form.Observacion
                };
                await PersonaService.ActualizarPersonaAsync(Id.Value, patch);
            }
            else
            {
                // Crear: si tu endpoint espera PersonaDto, mapear respetando defaults
                var dto = new PersonaDto
                {
                    Tipo = Form.Tipo,
                    Nombre = Form.Nombre ?? string.Empty,
                    Apellido = Form.Apellido ?? string.Empty,
                    RazonSocial = Form.RazonSocial ?? string.Empty,
                    DNI = Form.DNI ?? string.Empty,
                    CUIT = Form.CUIT ?? string.Empty,
                    ClasificacionId = Form.ClasificacionId,
                    CondicionIVAId = Form.CondicionIVAId,
                    Email = Form.Email ?? string.Empty,
                    Nacionalidad = Form.Nacionalidad ?? string.Empty,
                    FechaNacimiento = Form.FechaNacimiento,
                    EstadoCivil = Form.EstadoCivil ?? string.Empty,
                    CondicionIIBB = Form.CondicionIIBB ?? string.Empty,
                    Domicilio = Form.Domicilio ?? string.Empty,
                    Ciudad = Form.Ciudad ?? string.Empty,
                    CodigoPostal = Form.CodigoPostal ?? string.Empty,
                    Provincia = Form.Provincia ?? string.Empty,
                    Telefono = Form.Telefono ?? string.Empty,
                    Celular = Form.Celular ?? string.Empty,
                    Observacion = Form.Observacion ?? string.Empty
                };
                await PersonaService.CrearPersonaAsync(dto);
            }

            // 1. Notifica a otros componentes que una persona fue guardada
            //PersonaEventService.PersonaSaved();
            _tieneCambios = false;
            //await JS.InvokeVoidAsync("setUnsavedChanges", false);

            await JS.InvokeVoidAsync("mostrarToast", "✅ Persona guardada correctamente.", "success");
            Nav.NavigateTo("/personas");
            // 3. Añade un pequeño retraso (e.g., 300 milisegundos) para que el toast sea visible.
            //    Este tiempo permite que la UI se actualice en el navegador.
            // Llamas a este nuevo método que se encarga del cierre ordenado
            //await CerrarVentanaConToast();
        }





        protected async Task Cancelar()
        {
            if (_tieneCambios)
            {
                var confirmar = await JS.InvokeAsync<bool>(
                    "confirm",
                    "Hay cambios sin guardar en la persona. ¿Querés cancelar y perderlos?"
                );

                if (!confirmar)
                    return; // se queda en la página

                //  El usuario ya dijo que SÍ quiere perder cambios
                // desactivamos el guardia para esta navegación
                _tieneCambios = false;
            }
            

            Nav.NavigateTo("/personas");
        }
    }
}
