using EventEaseApp.Models;

namespace EventEaseApp.Services
{
    public class AttendeeRegistrationService
    {
        private const string StorageKey = "eventease.attendee-registrations";
        private readonly BrowserStorageService browserStorageService;
        private readonly EventService eventService;
        private List<AttendeeRegistration> registrations = new();
        private bool isInitialized;

        public AttendeeRegistrationService(BrowserStorageService browserStorageService, EventService eventService)
        {
            this.browserStorageService = browserStorageService;
            this.eventService = eventService;
        }

        public async Task InitializeAsync()
        {
            if (isInitialized)
            {
                return;
            }

            registrations = await browserStorageService.GetItemAsync<List<AttendeeRegistration>>(StorageKey) ?? new List<AttendeeRegistration>();
            isInitialized = true;

            await MigrateLegacyRegistrationsAsync();
        }

        public async Task<IReadOnlyList<AttendeeRegistration>> GetAllAsync()
        {
            await InitializeAsync();
            return registrations.OrderByDescending(r => r.RegisteredAtUtc).ToList();
        }

        public async Task<IReadOnlyList<AttendeeRegistration>> GetByEventAsync(int eventId)
        {
            await InitializeAsync();
            return registrations
            .Where(r => r.EventId == eventId)
                .OrderBy(r => r.LastName)
                .ThenBy(r => r.FirstName)
                .ToList();
        }

        public async Task AddAsync(AttendeeRegistration registration)
        {
            await InitializeAsync();
            registration.Id = Guid.NewGuid();
            registration.RegisteredAtUtc = DateTimeOffset.UtcNow;
            registrations.Add(registration);
            await PersistAsync();
        }

        public async Task UpdateAsync(AttendeeRegistration registration)
        {
            await InitializeAsync();
            var existingIndex = registrations.FindIndex(r => r.Id == registration.Id);
            if (existingIndex < 0)
            {
                return;
            }

            registrations[existingIndex] = registration;
            await PersistAsync();
        }

        public async Task DeleteAsync(Guid registrationId)
        {
            await InitializeAsync();
            registrations.RemoveAll(r => r.Id == registrationId);
            await PersistAsync();
        }

        public async Task DeleteByEventAsync(int eventId)
        {
            await InitializeAsync();
            registrations.RemoveAll(registration => registration.EventId == eventId);
            await PersistAsync();
        }

        public async Task SyncEventAsync(int eventId, string eventName)
        {
            await InitializeAsync();

            var changed = false;
            foreach (var registration in registrations.Where(registration => registration.EventId == eventId))
            {
                if (!string.Equals(registration.EventName, eventName, StringComparison.Ordinal))
                {
                    registration.EventName = eventName;
                    changed = true;
                }
            }

            if (changed)
            {
                await PersistAsync();
            }
        }

        public async Task ResetAsync()
        {
            await InitializeAsync();
            registrations = new List<AttendeeRegistration>();
            await PersistAsync();
        }

        private async Task MigrateLegacyRegistrationsAsync()
        {
            if (registrations.Count == 0 || registrations.All(registration => registration.EventId > 0))
            {
                return;
            }

            var events = await eventService.GetAllAsync();
            var changed = false;

            foreach (var registration in registrations)
            {
                if (registration.EventId > 0)
                {
                    continue;
                }

                var matchingEvent = events.FirstOrDefault(evt => string.Equals(evt.Name, registration.EventName, StringComparison.OrdinalIgnoreCase));
                if (matchingEvent == null)
                {
                    continue;
                }

                registration.EventId = matchingEvent.Id;
                registration.EventName = matchingEvent.Name;
                changed = true;
            }

            if (changed)
            {
                await PersistAsync();
            }
        }

        private Task PersistAsync()
        {
            return browserStorageService.SetItemAsync(StorageKey, registrations);
        }
    }
}