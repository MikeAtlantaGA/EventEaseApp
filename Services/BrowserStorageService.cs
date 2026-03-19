using System.Text.Json;
using Microsoft.JSInterop;

namespace EventEaseApp.Services
{
    public class BrowserStorageService
    {
        private readonly IJSRuntime jsRuntime;
        private readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);

        public BrowserStorageService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        public async Task<T?> GetItemAsync<T>(string key)
        {
            var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json, jsonOptions);
            }
            catch (JsonException)
            {
                await RemoveItemAsync(key);
                return default;
            }
        }

        public async Task SetItemAsync<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value, jsonOptions);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }

        public async Task RemoveItemAsync(string key)
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
    }
}