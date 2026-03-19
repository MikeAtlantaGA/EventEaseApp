using EventEaseApp.Models;
using EventEaseApp.Services;
using EventEaseApp.Tests.TestDoubles;
using Xunit;

namespace EventEaseApp.Tests.Services
{
    public class AttendeeRegistrationServiceTests
    {
        [Fact]
        public async Task InitializeAsync_MigratesLegacyRegistrations_ToEventIds()
        {
            var runtime = new FakeJSRuntime();
            var storage = new BrowserStorageService(runtime);
            var eventService = new EventService(storage);

            await storage.SetItemAsync("eventease.attendee-registrations", new List<AttendeeRegistration>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    EventName = "Annual Gala",
                    FirstName = "Ada",
                    LastName = "Lovelace",
                    Email = "ada@example.com",
                    TicketType = "VIP",
                    AcceptTerms = true,
                    RegisteredAtUtc = DateTimeOffset.UtcNow
                }
            });

            var registrationService = new AttendeeRegistrationService(storage, eventService);

            await registrationService.InitializeAsync();

            var galaId = (await eventService.GetAllAsync()).Single(evt => evt.Name == "Annual Gala").Id;
            var registration = Assert.Single(await registrationService.GetAllAsync());

            Assert.Equal(galaId, registration.EventId);
            Assert.Equal("Annual Gala", registration.EventName);
        }

        [Fact]
        public async Task DeleteByEventAsync_RemovesOnlyMatchingEventRegistrations()
        {
            var runtime = new FakeJSRuntime();
            var storage = new BrowserStorageService(runtime);
            var eventService = new EventService(storage);
            await eventService.InitializeAsync();

            var events = await eventService.GetAllAsync();
            var gala = events.Single(evt => evt.Name == "Annual Gala");
            var conference = events.Single(evt => evt.Name == "Tech Conference");

            await storage.SetItemAsync("eventease.attendee-registrations", new List<AttendeeRegistration>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    EventId = gala.Id,
                    EventName = gala.Name,
                    FirstName = "Ada",
                    LastName = "Lovelace",
                    Email = "ada@example.com",
                    TicketType = "VIP",
                    AcceptTerms = true,
                    RegisteredAtUtc = DateTimeOffset.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    EventId = conference.Id,
                    EventName = conference.Name,
                    FirstName = "Grace",
                    LastName = "Hopper",
                    Email = "grace@example.com",
                    TicketType = "General Admission",
                    AcceptTerms = true,
                    RegisteredAtUtc = DateTimeOffset.UtcNow
                }
            });

            var registrationService = new AttendeeRegistrationService(storage, eventService);
            await registrationService.InitializeAsync();

            await registrationService.DeleteByEventAsync(gala.Id);

            var remaining = await registrationService.GetAllAsync();
            Assert.Single(remaining);
            Assert.Equal(conference.Id, remaining[0].EventId);
        }
    }
}