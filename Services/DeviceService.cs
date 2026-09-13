using Microsoft.JSInterop;

public sealed class DeviceService
{
    private readonly IJSRuntime _js;
    public DeviceService(IJSRuntime js) { _js = js; }

    public bool? PreferSinglePage { get; private set; }
    public string OS { get; private set; } = "";
    public int EffectiveWidth { get; private set; }

    public async Task InitAsync()
    {
        // Evitá múltiples llamadas
        if (PreferSinglePage.HasValue) return;

        // Si el script no está cargado aún, esto tiraría; por eso llamaremos desde OnAfterRender
        var info = await _js.InvokeAsync<DeviceInfo>("deviceInfo.get");
        PreferSinglePage = info.preferSinglePage;
        OS = info.os ?? "";
        EffectiveWidth = info.effectiveWidth;
    }

    private sealed class DeviceInfo
    {
        public bool preferSinglePage { get; set; }
        public string? os { get; set; }
        public int effectiveWidth { get; set; }
    }
}
