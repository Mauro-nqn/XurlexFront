using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class CrearEditarJurisdiccionBase : ComponentBase
{
    [Inject] protected JurisdiccionService? Service { get; set; }
    [Inject] protected NavigationManager? Nav { get; set; }
    [Inject] protected IJSRuntime? JS { get; set; }

    [Parameter] public int? Id { get; set; }

    //  Usamos el DTO de escritura para el formulario
    protected CrearJurisdiccionDto CrearJurisdiccionDto { get; set; } = new();

    // ✅ También guardamos el DTO de lectura solo para mostrar datos en edición (opcional)
    protected JurisdiccionDto? Jurisdiccion { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            var dto = await Service!.ObtenerPorIdAsync(Id.Value);
            if (dto != null)
            {
                CrearJurisdiccionDto = new CrearJurisdiccionDto
                {
                    Nombre = dto.Nombre
                };
            }
        }
        else
        {
            // ✅ Inicialización explícita al crear nueva jurisdicción
            CrearJurisdiccionDto = new CrearJurisdiccionDto();
        }
    }

    protected async Task Guardar()
    {
        if (Id.HasValue)
            await Service!.ActualizarAsync(Id.Value, CrearJurisdiccionDto); // ✅ usa Id.Value y DTO de escritura
        else
            await Service!.CrearAsync(CrearJurisdiccionDto);

        Nav!.NavigateTo("/jurisdicciones");
    }

    protected async Task Eliminar()
    {
        if (Id.HasValue && Jurisdiccion != null)
        {
            var confirmar = await JS!.InvokeAsync<bool>("confirm", $"¿Eliminar {Jurisdiccion.Nombre}?");
            if (confirmar)
            {
                await Service!.EliminarAsync(Id.Value);
                Nav!.NavigateTo("/jurisdicciones");
            }
        }
    }

    protected void Cancelar() => Nav!.NavigateTo("/jurisdicciones");
}
