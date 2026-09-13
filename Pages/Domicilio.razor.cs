using IurixBlazor.Services;
using IurixBlazor.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public class DomicilioBase : ComponentBase
{
    [Inject] protected DomicilioService? DomicilioService { get; set; }
    [Inject] protected NavigationManager? Nav { get; set; }
    [Inject] private IJSRuntime? JS { get; set; }

    public List<DomicilioDto>? Domicilios { get; set; } = new();

 
    protected List<DomicilioDto>? DomiciliosFiltrados;
    protected string _filtroTipo = "";
    protected string FiltroTipo
    {
        get => _filtroTipo;
        set
        {
            _filtroTipo = value;
            FiltrarPorTipo();
        }
    }



    protected override async Task OnInitializedAsync()
    {
        //Domicilios = await DomicilioService!.ObtenerTodosAsync();

        await CargarDomiciliosAsync();
        //DomiciliosFiltrados = Domicilios;
    }

    private async Task CargarDomiciliosAsync()
    {
        Domicilios = (await DomicilioService!.ObtenerTodosAsync()).ToList();
        FiltrarPorTipo(); // 👈 siempre sincroniza la lista filtrada
    }

    protected void FiltrarPorTipo()
    {
        if (string.IsNullOrEmpty(FiltroTipo))
            DomiciliosFiltrados = Domicilios;
        else
            DomiciliosFiltrados = Domicilios!
                .Where(d => d.Tipo.ToString() == FiltroTipo)
                .ToList();
    }

    protected void NuevoDomicilio() => Nav!.NavigateTo("/domicilios/editar");

    protected void EditarDomicilio(int id) => Nav!.NavigateTo($"/domicilios/editar/{id}");

    //protected async Task EliminarDomicilio(int id)
    //{
    //    var confirmar = await JS!.InvokeAsync<bool>("confirm", "¿Seguro que deseas eliminar este domicilio?");
    //    if (confirmar)
    //    {
    //        await DomicilioService!.EliminarAsync(id);
    //        Domicilios = await DomicilioService.ObtenerTodosAsync();
    //        StateHasChanged();
    //    }
    //}

    protected async Task EliminarDomicilio(int id)
    {
        var confirmar = await JS!.InvokeAsync<bool>("confirm", "¿Seguro que deseas eliminar este domicilio?");
        if (!confirmar) return;

        try
        {
            await DomicilioService!.EliminarAsync(id);

            // opción 1: recargar todo
            await CargarDomiciliosAsync();

            // opción 2 (más liviana): sacar el item localmente
            // var dom = Domicilios.FirstOrDefault(x => x.Id == id);
            // if (dom != null) Domicilios.Remove(dom);
        }
        catch (InvalidOperationException ex)
        {
            // Después si querés lo cambiamos por Toastr o un modal
            await JS!.InvokeVoidAsync("alert", ex.Message);
        }
    }

}
