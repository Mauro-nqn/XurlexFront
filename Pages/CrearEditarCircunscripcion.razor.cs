using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Enums;
using IurixBlazor.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Runtime.Serialization;

using static IurixBlazor.Shared.Helpers.DextraEnumHelper;

public class CrearEditarCircunscripcionBase : ComponentBase
{
    [Inject] protected CircunscripcionService? Service { get; set; }
    [Inject] protected JurisdiccionService? JurisdiccionService { get; set; }
    [Inject] protected NavigationManager? Nav { get; set; }
    [Inject] protected IJSRuntime? JS { get; set; }

    [Parameter] public int? Id { get; set; }

    // ✅ Usamos el DTO de creación/actualización para el formulario
    protected CrearCircunscripcionDto Circunscripcion { get; set; } = new();

    protected List<JurisdiccionDto> Jurisdicciones { get; set; } = new();


    public string? DextraCiudadExacta { get; set; }   // ← texto exacto para Dextra (p.ej. "NEUQUEN")

    //protected bool EsNeuquen =>
    //Jurisdicciones?.FirstOrDefault(j => j.Id == Circunscripcion.JurisdiccionId) is { } j
    //&& IsNeuquenByName(j.Nombre);

    //protected IEnumerable<(int val, string label)> CiudadesDextra => Enum.GetValues(typeof(CiudadDextra))
    //    .Cast<CiudadDextra>()
    //    .Select(e => ((int)e, e.ToExactString()));

    // Opciones para el combo (value=string exacto; label=lo que se ve)
    protected IEnumerable<(string val, string label)> CiudadesDextra =>
        Enum.GetValues(typeof(CiudadDextra))
            .Cast<CiudadDextra>()
            .Select(e => (e.ToExactString(), /*Pretty*/(e.ToExactString())));

    // (Opcional) label “linda” para mostrar, pero el value queda exacto
    //private static string Pretty(string s)
    //{
    //    // podés dejarlo como viene si preferís
    //    var ti = System.Globalization.CultureInfo.GetCultureInfo("es-AR").TextInfo;
    //    var t = ti.ToTitleCase(s.ToLowerInvariant());
    //    return t.Replace(" De ", " de ").Replace(" Del ", " del ");
    //}

    // Neuquén case/acentos-insensitive
    protected bool EsNeuquen =>
        Jurisdicciones?.FirstOrDefault(j => j.Id == Circunscripcion.JurisdiccionId) is { } j
        && IsNeuquenByName(j.Nombre);

    protected override async Task OnInitializedAsync()
    {
        Jurisdicciones = await JurisdiccionService!.ObtenerTodosAsync();

        if (Id.HasValue)
        {
            // Obtenemos el DTO de lectura y lo convertimos en DTO de escritura
            var dto = await Service!.ObtenerPorIdAsync(Id.Value);
            if (dto != null)
            {
                Circunscripcion = new CrearCircunscripcionDto
                {
                    Nombre = dto.Nombre,
                    JurisdiccionId = dto.JurisdiccionId,
                    CiudadDextra = dto.CiudadDextra
                };
            }
        }
    }

    protected async Task Guardar()
    {
        if (Id.HasValue)
        {
            await Service!.ActualizarAsync(Id.Value, Circunscripcion);
        }
        else
        {
            await Service!.CrearAsync(Circunscripcion);
        }

        Nav!.NavigateTo("/circunscripciones");
    }

    protected async Task Eliminar()
    {
        if (Id.HasValue)
        {
            var confirmar = await JS!.InvokeAsync<bool>("confirm", $"¿Eliminar {Circunscripcion.Nombre}?");
            if (confirmar)
            {
                await Service!.EliminarAsync(Id.Value);
                Nav!.NavigateTo("/circunscripciones");
            }
        }
    }

    protected void Cancelar() => Nav!.NavigateTo("/circunscripciones");
}

