using System;

namespace IurixBlazor.Shared.Helpers
{
    //public static class LicenciaHelper
    //{
    //    public static string CalcularEstado(DateTime validoHasta)
    //    {
    //        var dias = (validoHasta.Date - DateTime.UtcNow.Date).TotalDays;
    //        if (dias <= 0) return "Vencida";
    //        if (dias <= 15) return "Por vencer";
    //        return "Vigente";
    //    }
    //}

    public static class LicenciaHelper
    {
        public static string CalcularEstado(DateTime? validoHasta)
        {
            if (!validoHasta.HasValue)
                return "Sin licencia";

            var dias = (validoHasta.Value.Date - DateTime.UtcNow.Date).TotalDays;

            if (dias <= 0) return "Vencida";
            if (dias <= 15) return "Por vencer";
            return "Vigente";
        }
    }

}