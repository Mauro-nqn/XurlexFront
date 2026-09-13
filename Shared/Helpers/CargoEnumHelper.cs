using System;
using IurixBlazor.Shared.Enums;


namespace IurixBlazor.Shared.Helpers
{
    public static class CargoEnumHelper
    {
        public static List<Cargo> Cargos { get; } = Enum.GetValues(typeof(Cargo)).Cast<Cargo>().ToList();
    }
}
