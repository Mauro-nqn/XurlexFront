using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using IurixBlazor.Shared.Services;



namespace IurixBlazor.Pages
{
    public partial class CollaboraEditor
    {


        [Inject] protected CollaboraService CollaboraService { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;

        [Inject] private IJSRuntime JS { get; set; } = default!;

        [Parameter]
        public int Id { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; } = default!;

        private string CollaboraUrl = string.Empty;

        protected override void OnParametersSet()
        {
            var wopiSrc =
                $"http://host.docker.internal:8080/api/collabora/wopi/files/{Id}";

            var encodedWopi =
                Uri.EscapeDataString(wopiSrc);

            //Local
            //CollaboraUrl =
            //    $"http://192.168.1.254:9980/browser/dist/cool.html?WOPISrc={encodedWopi}";

            //Tunel Cloudflare
            CollaboraUrl =
                $"https://collabora.xurlex.com.ar/browser/dist/cool.html?WOPISrc={encodedWopi}";

        }

        private void Volver()
        {
            Navigation.NavigateTo("/collabora");
        }


        private async Task GuardarComo()
        {
            var titulo = await JS.InvokeAsync<string?>(
                "prompt",
                "Nombre del nuevo escrito:"
            );

            if (string.IsNullOrWhiteSpace(titulo))
                return;

            var nuevo = await CollaboraService.GuardarComoAsync(
                Id,
                titulo
            );

            if (nuevo == null)
            {
                await JS.InvokeVoidAsync(
                    "alert",
                    "No se pudo guardar el nuevo escrito."
                );

                return;
            }

            Navigation.NavigateTo(
                $"/collabora/editor/{nuevo.Id}"
            );
        }

    }

}
