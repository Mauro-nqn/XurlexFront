using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace IurixBlazor.Pages
{
    public partial class Collabora
    {

        private List<CollaboraEscritoDto> escritos = new();

        private bool cargando = true;

        protected override async Task OnInitializedAsync()
        {
            escritos = await CollaboraService.ListarAsync();
            cargando = false;
        }

        private void Abrir(int id)
        {
            Navigation.NavigateTo($"/collabora/editor/{id}");
        }

        private async Task Nuevo()
        {
            var escrito =
                await CollaboraService.NuevoAsync("Nuevo escrito");

            if (escrito != null)
            {
                Navigation.NavigateTo(
                    $"/collabora/editor/{escrito.Id}"
                );
            }
        }

        // private async Task ImportarArchivo(InputFileChangeEventArgs e)
        // {
        //     var archivo = e.File;

        //     const long maximo = 20 * 1024 * 1024;

        //     if (!archivo.Name.EndsWith(
        //             ".docx",
        //             StringComparison.OrdinalIgnoreCase))
        //     {
        //         return;
        //     }

        //     await using var stream =
        //         archivo.OpenReadStream(maximo);

        //     var escrito =
        //         await CollaboraService.ImportarAsync(
        //             stream,
        //             archivo.Name
        //         );

        //     if (escrito != null)
        //     {
        //         Navigation.NavigateTo(
        //             $"/collabora/editor/{escrito.Id}"
        //         );
        //     }
        // }


        private async Task ImportarArchivo(InputFileChangeEventArgs e)
        {
            var archivo = e.File;

            var extension = Path.GetExtension(archivo.Name)
                .ToLowerInvariant();

            var permitidos = new[]
            {
        ".docx",
        ".rtf",
        ".txt",
        ".pdf"
    };

            if (!permitidos.Contains(extension))
                return;

            const long maximo = 50 * 1024 * 1024;

            await using var stream =
                archivo.OpenReadStream(maximo);

            var escrito =
                await CollaboraService.ImportarAsync(
                    stream,
                    archivo.Name,
                    archivo.ContentType
                );

            if (escrito != null)
            {
                Navigation.NavigateTo(
                    $"/collabora/editor/{escrito.Id}"
                );
            }
        }




        private async Task Eliminar(CollaboraEscritoDto escrito)
        {
            var confirmado = await JS.InvokeAsync<bool>(
                "confirm",
                $"¿Eliminar el escrito \"{escrito.Titulo}\"?"
            );

            if (!confirmado)
                return;

            var eliminado = await CollaboraService.EliminarAsync(
                escrito.Id
            );

            if (!eliminado)
            {
                await JS.InvokeVoidAsync(
                    "alert",
                    "No se pudo eliminar el escrito."
                );

                return;
            }

            escritos.Remove(escrito);
        }



    }
}
