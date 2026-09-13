using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace IurixBlazor.Shared.Helpers
{
    public static class DextraEnumHelper
    {
        // Devuelve EnumMember.Value si existe; si no, el nombre del enum
        public static string ToExactString<T>(this T value) where T : struct, Enum
        {
            var name = value.ToString();
            var mem = typeof(T).GetMember(name).FirstOrDefault();
            var attr = mem?.GetCustomAttribute<EnumMemberAttribute>();
            return attr?.Value ?? name!;
        }

        // Compat: match por "exacto" (EnumMember.Value) ignorando mayúsculas
        public static bool TryFromExactString<T>(string? s, out T result) where T : struct, Enum
            => EnumCache<T>.TryFromExact(s, out result);

        // Recomendado para AutoMapper (no usa out)
        public static T? ParseNullable<T>(string? s) where T : struct, Enum
            => EnumCache<T>.ParseNullable(s);

        public static T ParseOrDefault<T>(string? s, T @default = default) where T : struct, Enum
            => EnumCache<T>.ParseOrDefault(s, @default);

        // Normalizador general reutilizable
        public static string NormalizeNoAccents(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;

            // 1) minúsculas
            var lower = s.ToLowerInvariant();

            // 2) reemplazos rápidos de separadores comunes
            lower = lower.Replace('_', ' ').Replace('-', ' ');

            // 3) quitar acentos (FormD + filtrar marcas no espaciadoras)
            var formD = lower.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(formD.Length);
            foreach (var ch in formD)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
            }
            var noAccents = sb.ToString().Normalize(NormalizationForm.FormC);

            // 4) filtrar a solo [a-z0-9 y espacio]
            sb.Clear();
            foreach (var ch in noAccents)
            {
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == ' ')
                    sb.Append(ch);
            }

            // 5) colapsar espacios múltiples
            var compact = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), @"\s+", " ").Trim();

            return compact;
        }

        public static bool IsNeuquenByName(string? nombreJurisdiccion)
            => NormalizeNoAccents(nombreJurisdiccion) == "neuquen";



        public static bool AplicaACirc(this AplicacionDextra e, string circKeyNormalizada)
        {
            var mem = typeof(AplicacionDextra).GetMember(e.ToString()).FirstOrDefault();
            if (mem is null) return false;

            var attrs = mem.GetCustomAttributes(typeof(IurixBlazor.Shared.Enums.CircunscripcionAttribute), inherit: false)
                           .Cast<IurixBlazor.Shared.Enums.CircunscripcionAttribute>();

            foreach (var a in attrs)
            {
                var cand = NormalizeNoAccents(a.Nombre).ToUpperInvariant();
                if (!string.IsNullOrEmpty(cand) &&
                    (circKeyNormalizada.Contains(cand) || cand.Contains(circKeyNormalizada)))
                    return true;
            }
            return false;
        }

        // Helpers de UI: opciones (valor/etiqueta) filtradas por circunscripción
        public static IEnumerable<(int val, string label)> DextraOptionsForCirc(string? circNombre)
        {
            var key = NormalizeNoAccents(circNombre).ToUpperInvariant();
            if (string.IsNullOrEmpty(key)) return Enumerable.Empty<(int, string)>();

            return Enum.GetValues(typeof(AplicacionDextra))
                       .Cast<AplicacionDextra>()
                       .Where(e => e.AplicaACirc(key))
                       .Select(e => ((int)e, e.ToExactString()));
        }



        // === Cache por enum ===
        private static class EnumCache<T> where T : struct, Enum
        {
            // exact: EnumMember.Value (o name) tal cual -> enum (case-insensitive)
            private static readonly Dictionary<string, T> _byExact =
                new(StringComparer.OrdinalIgnoreCase);

            // normalized: normalización de EnumMember.Value y del Name -> enum
            private static readonly Dictionary<string, T> _byNormalized =
                new(StringComparer.Ordinal);

            static EnumCache()
            {
                foreach (var v in Enum.GetValues(typeof(T)).Cast<T>())
                {
                    var name = v.ToString()!;
                    var mem = typeof(T).GetMember(name).FirstOrDefault();
                    var attr = mem?.GetCustomAttribute<EnumMemberAttribute>();
                    var exact = attr?.Value ?? name;

                    // índice exacto (para TryFromExactString)
                    if (!_byExact.ContainsKey(exact))
                        _byExact.Add(exact, v);

                    // índices normalizados: tanto el exacto como el nombre
                    var n1 = DextraEnumHelper.NormalizeNoAccents(exact);
                    var n2 = DextraEnumHelper.NormalizeNoAccents(name);

                    if (!string.IsNullOrEmpty(n1) && !_byNormalized.ContainsKey(n1))
                        _byNormalized.Add(n1, v);

                    if (!string.IsNullOrEmpty(n2) && !_byNormalized.ContainsKey(n2))
                        _byNormalized.Add(n2, v);

                    // (Opcional) sin “la”, “de”, etc.: agregar variantes si querés
                    // p.ej., "villa la angostura" -> "villa angostura"
                    var n3 = RemoveCommonStopwords(n1);
                    if (!string.IsNullOrEmpty(n3) && !_byNormalized.ContainsKey(n3))
                        _byNormalized.Add(n3, v);
                }
            }

            public static bool TryFromExact(string? s, out T result)
            {
                result = default;
                if (string.IsNullOrWhiteSpace(s)) return false;
                return _byExact.TryGetValue(s, out result);
            }

            public static T? ParseNullable(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return null;
                var key = DextraEnumHelper.NormalizeNoAccents(s);
                if (string.IsNullOrEmpty(key)) return null;
                return _byNormalized.TryGetValue(key, out var val) ? val : (T?)null;
            }

            public static T ParseOrDefault(string? s, T @default = default)
            {
                var parsed = ParseNullable(s);
                return parsed.HasValue ? parsed.Value : @default;
            }

            // stopwords mínimas en español que aparecen en tus enums
            private static string RemoveCommonStopwords(string text)
            {
                if (string.IsNullOrEmpty(text)) return text;
                // ojo: esto es super simple; si querés, hacelo por tokens
                var stop = new HashSet<string> { "de", "la", "del", "las", "los" };
                var tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var filtered = tokens.Where(t => !stop.Contains(t)).ToArray();
                return string.Join(' ', filtered);
            }



        }
    }
    }
