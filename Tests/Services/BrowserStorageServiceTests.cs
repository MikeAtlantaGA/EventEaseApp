using EventEaseApp.Services;
using EventEaseApp.Tests.TestDoubles;
using Xunit;

namespace EventEaseApp.Tests.Services
{
    public class BrowserStorageServiceTests
    {
        [Fact]
        public async Task GetItemAsync_RemovesCorruptJson_AndReturnsDefault()
        {
            var runtime = new FakeJSRuntime();
            runtime.SetRawItem("broken", "{ not-valid-json }");
            var storage = new BrowserStorageService(runtime);

            var result = await storage.GetItemAsync<Dictionary<string, string>>("broken");

            Assert.Null(result);
            Assert.Null(runtime.GetRawItem("broken"));
        }
    }
}