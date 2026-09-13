using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace IurixBlazor.Pages
{
    public partial class EditorEscritos : ComponentBase
    {
        [Inject] private IJSRuntime? JS { get; set; }
        [Inject] private NavigationManager? Nav { get; set; }
        [Inject] private EscritoService? EscritoService { get; set; }



        [Parameter]
        [SupplyParameterFromQuery]
        public string? Origen { get; set; }  // Puede ser "main" o "navbar"

        [Parameter] public int? Id { get; set; } // ID opcional: si está es edición, si no, es nuevo

        private EscritoDto? escrito;

        //bool MostrarOpcionesGuardar = false;

        //TaskCompletionSource<string>? tcsOpcionGuardar;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (Id.HasValue) // Editar un escrito existente
                {
                    escrito = await EscritoService!.ObtenerEscritoPorIdAsync(Id.Value);
                    if (escrito != null && !string.IsNullOrEmpty(escrito.ContenidoHtml))
                    {
                        var base64 = ToBase64(escrito.ContenidoHtml);
                        await JS!.InvokeVoidAsync("initEditor", "editor1", base64); // ✅ Pasar contenido inicial directamente
                    }
                    else
                    {
                        await JS!.InvokeVoidAsync("initEditor", "editor1", (object?)null); // Editor vacío
                    }
                }
                else
                {
                    await JS!.InvokeVoidAsync("initEditor", "editor1", (object?)null); // Nuevo escrito vacío
                }
            }
        }


        //private async Task GuardarEscrito()
        //{
        //    // ✅ Obtener contenido HTML desde CKEditor
        //    var contenidoHtml = await JS.InvokeAsync<string>("getEditorContent");

        //    if (string.IsNullOrWhiteSpace(contenidoHtml))
        //    {
        //        await JS.InvokeVoidAsync("alert", "El contenido no puede estar vacío.");
        //        return;
        //    }

        //    bool exito = false;

        //    if (Id.HasValue && escrito != null)
        //    {
        //        // 🔄 Actualizar escrito existente
        //        string nuevoTitulo = await JS.InvokeAsync<string>("prompt", $"Modificar título:", escrito.Titulo);
        //        if (string.IsNullOrWhiteSpace(nuevoTitulo)) return;

        //        exito = await EscritoService.ActualizarEscritoAsync(escrito.Id, nuevoTitulo, contenidoHtml);
        //    }
        //    else
        //    {
        //        // 🆕 Escrito nuevo
        //        string tituloNuevo = await JS.InvokeAsync<string>("prompt", "Ingrese un título:", "Título sin nombre");
        //        if (string.IsNullOrWhiteSpace(tituloNuevo)) return;

        //        exito = await EscritoService.GuardarEscritoAsync(tituloNuevo, contenidoHtml);
        //    }

        //    if (exito)
        //        await JS.InvokeVoidAsync("alert", "✅ Escrito guardado correctamente.");
        //    else
        //        await JS.InvokeVoidAsync("alert", "❌ No se pudo guardar el escrito.");

        //    await JS.InvokeVoidAsync("destroyEditor"); // Limpieza del editor
        //    Nav.NavigateTo("/escritos");
        //}


        //protected async Task<string> MostrarOpcionGuardarEnPantallaAsync()
        //{
        //    MostrarOpcionesGuardar = true;
        //    StateHasChanged();
        //    tcsOpcionGuardar = new TaskCompletionSource<string>();
        //    return await tcsOpcionGuardar.Task;
        //}

        //protected void OpcionSeleccionada(string opcion)
        //{
        //    MostrarOpcionesGuardar = false;
        //    tcsOpcionGuardar?.TrySetResult(opcion);
        //}


        private async Task GuardarEscrito()
        {
            var contenidoHtml = await JS!.InvokeAsync<string>("getEditorContent", "editor1");
            if (string.IsNullOrWhiteSpace(contenidoHtml))
            {
                await MostrarToast("El contenido no puede estar vacío.", "error");
                return;
            }

            bool exito = false;

            // Si es un escrito existente, preguntar qué hacer
            if (Id.HasValue && escrito != null)
            {

                var opcion = await JS!.InvokeAsync<string>("mostrarOpcionGuardar");

                //var opcion = await MostrarOpcionGuardarEnPantallaAsync();

                if (opcion == "cancelar" || string.IsNullOrEmpty(opcion))
                    return;

                if (opcion == "actualizar")
                {
                    var nuevoTitulo = await JS!.InvokeAsync<string>("prompt", $"Modificar título:", escrito.Titulo);
                    if (string.IsNullOrWhiteSpace(nuevoTitulo)) return;

                    exito = await EscritoService!.ActualizarEscritoAsync(escrito.Id, nuevoTitulo, contenidoHtml);
                }
                else if (opcion == "nuevo")
                {
                    var tituloNuevo = await JS!.InvokeAsync<string>("prompt", "Ingrese un título:", "Título sin nombre");
                    if (string.IsNullOrWhiteSpace(tituloNuevo)) return;

                    exito = await EscritoService!.GuardarEscritoAsync(tituloNuevo, contenidoHtml);
                }
            }
            else
            {
                // Escrito nuevo directamente
                var tituloNuevo = await JS!.InvokeAsync<string>("prompt", "Ingrese un título:", "Título sin nombre");
                if (string.IsNullOrWhiteSpace(tituloNuevo)) return;

                exito = await EscritoService!.GuardarEscritoAsync(tituloNuevo, contenidoHtml);
            }

            // Mostrar resultado
            if (exito)
                await MostrarToast("✅ Escrito guardado correctamente.", "success");
            else
                await MostrarToast("❌ No se pudo guardar el escrito.", "error");

            await JS!.InvokeVoidAsync("destroyEditor");
            Nav!.NavigateTo("/escritos");
        }

        private async Task MostrarToast(string mensaje, string tipo)
        {
            await JS!.InvokeVoidAsync("mostrarToast", mensaje, tipo);
        }




        public void Dispose()
        {
            // Al cerrar la página, destruimos la instancia para evitar conflictos
            JS!.InvokeVoidAsync("destroyEditor");
        }

        //private async void Cancelar()
        //{
        //    await JS.InvokeVoidAsync("destroyEditor"); // 🔥 Limpia instancia previa
        //    Nav.NavigateTo("/escritos");
        //}



        private async void Cancelar()
        {
            await JS!.InvokeVoidAsync("destroyEditor"); // Limpia el CKEditor

            if (Origen == "main")
                Nav!.NavigateTo("/main");
            else if (Origen == "navbar")
                Nav!.NavigateTo("/"); // O a donde quieras que vaya el NavBar (home o dashboard)
            else
                Nav!.NavigateTo("/escritos"); // Fallback por defecto
        }

        private static string ToBase64(string plainText)
        {
            var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainBytes);
        }




    }
}
