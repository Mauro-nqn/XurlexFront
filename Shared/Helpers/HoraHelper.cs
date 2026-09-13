namespace IurixBlazor.Shared.Helpers
{
    public class HoraHelper
    {

        // === Helpers de TZ para el FRONT ===
        public static TimeZoneInfo GetArgentinaTz()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Salta"); } // Linux/containers
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");     // Windows
            }
        }

        /// UTC -> AR (para mostrar en el form). Deja Kind=Unspecified (lo que espera el backend).
        public static DateTime FromUtcToArUnspecified(DateTime utc)
        {
            if (utc.Kind != DateTimeKind.Utc)
                utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);

            var ar = TimeZoneInfo.ConvertTimeFromUtc(utc, GetArgentinaTz());
            return DateTime.SpecifyKind(ar, DateTimeKind.Unspecified);
        }

        /// “mañana a las HH” en AR, con Kind=Unspecified
        public static DateTime TomorrowAtHourArUnspecified(int hour)
        {
            var nowAr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GetArgentinaTz());
            var tomorrow = nowAr.Date.AddDays(1).AddHours(hour);
            return DateTime.SpecifyKind(tomorrow, DateTimeKind.Unspecified);
        }
               

        public static DateTime FromUtcToArUnspecified(DateTimeOffset utc)
        {
            var tz = GetArgentinaTz();
            var local = TimeZoneInfo.ConvertTime(utc, tz).DateTime;
            return DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        }


        // === Zona horaria AR (Linux/Windows) ===
        

        // === “Ahora” en AR con Kind=Unspecified (lo que espera el backend desde el front) ===
        public static DateTime NowArUnspecified()
        {
            var ar = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GetArgentinaTz());
            return DateTime.SpecifyKind(ar, DateTimeKind.Unspecified);
        }

                
       

        // === AR Unspecified -> UTC (para guardar/filtrar en DB) ===
        public static DateTime ToUtcFromArUnspecified(DateTime arUnspecified)
        {
            // Asumimos que Unspecified es hora local de AR
            if (arUnspecified.Kind == DateTimeKind.Utc) return arUnspecified;
            if (arUnspecified.Kind == DateTimeKind.Local) return arUnspecified.ToUniversalTime();
            return TimeZoneInfo.ConvertTimeToUtc(arUnspecified, GetArgentinaTz());
        }

        // === Normalización general (por si te llega cualquier Kind) ===
        public static DateTime EnsureUtc(DateTime dt, TimeZoneInfo tz)
        {
            if (dt.Kind == DateTimeKind.Utc) return dt;
            if (dt.Kind == DateTimeKind.Local) return dt.ToUniversalTime();
            // Unspecified => interpretalo en tz provista (AR)
            return TimeZoneInfo.ConvertTimeToUtc(dt, tz);
        }

        // === Construcción de rangos diarios y arbitrarios en UTC (para queries) ===
        public static (DateTime inicioUtc, DateTime finUtc) DayRangeUtc(DateOnly dia)
        {
            var tz = GetArgentinaTz();
            var startLocal = new DateTime(dia.Year, dia.Month, dia.Day, 0, 0, 0, DateTimeKind.Unspecified);
            var endLocal = startLocal.AddDays(1);
            return (TimeZoneInfo.ConvertTimeToUtc(startLocal, tz),
                    TimeZoneInfo.ConvertTimeToUtc(endLocal, tz));
        }

        public static (DateTime? inicioUtc, DateTime? finUtc) RangeUtc(DateOnly? desde, DateOnly? hasta)
        {
            var tz = GetArgentinaTz();
            DateTime? ini = null, fin = null;
            if (desde.HasValue)
            {
                var d0 = new DateTime(desde.Value.Year, desde.Value.Month, desde.Value.Day, 0, 0, 0, DateTimeKind.Unspecified);
                ini = TimeZoneInfo.ConvertTimeToUtc(d0, tz);
            }
            if (hasta.HasValue)
            {
                // Exclusivo: 00:00 del día siguiente
                var h1 = new DateTime(hasta.Value.Year, hasta.Value.Month, hasta.Value.Day, 0, 0, 0, DateTimeKind.Unspecified).AddDays(1);
                fin = TimeZoneInfo.ConvertTimeToUtc(h1, tz);
            }
            return (ini, fin);
        }

       

        public static bool IsPastInAr(DateTime arUnspecified, DateTime? referenceArUnspecified = null)
        {
            var refAr = referenceArUnspecified ?? NowArUnspecified();
            return arUnspecified < refAr;
        }



    }
}
