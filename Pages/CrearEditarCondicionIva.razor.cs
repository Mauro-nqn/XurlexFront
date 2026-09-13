using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Text.Json.Serialization;

namespace IurixBlazor.Pages
{
    public class CrearEditarCondicionIvaBase : ComponentBase
    {
        [Inject] protected CondicionIvaService? CondicionIvaService { get; set; }
        [Inject] protected NavigationManager? Nav { get; set; }

        [Inject] private IJSRuntime? JS { get; set; }

        [Parameter] public int? Id { get; set; }
        protected CondicionIvaDto Condicion { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            if (Id.HasValue)
            {
                var dto = await CondicionIvaService!.ObtenerPorIdAsync(Id.Value);
                if (dto != null)
                    Condicion = dto;
            }
        }

        protected async Task Guardar()
        {
            if (Id.HasValue)
                await CondicionIvaService!.ActualizarAsync(Condicion.Id, Condicion);
            else
                await CondicionIvaService!.CrearAsync(Condicion);

            Nav!.NavigateTo("/condicioniva");
        }

        protected async Task Eliminar()
        {
            if (Id.HasValue)
            {
                var confirmar = await JS!.InvokeAsync<bool>("confirm", $"¿Eliminar {Condicion.Nombre}?");
                if (confirmar)
                {
                    await CondicionIvaService!.EliminarAsync(Id.Value);
                    Nav!.NavigateTo("/condicioniva");
                }
            }
        }

        protected void Cancelar() => Nav!.NavigateTo("/condicioniva");
    }
}
