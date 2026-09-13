using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace IurixBlazor.Pages
{
    public class ClasificacionPersonaBase : ComponentBase
    {
        [Inject] protected PersonaService PersonaService { get; set; } = default!;
        [Inject] protected IJSRuntime JS { get; set; } = default!;

        protected List<ClasificacionPersonaDto> Clasificaciones { get; set; } = new();
        protected ClasificacionPersonaDto? Seleccionada { get; set; }
        protected string NombreClasificacion { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await CargarClasificaciones();
        }

        protected async Task CargarClasificaciones()
        {
            Clasificaciones = await PersonaService.ObtenerClasificacionesAsync();
            Seleccionada = null;
            NombreClasificacion = string.Empty;
        }

        protected void SeleccionarClasificacion(ClasificacionPersonaDto clasificacion)
        {
            Seleccionada = clasificacion;
            NombreClasificacion = clasificacion.Nombre;
        }

        protected async Task GuardarClasificacion()
        {
            if (string.IsNullOrWhiteSpace(NombreClasificacion))
            {
                await JS.InvokeVoidAsync("mostrarToast", "⚠️ Debes ingresar un nombre.", "warning");
                return;
            }

            if (Seleccionada == null)
            {
                await PersonaService.CrearClasificacionAsync(new ClasificacionPersonaDto { Nombre = NombreClasificacion });
                await JS.InvokeVoidAsync("mostrarToast", "✅ Clasificación creada.", "success");
            }
            else
            {
                Seleccionada.Nombre = NombreClasificacion;
                await PersonaService.ActualizarClasificacionAsync(Seleccionada.Id, Seleccionada);
                await JS.InvokeVoidAsync("mostrarToast", "✏️ Clasificación actualizada.", "info");
            }

            await CargarClasificaciones();
        }

        protected async Task EliminarClasificacion()
        {
            if (Seleccionada != null)
            {
                bool confirmar = await JS.InvokeAsync<bool>("mostrarConfirmacion", $"¿Eliminar '{Seleccionada.Nombre}'?");
                if (confirmar)
                {
                    await PersonaService.EliminarClasificacionAsync(Seleccionada.Id);
                    await JS.InvokeVoidAsync("mostrarToast", "🗑️ Clasificación eliminada.", "success");
                    await CargarClasificaciones();
                }
            }
        }

        protected void NuevaClasificacion()
        {
            Seleccionada = null;
            NombreClasificacion = string.Empty;
        }
    }
}
