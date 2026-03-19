using Microsoft.JSInterop;

namespace EventEaseApp.Services
{
    public class TooltipService
    {
        private readonly IJSRuntime jsRuntime;

        public TooltipService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        public ValueTask InitializeAsync()
        {
            return jsRuntime.InvokeVoidAsync("eventEase.initializeTooltips");
        }
    }
}