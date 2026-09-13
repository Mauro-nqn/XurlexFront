//using IurixBlazor.Forms;
//using IurixBlazor.Services;
//using IurixBlazor.Shared.Dtos;
//using IurixBlazor.Shared.Enums;
//using IurixBlazor.Shared.Services;
//using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Components.Forms;
//using Microsoft.AspNetCore.Components.Web;
//using Microsoft.JSInterop;
////using Radzen;
//using System.Linq.Expressions;
//using System.Text.RegularExpressions;

//namespace IurixBlazor.Components.Personas;

//public partial class CrearEditarPersonaDialog : ComponentBase
//{
//    [Parameter] public int? Id { get; set; }

//    [Inject] public PersonaService PersonaService { get; set; } = default!;
//    [Inject] public CondicionIvaService CondicionIvaService { get; set; } = default!;
//    //[Inject] public DialogService DialogService { get; set; } = default!;
//    [Inject] public IJSRuntime JS { get; set; } = default!;
//    [Inject] public AfipAuthService AfipAuthService { get; set; } = default!;

//    [Inject] IurixBlazor.Services.Windowing.WindowService Win { get; set; } = default!;

//    protected string Titulo => Id.HasValue ? "✏️ Editar Persona" : "➕ Nueva Persona";

//    // Modelo que edita el form (reutilizamos PersonaDto)
//    protected PersonaDto Model { get; set; } = new();

//    protected List<ClasificacionPersonaDto> Clasificaciones { get; set; } = new();
//    protected List<CondicionIvaDto> CondicionesIVA { get; set; } = new();
//    protected List<TipoPersona> TiposPersona { get; set; } = Enum.GetValues<TipoPersona>().ToList();

//    protected EditContext _editContext = default!;
//    protected ValidationMessageStore _messages = default!;
//    protected string? CuitError { get; set; }

//    [Parameter] public Guid WindowId { get; set; }



//    // ==== Helpers regla CF ====
//    int? ConsumidorFinalId =>
//        CondicionesIVA.FirstOrDefault(c =>
//            c.Nombre?.Equals("Consumidor Final", StringComparison.OrdinalIgnoreCase) == true
//            || c.Nombre?.Contains("Consumidor Final", StringComparison.OrdinalIgnoreCase) == true
//        )?.Id;

//    protected bool BloquearCondicionIva =>
//        Model.Tipo == TipoPersona.Fisica
//        && !string.IsNullOrWhiteSpace(Model.DNI)
//        && string.IsNullOrWhiteSpace(Model.CUIT);

//    protected override async Task OnInitializedAsync()
//    {
//        Clasificaciones = await PersonaService.ObtenerClasificacionesAsync();
//        CondicionesIVA = await CondicionIvaService.ObtenerTodasAsync();

//        if (Id.HasValue)
//        {
//            var personas = await PersonaService.ObtenerPersonasAsync();
//            Model = personas.First(p => p.Id == Id.Value);
//        }
//        else
//        {
//            Model.Tipo = TipoPersona.Fisica;
//            Model.ClasificacionId = Clasificaciones.FirstOrDefault(c => c.Nombre.Equals("Cliente", StringComparison.OrdinalIgnoreCase))?.Id
//                                    ?? Clasificaciones.FirstOrDefault()?.Id ?? 0;
//            Model.CondicionIVAId = ConsumidorFinalId ?? CondicionesIVA.FirstOrDefault()?.Id ?? 0;
//        }

//        // Normalizar por si entra Física con DNI (sin CUIT)
//        if (Model.Tipo == TipoPersona.Fisica
//            && !string.IsNullOrWhiteSpace(Model.DNI)
//            && string.IsNullOrWhiteSpace(Model.CUIT)
//            && ConsumidorFinalId.HasValue)
//        {
//            Model.CondicionIVAId = ConsumidorFinalId.Value;
//        }

//        _editContext = new EditContext(Model);
//        _messages = new ValidationMessageStore(_editContext);

//        _editContext.OnValidationRequested += (_, __) => ValidarTodo();
//        _editContext.OnFieldChanged += (_, args) => ValidarCampo(args.FieldIdentifier);
//    }

//    // ====== Validaciones ======
//    void ValidarTodo()
//    {
//        _messages.Clear();
//        ValidarEmail();
//        ValidarDniCuitFisica();
//        ValidarCondicionIvaConCuit();
//        _editContext.NotifyValidationStateChanged();
//    }

//    void ValidarCampo(FieldIdentifier field)
//    {
//        if (field.FieldName == nameof(Model.Email))
//        {
//            _messages.Clear(field);
//            ValidarEmail();
//        }
//        if (field.FieldName is nameof(Model.DNI) or nameof(Model.CUIT))
//        {
//            _messages.Clear(new FieldIdentifier(Model, nameof(Model.DNI)));
//            _messages.Clear(new FieldIdentifier(Model, nameof(Model.CUIT)));
//            ValidarDniCuitFisica();
//            ValidarCondicionIvaConCuit();
//        }
//        _editContext.NotifyValidationStateChanged();
//    }

//    void RevalidarCampo(Expression<Func<object?>> accessor)
//    {
//        var fi = FieldIdentifier.Create(accessor);
//        ValidarCampo(fi);
//    }

//    void ValidarEmail()
//    {
//        if (string.IsNullOrWhiteSpace(Model.Email)) return;
//        // tu regex “que funciona”
//        bool ok = Regex.IsMatch(Model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
//        if (!ok)
//            _messages.Add(new FieldIdentifier(Model, nameof(Model.Email)), "Email no es válido.");
//    }

//    void ValidarDniCuitFisica()
//    {
//        if (Model.Tipo != TipoPersona.Fisica) return;

//        bool tieneDni = !string.IsNullOrWhiteSpace(Model.DNI);
//        bool tieneCuit = !string.IsNullOrWhiteSpace(Model.CUIT);

//        if (!tieneDni && !tieneCuit)
//        {
//            _messages.Add(new FieldIdentifier(Model, nameof(Model.DNI)), "Ingresá DNI o CUIT.");
//            _messages.Add(new FieldIdentifier(Model, nameof(Model.CUIT)), "Ingresá DNI o CUIT.");
//        }
//        if (tieneDni && tieneCuit)
//        {
//            _messages.Add(new FieldIdentifier(Model, nameof(Model.DNI)), "No podés completar ambos (DNI y CUIT).");
//            _messages.Add(new FieldIdentifier(Model, nameof(Model.CUIT)), "No podés completar ambos (DNI y CUIT).");
//        }
//    }

//    void ValidarCondicionIvaConCuit()
//    {
//        var tieneCuit = !string.IsNullOrWhiteSpace(Model.CUIT);
//        if (tieneCuit && !(Model.CondicionIVAId is int))
//            _messages.Add(new FieldIdentifier(Model, nameof(Model.CondicionIVAId)), "Seleccioná la Condición IVA.");
//    }

//    // ====== Handlers de inputs ======
//    protected void OnTipoChanged(ChangeEventArgs e)
//    {
//        if (!Enum.TryParse<TipoPersona>(e.Value?.ToString(), out var tipo)) tipo = TipoPersona.Fisica;
//        Model.Tipo = tipo;

//        if (Model.Tipo == TipoPersona.Fisica)
//        {
//            Model.RazonSocial = string.Empty;
//        }
//        else
//        {
//            Model.DNI = string.Empty;
//            Model.Nombre = string.Empty;
//            Model.Apellido = string.Empty;
//        }
//        _editContext.NotifyFieldChanged(FieldIdentifier.Create(() => Model.Tipo));
//    }

//    protected void OnDniChanged(ChangeEventArgs e)
//    {
//        Model.DNI = e.Value?.ToString() ?? string.Empty;
//        if (!string.IsNullOrWhiteSpace(Model.DNI))
//        {
//            Model.CUIT = string.Empty;
//            if (ConsumidorFinalId.HasValue) Model.CondicionIVAId = ConsumidorFinalId.Value;
//        }
//        RevalidarCampo(() => Model.DNI);
//    }

//    protected void OnCuitChanged()
//    {
//        var limpio = LimpiarCUIT(Model.CUIT);
//        if (!string.IsNullOrWhiteSpace(limpio))
//        {
//            Model.DNI = string.Empty;
//        }
//        RevalidarCampo(() => Model.CUIT);
//    }

//    // ====== CUIT helpers (los tuyos) ======
//    protected string LimpiarCUIT(string cuit) =>
//        new string((cuit ?? "").Where(char.IsDigit).ToArray());

//    protected string FormatearCUIT(string cuitPlano)
//    {
//        if (string.IsNullOrEmpty(cuitPlano)) return "";
//        if (cuitPlano.Length == 11) return $"{cuitPlano[..2]}-{cuitPlano.Substring(2, 8)}-{cuitPlano[^1..]}";
//        if (cuitPlano.Length > 2) return $"{cuitPlano[..2]}-{cuitPlano.Substring(2)}";
//        return cuitPlano;
//    }

//    protected void FormatearCuitEnTiempoReal(ChangeEventArgs e)
//    {
//        var limpio = LimpiarCUIT(e.Value?.ToString() ?? "");
//        if (limpio.Length <= 11)
//            Model.CUIT = FormatearCUIT(limpio);
//    }

//    protected void ValidarCuit(FocusEventArgs _)
//    {
//        var limpio = LimpiarCUIT(Model.CUIT);
//        if (!EsCuitValido(limpio) && !string.IsNullOrWhiteSpace(limpio))
//            CuitError = "CUIT inválido. Verifique los 11 dígitos y el dígito verificador.";
//        else
//            CuitError = null;

//        // Guardar el CUIT sin guiones
//        Model.CUIT = limpio;
//    }

//    protected bool EsCuitValido(string cuit)
//    {
//        if (string.IsNullOrWhiteSpace(cuit) || cuit.Length != 11) return false;
//        var coef = new[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
//        var suma = cuit.Take(10).Select((c, i) => (c - '0') * coef[i]).Sum();
//        var resto = 11 - (suma % 11);
//        if (resto == 11) resto = 0;
//        if (resto == 10) resto = 9;
//        return (cuit[10] - '0') == resto;
//    }

//    // ====== AFIP (opcional) ======
//    protected async Task ConsultarAfip()
//    {
//        var cuitLimpio = LimpiarCUIT(Model.CUIT);
//        if (string.IsNullOrWhiteSpace(cuitLimpio) || cuitLimpio.Length != 11)
//        {
//            await JS.InvokeVoidAsync("mostrarToast", "❌ Ingrese un CUIT válido de 11 dígitos.", "error");
//            return;
//        }

//        var usuarioIdStr = await JS.InvokeAsync<string>("localStorage.getItem", "usuario_id");
//        int usuarioId = string.IsNullOrEmpty(usuarioIdStr) ? 0 : int.Parse(usuarioIdStr);

//        var (datos, error) = await AfipAuthService.ConsultarConstanciaAsync(usuarioId, cuitLimpio);
//        if (!string.IsNullOrEmpty(error))
//        {
//            await JS.InvokeVoidAsync("mostrarToast", $"❌ {error}", "error");
//            return;
//        }
//        if (datos == null)
//        {
//            await JS.InvokeVoidAsync("mostrarToast", "⚠️ No se pudo consultar AFIP.", "warning");
//            return;
//        }

//        Model.Tipo = datos.TipoPersona?.Equals("FISICA", StringComparison.OrdinalIgnoreCase) == true
//            ? TipoPersona.Fisica : TipoPersona.Juridica;

//        Model.Nombre = datos.Nombre ?? Model.Nombre;
//        Model.Apellido = datos.Apellido ?? Model.Apellido;
//        Model.RazonSocial = datos.RazonSocial ?? Model.RazonSocial;
//        Model.Domicilio = datos.Direccion ?? Model.Domicilio;
//        Model.CodigoPostal = datos.CodPostal ?? Model.CodigoPostal;
//        Model.Provincia = datos.DescripcionProvincia ?? Model.Provincia;
//        Model.Ciudad = datos.Localidad ?? Model.Ciudad;

//        Model.CondicionIVAId = datos.IdImpuesto switch
//        {
//            30 => CondicionesIVA.FirstOrDefault(c => c.Nombre!.Contains("Responsable", StringComparison.OrdinalIgnoreCase))?.Id ?? Model.CondicionIVAId,
//            20 => CondicionesIVA.FirstOrDefault(c => c.Nombre!.Contains("Monotributo", StringComparison.OrdinalIgnoreCase))?.Id ?? Model.CondicionIVAId,
//            32 => CondicionesIVA.FirstOrDefault(c => c.Nombre!.Contains("Exento", StringComparison.OrdinalIgnoreCase))?.Id ?? Model.CondicionIVAId,
//            34 => CondicionesIVA.FirstOrDefault(c => c.Nombre!.Contains("No Alcanzado", StringComparison.OrdinalIgnoreCase))?.Id ?? Model.CondicionIVAId,
//            _ => Model.CondicionIVAId
//        };

//        _editContext.NotifyValidationStateChanged();
//        StateHasChanged();
//    }

//    // ====== Submit / Cancel ======
//    async Task OnSubmitInternal()
//    {
//        ValidarTodo();
//        if (!_editContext.Validate()) return;

//        if (Id.HasValue)
//        {
//            var patch = new PersonaPatchDto
//            {
//                Tipo = Model.Tipo.ToString(),
//                Apellido = Model.Apellido,
//                Nombre = Model.Nombre,
//                ClasificacionId = Model.ClasificacionId,
//                DNI = Model.DNI,
//                Nacionalidad = Model.Nacionalidad,
//                FechaNacimiento = Model.FechaNacimiento,
//                EstadoCivil = Model.EstadoCivil,
//                CUIT = Model.CUIT,
//                RazonSocial = Model.RazonSocial,
//                CondicionIVAId = Model.CondicionIVAId,
//                CondicionIIBB = Model.CondicionIIBB,
//                Domicilio = Model.Domicilio,
//                Ciudad = Model.Ciudad,
//                CodigoPostal = Model.CodigoPostal,
//                Provincia = Model.Provincia,
//                Telefono = Model.Telefono,
//                Celular = Model.Celular,
//                Email = Model.Email,
//                Observacion = Model.Observacion
//            };
//            await PersonaService.ActualizarPersonaAsync(Model.Id, patch);
//        }
//        else
//        {
//            await PersonaService.CrearPersonaAsync(Model);
//        }

//        await JS.InvokeVoidAsync("mostrarToast", "✅ Persona guardada correctamente.", "success");
//        Win.Close(WindowId);
//    }

//    void Cancelar() => Win.Close(WindowId);
//}




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
    public class CrearEditarPersonaBaseDialog : ComponentBase
    {
        [Inject] protected PersonaService PersonaService { get; set; } = default!;
        [Inject] protected CondicionIvaService CondicionIvaService { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected IJSRuntime JS { get; set; } = default!;
        [Inject] IurixBlazor.Services.Windowing.WindowService Win { get; set; } = default!;
        [Inject] protected AfipAuthService AfipAuthService { get; set; } = default!;
        [Inject] protected UsuarioService UsuarioService { get; set; } = default!; // Para obtener usuario logueado
        [Inject] protected PersonaEventService PersonaEventService { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected PersonaDto Persona { get; set; } = new();
        protected List<ClasificacionPersonaDto> Clasificaciones { get; set; } = new();
        protected List<CondicionIvaDto> CondicionesIVA { get; set; } = new();
        protected List<string> TiposPersona { get; set; } = Enum.GetNames(typeof(TipoPersona)).ToList();


        // Form model
        protected PersonaFormModel Form { get; set; } = new();



        protected string? CuitError { get; set; }


        // Flags de UI
        protected bool DisableDNI => Form.Tipo == TipoPersona.Fisica && !string.IsNullOrWhiteSpace(Form.CUIT);
        protected bool DisableCUIT => Form.Tipo == TipoPersona.Fisica && !string.IsNullOrWhiteSpace(Form.DNI);
        protected bool DebeCondicionIva =>
            Form.Tipo == TipoPersona.Juridica || !string.IsNullOrWhiteSpace(Form.CUIT);

        [Parameter] public Guid WindowId { get; set; }


        protected EditContext EditContext { get; set; } = default!;



        private bool _tieneCambios;

        public CrearEditarPersonaBaseDialog()
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
                //Form.CUIT = string.IsNullOrWhiteSpace(persona.CUIT) ? null : persona.CUIT;
                //  formateo inmediato al cargar:
                SetCuit(persona.CUIT, triggerChanged: true);

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
                //Persona.Tipo = TipoPersona.Fisica; // Valor por defecto
                //Persona.ClasificacionId = Clasificaciones.FirstOrDefault(c => c.Nombre.Equals("Cliente", StringComparison.OrdinalIgnoreCase))?.Id
                //                  ?? Clasificaciones.FirstOrDefault()?.Id
                //                  ?? 0;
                //Persona.CondicionIVAId = CondicionesIVA.FirstOrDefault(c => c.Nombre!.Equals("Consumidor Final", StringComparison.OrdinalIgnoreCase))?.Id
                //                  ?? CondicionesIVA.FirstOrDefault()?.Id
                //                  ?? 0;
                Form.Tipo = TipoPersona.Fisica;
                Form.ClasificacionId = Clasificaciones.FirstOrDefault(c => c.Nombre.Equals("Cliente", StringComparison.OrdinalIgnoreCase))?.Id
                                       ?? Clasificaciones.FirstOrDefault()?.Id;
                Form.CondicionIVAId = null; // En física sin CUIT, no es obligatoria

            }

            // 👇 EditContext ligado al Form
            EditContext = new EditContext(Form);

            // 👇 Marcar cambios cuando se edita cualquier campo
            EditContext.OnFieldChanged += async (sender, e) =>
            {
                if (!_tieneCambios)
                {
                    _tieneCambios = true;
                    //await JS.InvokeVoidAsync("setUnsavedChanges", true);
                }
            }
            ;
        }






        protected async Task ConfirmarSalida(LocationChangingContext context)
        {
            if (!_tieneCambios)
                return; // no hay cambios, dejar navegar

            var confirmar = await JS.InvokeAsync<bool>(
                "confirm",
                "Hay cambios sin guardar en la persona. ¿Seguro que querés salir y perderlos?"
            );

            if (!confirmar)
            {
                context.PreventNavigation();
            }
        }






        // Helpers
        private static string SoloDigitos(string? s) => new string((s ?? "").Where(char.IsDigit).ToArray());
        private static string FormatearCUIT11(string digits)
        {
            digits = SoloDigitos(digits);
            if (digits.Length <= 2) return digits;
            if (digits.Length <= 10) return $"{digits[..2]}-{digits[2..]}";
            return $"{digits[..2]}-{digits.Substring(2, 8)}-{digits[10]}"; // XX-XXXXXXXX-X
        }

        // Invocado por @oninput (recibe ChangeEventArgs)
        protected void HandleCuitInput(ChangeEventArgs e)
        {
            var raw = e?.Value?.ToString();
            var digits = SoloDigitos(raw);
            if (digits.Length > 11) digits = digits[..11];

            // Mostrar con guiones mientras se tipea
            Form.CUIT = FormatearCUIT11(digits);

            // Si cargó CUIT, opcionalmente limpiá DNI para evitar conflictos
            if (!string.IsNullOrWhiteSpace(digits)) Form.DNI = null;

            // limpiá mensaje visual mientras escribe
            CuitError = string.Empty;
        }

        // Valida en blur (recibe FocusEventArgs)
        protected void ValidarCuit(FocusEventArgs _)
        {
            var digits = SoloDigitos(Form.CUIT);

            // sin nada -> limpiá error y salí
            if (string.IsNullOrWhiteSpace(digits))
            {
                CuitError = string.Empty;
                StateHasChanged();
                return;
            }

            if (digits.Length != 11)
            {
                CuitError = "CUIT debe tener 11 dígitos.";
                StateHasChanged();
                return;
            }

            if (!EsCuitValido(digits))
            {
                CuitError = "CUIT inválido. Verifique los 11 dígitos y el dígito verificador.";
                StateHasChanged();
                return;
            }

            // OK: dejá formateado con guiones para la UI
            Form.CUIT = FormatearCUIT11(digits);
            CuitError = string.Empty;
            StateHasChanged();
        }

        // Verificador AFIP
        protected bool EsCuitValido(string cuitDigits)
        {
            if (string.IsNullOrWhiteSpace(cuitDigits) || cuitDigits.Length != 11) return false;
            var coef = new[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            var suma = cuitDigits.Take(10).Select((c, i) => (c - '0') * coef[i]).Sum();
            var resto = 11 - (suma % 11);
            if (resto == 11) resto = 0;
            if (resto == 10) resto = 9;
            return (cuitDigits[10] - '0') == resto;
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


        //protected void FormatearCuitEnTiempoReal(ChangeEventArgs e)
        //{
        //    var limpio = LimpiarCUIT(e.Value?.ToString() ?? "");
        //    if (limpio.Length <= 11)
        //        Form.CUIT = FormatearCUIT(limpio); // Muestra con guiones
        //}

        //protected void ValidarCuit(FocusEventArgs e)
        //{
        //    var limpio = LimpiarCUIT(Persona.CUIT);
        //    if (!EsCuitValido(limpio) && !string.IsNullOrWhiteSpace(limpio))
        //        CuitError = "CUIT inválido. Verifique los 11 dígitos y el dígito verificador.";
        //    else
        //        CuitError = null;

        //    // Siempre guardar el CUIT sin guiones
        //    Persona.CUIT = limpio;
        //}







        //protected string LimpiarCUIT(string cuit) =>
        //    new string(cuit.Where(char.IsDigit).ToArray());

        //protected string FormatearCUIT(string cuitPlano)
        //{
        //    if (cuitPlano.Length == 11)
        //        return $"{cuitPlano[..2]}-{cuitPlano.Substring(2, 8)}-{cuitPlano[^1..]}";
        //    if (cuitPlano.Length > 2)
        //        return $"{cuitPlano[..2]}-{cuitPlano.Substring(2)}";
        //    return cuitPlano;
        //}



        //protected void HandleCuitInput(ChangeEventArgs e)
        //{
        //    var texto = e?.Value?.ToString() ?? string.Empty;

        //    // Normalizá a dígitos, recortá a 11
        //    var limpio = LimpiarCUIT(texto);
        //    if (limpio.Length > 11) limpio = limpio[..11];

        //    // Reaplicá formateo con guiones
        //    Form.CUIT = FormatearCUIT(limpio);

        //    OnCuitChanged(); // 👈 lo que te faltaba
        //}




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






        private static string LimpiarCUIT(string s)
    => new string((s ?? string.Empty).Where(char.IsDigit).ToArray());

        private static string FormatearCUIT(string d)
        {
            d = LimpiarCUIT(d);
            if (d.Length <= 2) return d;
            if (d.Length <= 10) return $"{d[..2]}-{d[2..]}";
            return $"{d[..2]}-{d.Substring(2, 8)}-{d[10]}"; // XX-XXXXXXXX-X
        }

        // ← Usalo en carga y en oninput
        private void SetCuit(string? raw, bool triggerChanged = true)
        {
            var limpio = LimpiarCUIT(raw ?? "");
            if (limpio.Length > 11) limpio = limpio[..11];
            Form.CUIT = FormatearCUIT(limpio);
            if (triggerChanged) OnCuitChanged();
        }

        //protected void HandleCuitInput(ChangeEventArgs e)
        //{
        //    SetCuit(e?.Value?.ToString(), triggerChanged: true);
        //}

        // lo que ya tenías, pero ahora llama a SetCuit:
        protected void FormatearCuitEnTiempoReal(ChangeEventArgs e) => SetCuit(e?.Value?.ToString());





        // Validador CUIT (algoritmo dígito verificador AFIP)
        //protected bool EsCuitValido(string cuit)
        //{
        //    if (string.IsNullOrWhiteSpace(cuit) || cuit.Length != 11) return false;
        //    var coef = new[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
        //    var suma = cuit.Take(10).Select((c, i) => (c - '0') * coef[i]).Sum();
        //    var resto = 11 - (suma % 11);
        //    if (resto == 11) resto = 0;
        //    if (resto == 10) resto = 9;
        //    return (cuit[10] - '0') == resto;
        //}




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




        //protected async Task ConsultarAfip()
        //{
        //    var cuitLimpio = new string(Persona.CUIT.Where(char.IsDigit).ToArray());

        //    if (string.IsNullOrWhiteSpace(cuitLimpio) || cuitLimpio.Length != 11)
        //    {
        //        await JS.InvokeVoidAsync("mostrarToast", "❌ Ingrese un CUIT válido de 11 dígitos.", "error");
        //        return;
        //    }

        //    var usuarioIdStr = await JS.InvokeAsync<string>("localStorage.getItem", "usuario_id");
        //    int usuarioId = string.IsNullOrEmpty(usuarioIdStr) ? 0 : int.Parse(usuarioIdStr);

        //    System.Diagnostics.Debug.WriteLine($"Usuario Id: {usuarioId}");

        //    var (datos, error) = await AfipAuthService.ConsultarConstanciaAsync(usuarioId, cuitLimpio);

        //    if (!string.IsNullOrEmpty(error))
        //    {
        //        await JS.InvokeVoidAsync("mostrarToast", $"❌ {error}", "error");
        //        return;
        //    }

        //    if (datos == null)
        //    {
        //        await JS.InvokeVoidAsync("mostrarToast", "⚠️ No se pudo consultar AFIP.", "warning");
        //        return;
        //    }

        //    if (!string.IsNullOrEmpty(datos.TipoPersona))
        //    {
        //        Persona.Tipo = datos.TipoPersona.Equals("FISICA", StringComparison.OrdinalIgnoreCase)
        //            ? TipoPersona.Fisica
        //            : TipoPersona.Juridica;
        //    }


        //    // Autocompletar datos en Persona
        //    Persona.Nombre = datos.Nombre ?? Persona.Nombre;
        //        Persona.Apellido = datos.Apellido ?? Persona.Apellido;
        //        Persona.RazonSocial = datos.RazonSocial ?? Persona.RazonSocial;
        //        Persona.Domicilio = datos.Direccion ?? Persona.Domicilio;
        //        Persona.CodigoPostal = datos.CodPostal ?? Persona.CodigoPostal;
        //        Persona.Provincia = datos.DescripcionProvincia ?? Persona.Provincia;
        //        Persona.Ciudad = datos.Localidad ?? Persona.Ciudad;

        //        Persona.CondicionIVAId = datos.IdImpuesto switch
        //        {
        //            30 => CondicionesIVA.FirstOrDefault(c => c.Nombre!.Contains("Responsable", StringComparison.OrdinalIgnoreCase))?.Id,
        //            20 => CondicionesIVA.FirstOrDefault(c => c.Nombre!.Contains("Monotributo", StringComparison.OrdinalIgnoreCase))?.Id,
        //            32 => CondicionesIVA.FirstOrDefault(c => c.Nombre!.Contains("Exento", StringComparison.OrdinalIgnoreCase))?.Id,
        //            34 => CondicionesIVA.FirstOrDefault(c => c.Nombre!.Contains("No Alcanzado", StringComparison.OrdinalIgnoreCase))?.Id,
        //            _ => Persona.CondicionIVAId
        //        };

        //    }


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





        //protected async Task Guardar()
        //{
        //    if (Id.HasValue)
        //    {
        //        var patch = new PersonaPatchDto
        //        {
        //            Nombre = Persona.Nombre,
        //            Apellido = Persona.Apellido,
        //            CUIT = Persona.CUIT,
        //            RazonSocial = Persona.RazonSocial,
        //            Nacionalidad = Persona.Nacionalidad,
        //            FechaNacimiento = Persona.FechaNacimiento,
        //            EstadoCivil = Persona.EstadoCivil,                    
        //            Domicilio = Persona.Domicilio,
        //            Ciudad = Persona.Ciudad,
        //            CodigoPostal = Persona.CodigoPostal,
        //            Provincia = Persona.Provincia,
        //            Telefono = Persona.Telefono,
        //            Celular = Persona.Celular,
        //            Email = Persona.Email,
        //            Tipo = Persona.Tipo.ToString(),
        //            DNI = Persona.DNI,
        //            CondicionIIBB = Persona.CondicionIIBB,
        //            ClasificacionId = Persona.ClasificacionId,
        //            CondicionIVAId = Persona.CondicionIVAId,
        //            Observacion = Persona.Observacion,
        //        };
        //        await PersonaService.ActualizarPersonaAsync(Persona.Id, patch);
        //    }
        //    else
        //    {
        //        await PersonaService.CrearPersonaAsync(Persona);
        //    }

        //    await JS.InvokeVoidAsync("mostrarToast", "✅ Persona guardada correctamente.", "success");
        //    Nav.NavigateTo("/personas");
        //}



    //    private static string SoloDigitos(string? s)
    //=> new string((s ?? string.Empty).Where(char.IsDigit).ToArray());


        protected async Task Guardar()
        {
            string Digits(string? s) => new string((s ?? "").Where(char.IsDigit).ToArray());
            string? NullOrEmptyToEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? "" : s!.Trim();
                        
            // Normalizar antes de armar el DTO/PATCH
            var cuitDigits = SoloDigitos(Form.CUIT);
            string? cuitToSend = string.IsNullOrWhiteSpace(cuitDigits) ? null : cuitDigits;

            var dniDigits = SoloDigitos(Form.DNI);
            string? dniToSend = string.IsNullOrWhiteSpace(dniDigits) ? null : dniDigits;
            
            // Si llega acá, el form pasó validaciones de DataAnnotations/IValidatableObject
            if (Id.HasValue)
            {
                var patch = new PersonaPatchDto
                {
                    

                    Tipo = Form.Tipo.ToString(),
                    Nombre = NullOrEmptyToEmpty(Form.Nombre),
                    Apellido = NullOrEmptyToEmpty(Form.Apellido),
                    RazonSocial = NullOrEmptyToEmpty(Form.RazonSocial),
                    DNI = dniToSend,      // "" = limpiar -> backend lo convierte a null
                    CUIT = cuitToSend,     // "" = limpiar -> backend lo convierte a null
                    ClasificacionId = Form.ClasificacionId,
                    CondicionIVAId = Form.CondicionIVAId,
                    Email = NullOrEmptyToEmpty(Form.Email),
                    Nacionalidad = NullOrEmptyToEmpty(Form.Nacionalidad),
                    FechaNacimiento = Form.FechaNacimiento,
                    EstadoCivil = NullOrEmptyToEmpty(Form.EstadoCivil),
                    CondicionIIBB = NullOrEmptyToEmpty(Form.CondicionIIBB),
                    Domicilio = NullOrEmptyToEmpty(Form.Domicilio),
                    Ciudad = NullOrEmptyToEmpty(Form.Ciudad),
                    CodigoPostal = NullOrEmptyToEmpty(Form.CodigoPostal),
                    Provincia = NullOrEmptyToEmpty(Form.Provincia),
                    Telefono = NullOrEmptyToEmpty(Form.Telefono),
                    Celular = NullOrEmptyToEmpty(Form.Celular),
                    Observacion = NullOrEmptyToEmpty(Form.Observacion)
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
                    //DNI = Form.DNI ?? string.Empty,
                    DNI = dniToSend ?? string.Empty,
                    //CUIT = Form.CUIT ?? string.Empty,
                    CUIT = cuitToSend ?? string.Empty,
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


            // 👇 Ya no hay cambios pendientes
            _tieneCambios = false;
            //await JS.InvokeVoidAsync("setUnsavedChanges.set", false);

            // 1. Notifica a otros componentes que una persona fue guardada
            PersonaEventService.PersonaSaved();

            //await JS.InvokeVoidAsync("mostrarToast", "✅ Persona guardada correctamente.", "success");

            // 3. Añade un pequeño retraso (e.g., 300 milisegundos) para que el toast sea visible.
            //    Este tiempo permite que la UI se actualice en el navegador.
            // Llamas a este nuevo método que se encarga del cierre ordenado
            await CerrarVentanaConToast();
        }


        protected async Task CerrarVentanaConToast()
        {
            

            _tieneCambios = false;
            //await JS.InvokeVoidAsync("setUnsavedChanges", false);

            await JS.InvokeVoidAsync("mostrarToast", "✅ Persona guardada correctamente.", "success");
            await Task.Delay(300); // Pequeña pausa para asegurar la visualización

            Win.Close(WindowId);
        }



        //protected void OnTipoChanged(ChangeEventArgs e)
        //{
        //    if (Enum.TryParse<TipoPersona>(e.Value?.ToString(), out var tipoSeleccionado))
        //    {
        //        Persona.Tipo = tipoSeleccionado;
        //    }
        //    else
        //    {
        //        Persona.Tipo = TipoPersona.Fisica; // Valor por defecto
        //    }
        //}





        //public void Cancelar() => Win.Close(WindowId);

        protected async void Cancelar()
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

            
            //Nav.NavigateTo("/personas");
            Win.Close(WindowId);
        }

    }
}