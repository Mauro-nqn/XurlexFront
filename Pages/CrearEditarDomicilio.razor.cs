using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class CrearEditarDomicilioBase : ComponentBase
{
    [Inject] protected DomicilioService? DomicilioService { get; set; }
    [Inject] protected NavigationManager? Nav { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }

    [Parameter] public int? Id { get; set; }
    protected DomicilioDto Domicilio { get; set; } = new();

    protected string? ErrorMensaje;

  
    protected bool IsSaving;
    protected bool IsDeleting;

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            var dto = await DomicilioService!.ObtenerPorIdAsync(Id.Value);
            if (dto != null)
                Domicilio = dto;
        }
        else
        {
            // Para nuevos, dejar en blanco pero con valores válidos
            Domicilio = new DomicilioDto();
        }
    }

    //protected async Task Guardar()
    //{
    //    if (Id.HasValue)
    //    {
    //        await DomicilioService!.ActualizarAsync(Domicilio.Id, Domicilio);
    //    }
    //    else
    //    {
    //        var crearDto = new CrearDomicilioDto
    //        {
    //            Descripcion = Domicilio.Descripcion,
    //            Tipo = Domicilio.Tipo
    //        };

    //        await DomicilioService!.CrearAsync(crearDto);
    //    }

    //    Nav!.NavigateTo("/domicilios");
    //}

    //protected async Task Guardar()
    //{
    //    // Si es electrónico, podés limpiar ciudad/provincia para no guardar basura
    //    if (Domicilio.Tipo == TipoDomicilio.Electronico)
    //    {
    //        Domicilio.Ciudad = null;
    //        Domicilio.Provincia = null;
    //    }

    //    if (Id.HasValue)
    //    {
    //        await DomicilioService!.ActualizarAsync(Domicilio.Id, Domicilio);
    //    }
    //    else
    //    {
    //        var crearDto = new CrearDomicilioDto
    //        {
    //            Descripcion = Domicilio.Descripcion,
    //            Tipo = Domicilio.Tipo,
    //            Ciudad = Domicilio.Tipo == TipoDomicilio.Constituido ? Domicilio.Ciudad : null,
    //            Provincia = Domicilio.Tipo == TipoDomicilio.Constituido ? Domicilio.Provincia : null
    //        };

    //        await DomicilioService!.CrearAsync(crearDto);
    //    }

    //    Nav!.NavigateTo("/domicilios");
    //}


    protected async Task Guardar()
    {
        ErrorMensaje = null;
        if (IsSaving) return;
        IsSaving = true;

        try
        {
            // Si es electrónico, limpiamos ciudad/provincia
            if (Domicilio.Tipo == TipoDomicilio.Electronico)
            {
                Domicilio.Ciudad = null;
                Domicilio.Provincia = null;
            }

            if (Id.HasValue)
            {
                // Edición
                await DomicilioService!.ActualizarAsync(Domicilio.Id, Domicilio);
            }
            else
            {
                // Creación
                var crearDto = new CrearDomicilioDto
                {
                    Descripcion = Domicilio.Descripcion,
                    Tipo = Domicilio.Tipo,
                    Ciudad = Domicilio.Tipo == TipoDomicilio.Constituido ? Domicilio.Ciudad : null,
                    Provincia = Domicilio.Tipo == TipoDomicilio.Constituido ? Domicilio.Provincia : null
                };

                await DomicilioService!.CrearAsync(crearDto);
            }

            // Todo OK → volvemos al listado
            Nav!.NavigateTo("/domicilios");
        }
        catch (InvalidOperationException ex)
        {
            // Conflictos lógicos: duplicado, etc.
            ErrorMensaje = ex.Message;
        }
        catch (Exception ex)
        {
            // Por si pasa algo inesperado
            ErrorMensaje = "Ocurrió un error al guardar el domicilio: " + ex.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }


    //protected async Task Eliminar()
    //{
    //    if (Id.HasValue)
    //    {
    //        var confirmar = await JS!.InvokeAsync<bool>("confirm", $"¿Eliminar {Domicilio.Descripcion}?");
    //        if (confirmar)
    //        {
    //            await DomicilioService!.EliminarAsync(Id.Value);
    //            Nav!.NavigateTo("/domicilios");
    //        }
    //    }
    //}

    //protected async Task Eliminar()
    //{
    //    ErrorMensaje = null;
    //    if (!Id.HasValue || IsDeleting) return;

    //    var confirmar = await JS!.InvokeAsync<bool>("confirm", $"¿Eliminar {Domicilio.Descripcion}?");
    //    if (!confirmar) return;

    //    IsDeleting = true;

    //    try
    //    {
    //        await DomicilioService!.EliminarAsync(Id.Value);
    //        Nav!.NavigateTo("/domicilios");
    //    }
    //    catch (InvalidOperationException ex)
    //    {
    //        // Ej: “No se puede eliminar porque está asociado a procesos judiciales”
    //        ErrorMensaje = ex.Message;
    //    }
    //    catch (Exception ex)
    //    {
    //        ErrorMensaje = "Ocurrió un error al eliminar el domicilio: " + ex.Message;
    //    }
    //    finally
    //    {
    //        IsDeleting = false;
    //    }
    //}


    protected void Cancelar() => Nav!.NavigateTo("/domicilios");
}
