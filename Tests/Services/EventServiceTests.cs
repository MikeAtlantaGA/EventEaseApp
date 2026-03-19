using EventEaseApp.Models;
using EventEaseApp.Services;
using EventEaseApp.Tests.TestDoubles;
using Xunit;

namespace EventEaseApp.Tests.Services
{
    public class EventServiceTests
    {
        [Fact]
        public async Task InitializeAsync_SeedsDefaultEvents_WhenStorageIsEmpty()
        {
            var runtime = new FakeJSRuntime();
            var storage = new BrowserStorageService(runtime);
            var service = new EventService(storage);

            await service.InitializeAsync();
            var events = await service.GetAllAsync();

            Assert.Equal(3, events.Count);
            Assert.Contains(events, evt => evt.Name == "Annual Gala");
            Assert.Contains(events, evt => evt.Name == "Tech Conference");
            Assert.Contains(events, evt => evt.Name == "Music Festival");
            Assert.NotNull(runtime.GetRawItem("eventease.events"));
        }

        [Fact]
        public async Task AddUpdateDeleteAndResetAsync_PersistEventLifecycle()
        {
            var runtime = new FakeJSRuntime();
            var storage = new BrowserStorageService(runtime);
            var service = new EventService(storage);

            await service.InitializeAsync();
            await service.AddAsync(new Event
            {
                Name = "Community Meetup",
                Date = DateTime.Today.AddDays(90),
                Location = "Library"
            });

            var addedEvent = (await service.GetAllAsync()).Single(evt => evt.Name == "Community Meetup");
            addedEvent.Location = "Downtown Library";
            await service.UpdateAsync(addedEvent);

            var updatedEvent = (await service.GetAllAsync()).Single(evt => evt.Id == addedEvent.Id);
            Assert.Equal("Downtown Library", updatedEvent.Location);

            await service.DeleteAsync(addedEvent.Id);
            Assert.DoesNotContain(await service.GetAllAsync(), evt => evt.Id == addedEvent.Id);

            await service.ResetAsync();
            Assert.Equal(3, (await service.GetAllAsync()).Count);
        }
    }
}