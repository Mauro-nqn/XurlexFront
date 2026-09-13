using Microsoft.AspNetCore.Components;
using IurixBlazor.Services.Windowing;

namespace IurixBlazor.Components.Windowing;

public partial class WindowHost : ComponentBase, IDisposable
{
    [Inject] public WindowService Windows { get; set; } = default!;

    protected override void OnInitialized()
    {
        Windows.OnChanged += () => InvokeAsync(StateHasChanged);
    }
    public void Dispose()
    {
        Windows.OnChanged -= () => InvokeAsync(StateHasChanged); // si lo tenías como lambda inline, guardá la ref
    }
}
