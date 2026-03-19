using System;
using System.Text.Json;
using EventEaseApp.Models;

namespace EventEaseApp.Services
{
    public class UserSessionService
    {
        private const string StorageKey = "eventease.user-session";
        private readonly BrowserStorageService browserStorageService;
        private readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);
        private bool isInitialized;

        public UserSessionState State { get; private set; } = new();

        public event Action? OnChange;

        public UserSessionService(BrowserStorageService browserStorageService)
        {
            this.browserStorageService = browserStorageService;
        }

        public async Task InitializeAsync()
        {
            if (isInitialized)
            {
                return;
            }

            State = await browserStorageService.GetItemAsync<UserSessionState>(StorageKey) ?? new UserSessionState();
            State.LastActivityUtc = DateTimeOffset.UtcNow;
            isInitialized = true;
            await PersistAsync(notify: false);
        }

        public async Task SetLastVisitedPageAsync(string path)
        {
            await InitializeAsync();
            State.LastVisitedPage = path;
            await PersistAsync();
        }

        public async Task SaveSearchCriteriaAsync(string name, string date, string location)
        {
            await InitializeAsync();
            State.SearchCriteria.Name = name;
            State.SearchCriteria.Date = date;
            State.SearchCriteria.Location = location;
            await PersistAsync();
        }

        public async Task SaveDraftRegistrationAsync(AttendeeRegistration registration)
        {
            await InitializeAsync();
            State.DraftRegistration = Clone(registration);
            await PersistAsync();
        }

        public async Task ClearDraftRegistrationAsync()
        {
            await InitializeAsync();
            State.DraftRegistration = new AttendeeRegistration();
            await PersistAsync();
        }

        public async Task ResetSessionAsync()
        {
            State = new UserSessionState();
            isInitialized = true;
            await browserStorageService.RemoveItemAsync(StorageKey);
            await PersistAsync();
        }

        private async Task PersistAsync(bool notify = true)
        {
            State.LastActivityUtc = DateTimeOffset.UtcNow;
            await browserStorageService.SetItemAsync(StorageKey, State);
            if (notify)
            {
                OnChange?.Invoke();
            }
        }

        private AttendeeRegistration Clone(AttendeeRegistration registration)
        {
            var json = JsonSerializer.Serialize(registration, jsonOptions);
            return JsonSerializer.Deserialize<AttendeeRegistration>(json, jsonOptions) ?? new AttendeeRegistration();
        }
    }
}