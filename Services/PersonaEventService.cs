// Services/PersonaEventService.cs

namespace IurixBlazor.Services
{
    public class PersonaEventService
    {
        public event Action? OnPersonaSaved;

        public void PersonaSaved() => OnPersonaSaved?.Invoke();
    }
}