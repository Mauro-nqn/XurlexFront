namespace IurixBlazor.Shared.Enums
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class CircunscripcionAttribute : Attribute
    {
        public string Nombre { get; }
        public CircunscripcionAttribute(string nombre) => Nombre = nombre;
    }
}
