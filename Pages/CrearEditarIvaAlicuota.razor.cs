using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace IurixBlazor.Pages
{
    public class CrearEditarIvaAlicuotaBase : ComponentBase
    {
        [Inject] protected IvaAlicuotaService? IvaAlicuotaService { get; set; }
        [Inject] protected NavigationManager? Nav { get; set; }
        [Inject] private IJSRuntime? JS { get; set; }

        [Parameter] public int? Id { get; set; }
        protected IvaAlicuotaDto Alicuota { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            if (Id.HasValue)
            {
                var dto = await IvaAlicuotaService!.ObtenerPorIdAsync(Id.Value);
                if (dto != null)
                    Alicuota = dto;
            }
        }

        protected async Task Guardar()
        {
            if (Id.HasValue)
                await IvaAlicuotaService!.ActualizarAsync(Alicuota.Id, Alicuota);
            else
                await IvaAlicuotaService!.CrearAsync(Alicuota);

            Nav!.NavigateTo("/ivaalicuotas");
        }

        protected async Task Eliminar()
        {
            if (Id.HasValue)
            {
                var confirmar = await JS!.InvokeAsync<bool>("confirm", $"¿Eliminar {Alicuota.Nombre}?");
                if (confirmar)
                {
                    await IvaAlicuotaService!.EliminarAsync(Id.Value);
                    Nav!.NavigateTo("/ivaalicuotas");
                }
            }
        }

        protected void Cancelar() => Nav!.NavigateTo("/ivaalicuotas");
    }
}
