using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System.Globalization;

namespace IurixBlazor.Shared.Helpers
{

    public class InputDateTime<TValue> : InputDate<TValue>
    {
        private static readonly string[] AcceptFormats = new[]
 {
    "yyyy-MM-ddTHH:mm:ss",
    "yyyy-MM-ddTHH:mm"
};

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "input");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(2, "type", "datetime-local");
            builder.AddAttribute(3, "class", CssClass);
            //builder.AddAttribute(4, "value", BindConverter.FormatValue(CurrentValueAsString));
            // También el value del input:
            builder.AddAttribute(4, "value", BindConverter.FormatValue(CurrentValueAsString ?? string.Empty));
            // usar oninput en vez de onchange para evitar el flash de valor vacío en blur
            builder.AddAttribute(5, "oninput",
                 EventCallback.Factory.CreateBinder<string>(
                     this,
                     v => CurrentValueAsString = v,
                     CurrentValueAsString ?? string.Empty // 👈 evita null aquí
                 )
             );
            builder.CloseElement();
        }

        protected override string FormatValueAsString(TValue? value)
        {
            return value switch
            {
                DateTime dt => dt.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture),
                DateTimeOffset dto => dto.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture),
                _ => string.Empty
            };
        }



        protected override bool TryParseValueFromString(string? value, out TValue result, out string validationErrorMessage)
        {
            var targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

            // 1) No generar error por valor vacío (evita viñeta en blur)
            if (string.IsNullOrWhiteSpace(value))
            {
                // Mantener el valor actual y NO emitir mensaje
                result = CurrentValue!;
                validationErrorMessage = null!;
                return true;
            }

            bool success;
            if (targetType == typeof(DateTime))
            {
                success = TryParseDateTime(value, out result);
            }
            else if (targetType == typeof(DateTimeOffset))
            {
                success = TryParseDateTimeOffset(value, out result);
            }
            else
            {
                throw new InvalidOperationException($"The type '{targetType}' is not a supported date type.");
            }

            if (success)
            {
                validationErrorMessage = null!;
                return true;
            }

            // Mensaje amigable si realmente es inválido
            var defaultMsg = "Ingrese una fecha y hora válidas.";
            validationErrorMessage = string.IsNullOrWhiteSpace(ParsingErrorMessage)
                ? defaultMsg
                : string.Format(ParsingErrorMessage!, FieldIdentifier.FieldName);
            return false;
        }

        static bool TryParseDateTime(string value, out TValue result)
        {
            if (DateTime.TryParseExact(value, AcceptFormats, CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out var parsed))
            {
                result = (TValue)(object)parsed;
                return true;
            }
            result = default!;
            return false;
        }

        static bool TryParseDateTimeOffset(string value, out TValue result)
        {
            if (DateTimeOffset.TryParseExact(value, AcceptFormats, CultureInfo.InvariantCulture,
                                             DateTimeStyles.None, out var parsed))
            {
                result = (TValue)(object)parsed;
                return true;
            }
            result = default!;
            return false;
        }

    }
}