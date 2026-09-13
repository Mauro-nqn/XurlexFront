using IurixBlazor.Pages;
using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Helpers;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;



namespace IurixBlazor.Pages
{
    public class AdminRolesBase : ComponentBase
    {
        [Inject] protected RolService RolService { get; set; } = default!;

        [Inject] protected IJSRuntime JS { get; set; } = default!;



        protected List<RolDto> Roles = new();
        protected RolDto? RolSeleccionado;

        // Permisos agrupados por módulo
        protected Dictionary<string, List<PermisoDef>> PermisosPorModulo =
            PermisosCatalogo.Todos
                .GroupBy(p => p.Modulo)
                .ToDictionary(g => g.Key, g => g.ToList());

        protected override async Task OnInitializedAsync()
        {
            Roles = await RolService.ObtenerRolesAsync();
        }

        protected void NuevoRol()
        {
            RolSeleccionado = new RolDto
            {
                Nombre = string.Empty,
                Permisos = new List<string>()
            };
        }

        protected void SeleccionarRol(RolDto rol)
        {
            // Clonar para no pisar hasta guardar
            RolSeleccionado = new RolDto
            {
                Id = rol.Id,
                Nombre = rol.Nombre,
                Permisos = rol.Permisos.ToList()
            };
        }

        protected void TogglePermiso(string codigo, bool isChecked)
        {
            if (RolSeleccionado is null) return;

            if (isChecked)
            {
                if (!RolSeleccionado.Permisos.Contains(codigo))
                    RolSeleccionado.Permisos.Add(codigo);
            }
            else
            {
                RolSeleccionado.Permisos.Remove(codigo);
            }
        }

        protected async Task GuardarRol()
        {
            if (RolSeleccionado is null) return;

            RolDto? actualizado;
            if (RolSeleccionado.Id == 0)
            {
                actualizado = await RolService.CrearRolAsync(RolSeleccionado);
                if (actualizado != null)
                    Roles.Add(actualizado);
            }
            else
            {
                actualizado = await RolService.ActualizarRolAsync(RolSeleccionado);
                if (actualizado != null)
                {
                    var idx = Roles.FindIndex(r => r.Id == actualizado.Id);
                    if (idx >= 0) Roles[idx] = actualizado;
                }
            }

            //if (actualizado != null)
            //    RolSeleccionado = actualizado;

            if (actualizado != null)
            {
                RolSeleccionado = actualizado;
                await RolService.RefrescarPermisosSiAfectaUsuarioActual(actualizado.Id);

                //  NUEVO: refrescar token con claims actualizados
                await RolService.RefrescarTokenAsync();

                //  Mensaje de éxito
                await Toast("Permisos actualizados correctamente.", "success");

                //otra forma de mostrar Toast
                //await JS.InvokeVoidAsync("mostrarToast", "🗑 Evento eliminado", "success");
            }
            else
            {
                //  Mensaje si falló       

                await Toast("No se pudo guardar el rol.", "error");

                //otra forma de mostrar Toast
                //await JS.InvokeVoidAsync("mostrarToast", "🗑 Evento eliminado", "success");
            }

        }


        private async Task Toast(string msg, string tipo) =>
              await JS.InvokeVoidAsync("mostrarToast", msg, tipo);





        protected async Task EliminarRol()
        {
            if (RolSeleccionado is null || RolSeleccionado.Id == 0) return;

            var ok = await RolService.EliminarRolAsync(RolSeleccionado.Id);
            if (!ok) return;

            Roles.RemoveAll(r => r.Id == RolSeleccionado.Id);
            RolSeleccionado = null;
        }
    }
}
