using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace IurixBlazor.Services.Windowing;

public record WindowOptions(
    string Title,
    Type ComponentType,
    Dictionary<string, object?>? Parameters = null,
    double Left = 140, double Top = 120,
    double Width = 900, double Height = 640,
    bool Resizable = true, bool Draggable = true
);

public class WindowItem
{
    public Guid Id { get; init; } = Guid.NewGuid();     // 👈 ID estable de la ventana
    public string Title { get; set; } = "";
    public Type ComponentType { get; set; } = typeof(EmptyComponent);
    public Dictionary<string, object?> Parameters { get; set; } = new();
    public double Left { get; set; }
    public double Top { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public bool Resizable { get; set; } = true;
    public bool Draggable { get; set; } = true;
    public int ZIndex { get; set; }
    //public bool IsMinimized { get; set; }

    // Estado guardado ANTES de MAXIMIZAR
    public double? OriginalLeft { get; set; }
    public double? OriginalTop { get; set; }
    public double? OriginalWidth { get; set; }
    public double? OriginalHeight { get; set; }

    // Estado guardado ANTES de MINIMIZAR
    public double? BeforeMinimizeLeft { get; set; }
    public double? BeforeMinimizeTop { get; set; }
    public double? BeforeMinimizeWidth { get; set; }
    public double? BeforeMinimizeHeight { get; set; }

    public double? SavedLeft { get; set; }
    public double? SavedTop { get; set; }
    public double? SavedWidth { get; set; }
    public double? SavedHeight { get; set; }

    public bool IsMinimized { get; set; }
    public bool IsMaximized { get; set; }
}

public class WindowService
{
    public event Action? OnChanged;
    private readonly List<WindowItem> _windows = new();
    private int _z = 5000;

    public IReadOnlyList<WindowItem> Windows => _windows;

    public Guid Open(WindowOptions o)
    {
        var w = new WindowItem
        {
            Title = o.Title,
            ComponentType = o.ComponentType,
            Parameters = o.Parameters ?? new(),
            Left = o.Left,
            Top = o.Top,
            Width = o.Width,
            Height = o.Height,
            Resizable = o.Resizable,
            Draggable = o.Draggable,
            ZIndex = ++_z
        };
        _windows.Add(w);
        OnChanged?.Invoke();
        return w.Id;                                  // 👈 devolvemos Id
    }

    public void Close(Guid id)
    {
        var idx = _windows.FindIndex(x => x.Id == id);
        if (idx < 0) return;
        _windows.RemoveAt(idx);                       // 👈 elimina EXACTA
        OnChanged?.Invoke();
    }



    public void BringToFront(Guid id)
    {
        var w = _windows.FirstOrDefault(x => x.Id == id);
        if (w is null) return;
        w.ZIndex = ++_z;
        OnChanged?.Invoke();
    }
    public void Move(Guid id, double left, double top)
    {
        var w = _windows.FirstOrDefault(x => x.Id == id);
        if (w is null) return;
        w.Left = left; w.Top = top;
        OnChanged?.Invoke();
    }

    //public void MaximizeOrRestore(Guid id, double screenWidth, double screenHeight)
    //{
    //    var w = _windows.FirstOrDefault(x => x.Id == id);
    //    if (w is null) return;

    //    // Si la ventana no está maximizada, guardamos su estado y la maximizamos
    //    // Usamos SavedWidth como bandera para saber si está maximizada
    //    if (w.SavedWidth == null)
    //    {
    //        // Guardamos las dimensiones y la posición actuales
    //        w.SavedLeft = w.Left;
    //        w.SavedTop = w.Top;
    //        w.SavedWidth = w.Width;
    //        w.SavedHeight = w.Height;

    //        // Maximizamos la ventana
    //        w.Left = 8; // Pequeño margen
    //        w.Top = 8;
    //        w.Width = Math.Max(480, screenWidth - 16);
    //        w.Height = Math.Max(320, screenHeight - 16);
    //    }
    //    // Si ya está maximizada, la restauramos a su estado original
    //    else
    //    {
    //        // Restauramos a los valores guardados
    //        w.Left = w.SavedLeft.Value;
    //        w.Top = w.SavedTop.Value;
    //        w.Width = w.SavedWidth.Value;
    //        w.Height = w.SavedHeight.Value;

    //        // Limpiamos los valores guardados para indicar que no está maximizada
    //        w.SavedLeft = null;
    //        w.SavedTop = null;
    //        w.SavedWidth = null;
    //        w.SavedHeight = null;
    //    }

    //    BringToFront(id);
    //    OnChanged?.Invoke();
    //}


    // Modifica MaximizeOrRestore para usar el nuevo estado IsMaximized
    //public void MaximizeOrRestore(Guid id, double screenWidth, double screenHeight)
    //{
    //    var w = _windows.FirstOrDefault(x => x.Id == id);
    //    if (w is null) return;

    //    if (!w.IsMaximized)
    //    {
    //        // Guardar el estado actual (que es el "restaurado")
    //        w.SavedLeft = w.Left;
    //        w.SavedTop = w.Top;
    //        w.SavedWidth = w.Width;
    //        w.SavedHeight = w.Height;

    //        // Maximizar
    //        w.Left = 8;
    //        w.Top = 8;
    //        w.Width = Math.Max(480, screenWidth - 16);
    //        w.Height = Math.Max(320, screenHeight - 16);
    //        w.IsMaximized = true;
    //    }
    //    else
    //    {
    //        // Restaurar desde el estado guardado
    //        if (w.SavedWidth.HasValue)
    //        {
    //            w.Left = w.SavedLeft.Value;
    //            w.Top = w.SavedTop.Value;
    //            w.Width = w.SavedWidth.Value;
    //            w.Height = w.SavedHeight.Value;
    //        }

    //        w.IsMaximized = false;
    //        // No limpiar los estados guardados aún, podrías necesitarlo
    //        // si quieres restaurar desde un estado minimizado
    //    }

    //    BringToFront(id);
    //    OnChanged?.Invoke();
    //}

    // En la clase WindowService

    public void MaximizeOrRestore(Guid id, double screenWidth, double screenHeight)
    {
        var w = _windows.FirstOrDefault(x => x.Id == id);
        if (w is null) return;

        if (!w.IsMaximized)
        {
            // 1. Guardar el estado actual (la ventana intermedia original)
            w.OriginalLeft = w.Left;
            w.OriginalTop = w.Top;
            w.OriginalWidth = w.Width;
            w.OriginalHeight = w.Height;

            // 2. Maximizar la ventana
            w.Left = 8;
            w.Top = 8;
            w.Width = Math.Max(480, screenWidth - 16);
            w.Height = Math.Max(320, screenHeight - 16);
            w.IsMaximized = true;
        }
        else // Ya está maximizada, restaurar a la original
        {
            // 3. Restaurar a los valores originales guardados
            if (w.OriginalWidth.HasValue)
            {
                w.Left = w.OriginalLeft.Value;
                w.Top = w.OriginalTop.Value;
                w.Width = w.OriginalWidth.Value;
                w.Height = w.OriginalHeight.Value;
            }

            // 4. Limpiar el estado de maximizada
            w.IsMaximized = false;
            // No borres Original... por si se vuelve a maximizar
        }

        // Si la ventana estaba minimizada, al maximizarla la restauramos
        if (w.IsMinimized)
        {
            w.IsMinimized = false;
        }

        BringToFront(id);
        OnChanged?.Invoke();
    }

    public void Minimize(Guid id)
    {
        var w = _windows.FirstOrDefault(x => x.Id == id);
        if (w is null || w.IsMinimized) return;

        // 1. Guardar el estado actual (puede ser original o maximizado)
        w.BeforeMinimizeLeft = w.Left;
        w.BeforeMinimizeTop = w.Top;
        w.BeforeMinimizeWidth = w.Width;
        w.BeforeMinimizeHeight = w.Height;
        w.IsMinimized = true;

        OnChanged?.Invoke();
    }

    public void Restore(Guid id)
    {
        var w = _windows.FirstOrDefault(x => x.Id == id);
        if (w is null || !w.IsMinimized) return;

        // 1. Restaurar al estado que tenía justo antes de minimizar
        if (w.BeforeMinimizeWidth.HasValue)
        {
            w.Left = w.BeforeMinimizeLeft.Value;
            w.Top = w.BeforeMinimizeTop.Value;
            w.Width = w.BeforeMinimizeWidth.Value;
            w.Height = w.BeforeMinimizeHeight.Value;
        }

        // 2. Limpiar el estado de minimizada
        w.IsMinimized = false;
        OnChanged?.Invoke();
    }

    //public void Resize(Guid id, double width, double height)
    //{
    //    var w = _windows.FirstOrDefault(x => x.Id == id);
    //    if (w is null) return;
    //    w.Width = Math.Max(360, width);
    //    w.Height = Math.Max(240, height);
    //    OnChanged?.Invoke();
    //}

    //public void Resize(Guid id, double width, double height)
    //{
    //    var w = _windows.FirstOrDefault(x => x.Id == id);
    //    if (w is null) return;

    //    // Solo permitir redimensionar si la ventana no está maximizada
    //    if (w.SavedWidth == null)
    //    {
    //        w.Width = Math.Max(360, width);
    //        w.Height = Math.Max(240, height);
    //        OnChanged?.Invoke();
    //    }
    //}

    public void Resize(Guid id, double width, double height)
    {
        var w = _windows.FirstOrDefault(x => x.Id == id);
        if (w is null) return;

        // Solo permitir redimensionar si NO está maximizada y la ventana es resizable
        if (!w.IsMaximized && w.Resizable)
        {
            w.Width = Math.Max(360, width);
            w.Height = Math.Max(240, height);
            OnChanged?.Invoke();
        }
    }



    //public void Minimize(Guid id)
    //{
    //    var w = _windows.FirstOrDefault(x => x.Id == id);
    //    if (w is null) return;

    //    // Guardar la posición y el tamaño actuales
    //    w.SavedLeft = w.Left;
    //    w.SavedTop = w.Top;
    //    w.SavedWidth = w.Width;
    //    w.SavedHeight = w.Height;
    //    w.IsMinimized = true;

    //    OnChanged?.Invoke();
    //}

    //public void Minimize(Guid id)
    //{
    //    var w = _windows.FirstOrDefault(x => x.Id == id);
    //    if (w is null || w.IsMinimized) return;

    //    // Guardar el estado antes de minimizar
    //    w.SavedLeft = w.Left;
    //    w.SavedTop = w.Top;
    //    w.SavedWidth = w.Width;
    //    w.SavedHeight = w.Height;
    //    w.IsMinimized = true;

    //    OnChanged?.Invoke();
    //}

    //public void Restore(Guid id)
    //{
    //    var w = _windows.FirstOrDefault(x => x.Id == id);
    //    if (w is null) return;

    //    // Restaurar la posición y el tamaño guardados
    //    w.Left = w.SavedLeft ?? w.Left;
    //    w.Top = w.SavedTop ?? w.Top;
    //    w.Width = w.SavedWidth ?? w.Width;
    //    w.Height = w.SavedHeight ?? w.Height;
    //    w.IsMinimized = false;

    //    BringToFront(id); // Trae la ventana restaurada al frente
    //    OnChanged?.Invoke();
    //}

    // Nuevo método para restaurar la ventana minimizada
    //public void Restore(Guid id)
    //{
    //    var w = _windows.FirstOrDefault(x => x.Id == id);
    //    if (w is null || !w.IsMinimized) return;

    //    // Restaurar la ventana a su estado previo a la minimización
    //    if (w.SavedWidth.HasValue)
    //    {
    //        w.Left = w.SavedLeft.Value;
    //        w.Top = w.SavedTop.Value;
    //        w.Width = w.SavedWidth.Value;
    //        w.Height = w.SavedHeight.Value;
    //    }

    //    // Limpiar los estados de "guardado"
    //    w.SavedLeft = null;
    //    w.SavedTop = null;
    //    w.SavedWidth = null;
    //    w.SavedHeight = null;
    //    w.IsMinimized = false;

    //    OnChanged?.Invoke();
    //}



}

// Fallback simple para probar apertura
public class EmptyComponent : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder b)
    {
        b.OpenElement(0, "div");
        b.AddAttribute(1, "style", "padding:12px");
        b.AddContent(2, "Hola desde EmptyComponent 👋");
        b.CloseElement();
    }
}
