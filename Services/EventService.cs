using EventEaseApp.Models;

namespace EventEaseApp.Services
{
    public class EventService
    {
        private const string StorageKey = "eventease.events";
        private readonly BrowserStorageService browserStorageService;
        private List<Event> events = new();
        private bool isInitialized;

        public EventService(BrowserStorageService browserStorageService)
        {
            this.browserStorageService = browserStorageService;
        }

        public async Task InitializeAsync()
        {
            if (isInitialized)
            {
                return;
            }

            var storedEvents = await browserStorageService.GetItemAsync<List<Event>>(StorageKey);
            events = storedEvents ?? CreateDefaultEvents();
            isInitialized = true;

            if (storedEvents == null)
            {
                await PersistAsync();
            }
        }

        public async Task<IReadOnlyList<Event>> GetAllAsync()
        {
            await InitializeAsync();
            return events
                .OrderBy(e => e.Date)
                .ThenBy(e => e.Name)
                .ThenBy(e => e.Id)
                .ToList();
        }

        public async Task AddAsync(Event evt)
        {
            await InitializeAsync();
            evt.Id = events.Any() ? events.Max(existingEvent => existingEvent.Id) + 1 : 1;
            events.Add(Clone(evt));
            await PersistAsync();
        }

        public async Task UpdateAsync(Event evt)
        {
            await InitializeAsync();
            var existingIndex = events.FindIndex(existingEvent => existingEvent.Id == evt.Id);
            if (existingIndex < 0)
            {
                return;
            }

            events[existingIndex] = Clone(evt);
            await PersistAsync();
        }

        public async Task DeleteAsync(int eventId)
        {
            await InitializeAsync();
            events.RemoveAll(evt => evt.Id == eventId);
            await PersistAsync();
        }

        public async Task ResetAsync()
        {
            await InitializeAsync();
            events = CreateDefaultEvents();
            await PersistAsync();
        }

        private Task PersistAsync()
        {
            return browserStorageService.SetItemAsync(StorageKey, events);
        }

        private static List<Event> CreateDefaultEvents()
        {
            var today = DateTime.Today;
            return new List<Event>
            {
                new() { Id = 1, Name = "Annual Gala", Date = today.AddDays(10), Location = "Grand Hall" },
                new() { Id = 2, Name = "Tech Conference", Date = today.AddDays(30), Location = "Convention Center" },
                new() { Id = 3, Name = "Music Festival", Date = today.AddDays(60), Location = "City Park" }
            };
        }

        private static Event Clone(Event source)
        {
            return new Event
            {
                Id = source.Id,
                Name = source.Name,
                Date = source.Date,
                Location = source.Location
            };
        }
    }
}