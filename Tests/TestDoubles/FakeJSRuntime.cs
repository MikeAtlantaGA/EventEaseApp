using Microsoft.JSInterop;

namespace EventEaseApp.Tests.TestDoubles
{
    internal sealed class FakeJSRuntime : IJSRuntime
    {
        private readonly Dictionary<string, string?> storage = new(StringComparer.Ordinal);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            return InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            object? result = identifier switch
            {
                "localStorage.getItem" => GetItem(args),
                "localStorage.setItem" => SetItem(args),
                "localStorage.removeItem" => RemoveItem(args),
                _ => throw new NotSupportedException($"Unsupported JS interop call: {identifier}")
            };

            return new ValueTask<TValue>(result is null ? default! : (TValue)result);
        }

        public void SetRawItem(string key, string? value)
        {
            if (value is null)
            {
                storage.Remove(key);
                return;
            }

            storage[key] = value;
        }

        public string? GetRawItem(string key)
        {
            return storage.TryGetValue(key, out var value) ? value : null;
        }

        private object? GetItem(object?[]? args)
        {
            var key = (string?)args?[0] ?? string.Empty;
            return GetRawItem(key);
        }

        private object? SetItem(object?[]? args)
        {
            var key = (string?)args?[0] ?? string.Empty;
            var value = (string?)args?[1];
            SetRawItem(key, value);
            return null;
        }

        private object? RemoveItem(object?[]? args)
        {
            var key = (string?)args?[0] ?? string.Empty;
            storage.Remove(key);
            return null;
        }
    }
}