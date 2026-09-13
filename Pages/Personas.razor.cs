using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using IurixBlazor.Services.Windowing;

namespace IurixBlazor.Pages
{
    public class PersonasBase : ComponentBase
    {
        [Inject] protected PersonaService PersonaService { get; set; } = default!;
        [Inject] protected CondicionIvaService CondicionIvaService { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected IJSRuntime JS { get; set; } = default!;        

        [Inject] protected WindowService Win { get; set; } = default!;

        [Inject] protected PersonaEventService PersonaEventService { get; set; } = default!;

        [Inject] protected DeviceService DeviceSrv { get; set; } = default!;
        protected List<PersonaDto> Personas { get; set; } = new();



        
        protected string FiltroBusqueda { get; set; } = "";
        protected string CampoBusqueda { get; set; } = "General";
        protected string FiltroTipoPersona { get; set; } = "";


        //int _offset = 0;
        //string NextStyle() => $"top:{120 + (_offset = (_offset + 28) % 180)}px;left:{160 + _offset}px;position:fixed;transform:none;";

        protected int _cascade = 0;
        protected double NextLeft() => 160 + ((_cascade += 28) % 180);
        protected double NextTop() => 120 + (_cascade % 180);



        protected bool _deviceReady;

        protected bool esMovil = false;

        protected override async Task OnInitializedAsync()
        {
            // 1. Suscribe a los eventos
            PersonaEventService.OnPersonaSaved += OnPersonaSaved;

            await CargarPersonas();
        }

 

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // ⚠️ JSInterop después del primer render, así el script seguro ya está
                await DeviceSrv.InitAsync();
                _deviceReady = true;

                var width = await JS.InvokeAsync<int>("obtenerAnchoPantalla");
                esMovil = width < 768; // breakpoint Bootstrap sm/md

                StateHasChanged();
            }
        }

        protected async Task CargarPersonas()
        {
            Personas = await PersonaService.ObtenerPersonasAsync();
        }
        // Este método se ejecutará cuando se dispare el evento
        private async void OnPersonaSaved()
        {
            // Recarga los datos de la lista para mostrar los cambios
            await CargarPersonas();
            // Forzamos la actualización de la UI
            await InvokeAsync(StateHasChanged);
        }


        private static string SoloDigitos(string? s) =>
    new string((s ?? "").Where(char.IsDigit).ToArray());

        protected static string FormatearCUITVista(string? cuit)
        {
            var d = SoloDigitos(cuit);
            if (string.IsNullOrEmpty(d)) return "";      // o "", como prefieras
            if (d.Length <= 2) return d;
            if (d.Length <= 10) return $"{d[..2]}-{d[2..]}";
            // largo esperado (11): XX-XXXXXXXX-X
            return $"{d[..2]}-{d.Substring(2, 8)}-{d[10]}";
        }

        public void Dispose()
        {
            // Importante: Desuscribe para evitar fugas de memoria
            PersonaEventService.OnPersonaSaved -= OnPersonaSaved;
        }

        // Lista filtrada combinando ambos filtros
        protected IEnumerable<PersonaDto> PersonasFiltradas =>
            Personas
                .Where(p => FiltrarBusqueda(p))
                .Where(p => string.IsNullOrEmpty(FiltroTipoPersona) || p.Tipo.ToString() == FiltroTipoPersona);

        protected bool FiltrarBusqueda(PersonaDto p)
        {
            if (string.IsNullOrWhiteSpace(FiltroBusqueda))
                return true;

            return CampoBusqueda switch
            {
                "General" =>
                    (p.Apellido?.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Nombre?.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.RazonSocial?.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase) ?? false),

                "Email" =>
                    p.Email?.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase) ?? false,

                "Domicilio" =>
                    p.Domicilio?.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase) ?? false,

                _ => true
            };
        }

        //protected void NuevaPersona()
        //{
        //    Nav.NavigateTo("/personas/editar");
        //}






        //protected async Task NuevaPersona()
        //{
        //    var result = await DialogService.OpenAsync<IurixBlazor.Components.Personas.CrearEditarPersonaDialog>(
        //        "➕ Nueva Persona",
        //        new() { ["Id"] = (int?)null },
        //             new DialogOptions
        //             {
        //                 Draggable = true,
        //                 Resizable = true,
        //                 Width = "860px",
        //                 Height = "600px",
        //                 CloseDialogOnOverlayClick = false,
        //                 Style = NextStyle()
        //             });
        //}


        //protected async Task EditarPersona(int id)
        //{
        //    var result = await DialogService.OpenAsync<IurixBlazor.Components.Personas.CrearEditarPersonaDialog>(
        //        $"✏️ Editar Persona #{id}",
        //        new() { ["Id"] = id },
        //        new DialogOptions
        //        {
        //            Draggable = true,
        //            Resizable = true,
        //            Width = "860px",
        //            Height = "600px",
        //            CloseDialogOnOverlayClick = false,
        //            Style = NextStyle()
        //        });
        //}




        //protected void NuevaPersona()
        //{
        //    Win.Open(new WindowOptions(
        //        Title: "➕ Nueva Persona",
        //        ComponentType: typeof(IurixBlazor.Pages.CrearEditarPersonaDialog),
        //        Parameters: new() { ["Id"] = (int?)null },
        //        Left: NextLeft(), Top: NextTop(),
        //        Width: 900, Height: 640
        //    ));
        //}


        //protected void InteractLab()
        //{
        //    Nav.NavigateTo("/interact-lab");

        //}


        //protected void EditarPersona(int id)
        //{
        //    Win.Open(new WindowOptions(
        //        Title: $"✏️ Editar Persona #{id}",
        //        ComponentType: typeof(IurixBlazor.Pages.CrearEditarPersonaDialog),
        //        Parameters: new() { ["Id"] = id },
        //        Left: NextLeft(), Top: NextTop(),
        //        Width: 900, Height: 640
        //    ));
        //}


        

      





        protected void NuevaPersona()
        {
            // Defensivo: si aún no inicializó, asumimos página completa (mobile-friendly)
            var preferSinglePage = DeviceSrv.PreferSinglePage ?? true;

            if (preferSinglePage)
            {
                Nav.NavigateTo("/personas/editar");
                return;
            }

            Win.Open(new WindowOptions(
                Title: "➕ Nueva Persona",
                ComponentType: typeof(IurixBlazor.Pages.CrearEditarPersonaDialog),
                Parameters: new() { ["Id"] = (int?)null },
                Left: NextLeft(), Top: NextTop(),
                Width: 900, Height: 640
            ));
        }



        protected void EditarPersona(int id)
        {
            var preferSinglePage = DeviceSrv.PreferSinglePage ?? true;

            if (preferSinglePage)
            {
                Nav.NavigateTo($"/personas/editar/{id}");
                return;
            }

            Win.Open(new WindowOptions(
                Title: $"✏️ Editar Persona #{id}",
                ComponentType: typeof(IurixBlazor.Pages.CrearEditarPersonaDialog),
                Parameters: new() { ["Id"] = id },
                Left: NextLeft(), Top: NextTop(),
                Width: 900, Height: 640
            ));
        
        }














        protected void VerCtaCte(int personaId)
    => Nav.NavigateTo($"/clientes/{personaId}/cta-cte");

        //protected void EditarPersona(PersonaDto persona)
        //{
        //    Nav.NavigateTo($"/personas/editar/{persona.Id}");
        //}

        //protected async Task EliminarPersona(PersonaDto persona)
        //{

        //    bool confirmar = await JS.InvokeAsync<bool>("mostrarConfirmacion", $"¿Eliminar a {persona.Nombre}?");
        //    if (confirmar)
        //    {
        //        await PersonaService.EliminarPersonaAsync(persona.Id);
        //        await JS.InvokeVoidAsync("mostrarToast", "🗑 Persona eliminada.", "success");
        //        await CargarPersonas();
        //    }
        //}

        protected async Task EliminarPersona(PersonaDto persona)
        {
            

            bool confirmar = await JS.InvokeAsync<bool>("mostrarConfirmacion", $"¿Eliminar a {persona.Apellido} {persona.Nombre} {persona.RazonSocial}?");
            
                     
            if (!confirmar) return;

            try
            {
                await PersonaService.EliminarPersonaAsync(persona.Id);
                await JS.InvokeVoidAsync("mostrarToast", "🗑 Persona eliminada.", "success");
                await CargarPersonas();
            }
            catch (InvalidOperationException ex)
            {
                await JS.InvokeVoidAsync("mostrarToast", ex.Message, "warning");
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("mostrarToast", $"Error al eliminar: {ex.Message}", "danger");
            }
        }

    }
}
