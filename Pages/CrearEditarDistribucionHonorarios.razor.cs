using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using IurixBlazor.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace IurixBlazor.Pages
{
    public class CrearEditarDistribucionHonorariosBase : ComponentBase
    {
        [Inject] protected DistribucionHonorarioService HonorariosService { get; set; } = default!;
        [Inject] protected UsuarioService UsuarioService { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected DistribucionHonorarioDto Distribucion { get; set; } = new()
        {
            Detalles = new List<DistribucionDetalleDto>()
        };

        protected List<DistribucionDetalleDto> Detalles { get; set; } = new();
        protected List<UsuarioDto> Usuarios { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            Usuarios = await UsuarioService.ObtenerUsuariosAsync();

            if (Id.HasValue)
            {
                // Traer la distribución
                Distribucion = await HonorariosService.ObtenerPorIdAsync(Id.Value)
                              ?? new DistribucionHonorarioDto { Detalles = new List<DistribucionDetalleDto>() };

                // Traer los detalles asociados usando el endpoint separado
                Detalles = await HonorariosService.ObtenerDetallesPorDistribucionAsync(Id.Value)
                           ?? new List<DistribucionDetalleDto>();
            }
            else
            {
                // Si es nuevo, inicializar vacíos
                Distribucion = new DistribucionHonorarioDto
                {
                    Detalles = new List<DistribucionDetalleDto>()
                };
                Detalles = new List<DistribucionDetalleDto>();
            }
        }


        protected void AgregarDetalle()
        {
            Detalles.Add(new DistribucionDetalleDto { DistribucionHonorarioId = Distribucion.Id });
        }

        protected void EliminarDetalle(DistribucionDetalleDto detalle)
        {
            Detalles.Remove(detalle);
        }

        protected async Task Guardar()
        {
            if (!Id.HasValue)
            {
                // 🆕 CREAR distribución
                var nuevaDistribucion = new DistribucionHonorarioDto
                {
                    Nombre = Distribucion.Nombre,
                    Descripcion = Distribucion.Descripcion,
                    FechaCreacion = DateTime.Now,
                    Detalles = new List<DistribucionDetalleDto>() // Inicializamos vacío
                };

                var creada = await HonorariosService.CrearAsync(nuevaDistribucion);

                // Crear detalles asociados
                foreach (var detalle in Detalles)
                {
                    detalle.DistribucionHonorarioId = creada.Id;
                    await HonorariosService.CrearDetalleAsync(detalle);
                }
            }
            else
            {
                // ✏️ EDITAR distribución existente
                await HonorariosService.ActualizarAsync(Distribucion.Id, Distribucion);

                // 🔄 Sincronizar detalles:
                var detallesExistentes = await HonorariosService.ObtenerDetallesPorDistribucionAsync(Distribucion.Id);

                // 1️⃣ Eliminar detalles que ya no están
                foreach (var detalleExistente in detallesExistentes)
                {
                    if (!Detalles.Any(d => d.Id == detalleExistente.Id))
                    {
                        await HonorariosService.EliminarDetalleAsync(detalleExistente.Id);
                    }
                }

                // 2️⃣ Crear o actualizar los actuales
                foreach (var detalle in Detalles)
                {
                    if (detalle.Id == 0) // Nuevo detalle
                        await HonorariosService.CrearDetalleAsync(detalle);
                    else
                        await HonorariosService.ActualizarDetalleAsync(detalle.Id, detalle);
                }
            }

            Nav.NavigateTo("/distribucion-honorarios");
        }


        protected void Cancelar() => Nav.NavigateTo("/distribucion-honorarios");
    }
}


