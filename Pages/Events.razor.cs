using System.Threading;
using EventEaseApp.Models;
using EventEaseApp.Services;
using Microsoft.AspNetCore.Components;

namespace EventEaseApp.Pages
{
    public partial class Events : IDisposable
    {
        [Inject]
        private EventService EventService { get; set; } = default!;

        [Inject]
        private UserSessionService SessionService { get; set; } = default!;

        [Inject]
        private AttendeeRegistrationService RegistrationService { get; set; } = default!;

        [Inject]
        private TooltipService TooltipService { get; set; } = default!;

        private bool isDateValid = true;
        private bool showDateValidation;
        private string dateInputText = string.Empty;
        private List<Event>? events;
        private Event? editingEvent;
        private bool showEventForm;
        private bool isEditMode;
        private string sortField = nameof(Event.Name);
        private bool sortAsc = true;
        private string searchName = string.Empty;
        private string searchDate = string.Empty;
        private string searchLocation = string.Empty;
        private IReadOnlyList<Event> filteredEventsCache = Array.Empty<Event>();
        private Dictionary<int, int> attendeeCounts = new();
        private string lastSearchName = string.Empty;
        private string lastSearchDate = string.Empty;
        private string lastSearchLocation = string.Empty;
        private string lastSortField = nameof(Event.Name);
        private bool lastSortAsc = true;
        private CancellationTokenSource? searchDebounceCts;
        private bool showDeleteModal;
        private Event? eventToDelete;
        private bool showRegistrationModal;
        private Event? registrationEvent;
        private bool showAttendeesModal;
        private Event? attendeesEvent;
        private AttendeeRegistration? editingRegistration;
        private IReadOnlyList<AttendeeRegistration> eventAttendees = Array.Empty<AttendeeRegistration>();
        private bool returnToAttendeesModalAfterRegistration;
        private bool showRegistrationDeleteModal;
        private AttendeeRegistration? registrationToDelete;

        private EventCallback<(string field, string value)> onSearchChangedCallback => EventCallback.Factory.Create<(string field, string value)>(this, OnSearchChanged);

        protected override async Task OnInitializedAsync()
        {
            await EventService.InitializeAsync();
            await SessionService.InitializeAsync();
            await RegistrationService.InitializeAsync();

            await ReloadEventsAsync();
            await RefreshAttendeeCountsAsync();
            await SessionService.SetLastVisitedPageAsync(AppRoutes.Events);

            searchName = SessionService.State.SearchCriteria.Name;
            searchDate = SessionService.State.SearchCriteria.Date;
            searchLocation = SessionService.State.SearchCriteria.Location;
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            return TooltipService.InitializeAsync().AsTask();
        }

        private void OnDatePickerChanged(ChangeEventArgs e)
        {
            dateInputText = e.Value?.ToString() ?? string.Empty;
            showDateValidation = false;
        }

        private IReadOnlyList<Event> GetFilteredEvents()
        {
            if (events == null)
            {
                return Array.Empty<Event>();
            }

            if (searchName == lastSearchName && searchDate == lastSearchDate && searchLocation == lastSearchLocation && sortField == lastSortField && sortAsc == lastSortAsc && filteredEventsCache.Count > 0)
            {
                return filteredEventsCache;
            }

            var filtered = events
                .Where(evt => string.IsNullOrWhiteSpace(searchName) || evt.Name.Contains(searchName, StringComparison.OrdinalIgnoreCase))
                .Where(evt => string.IsNullOrWhiteSpace(searchDate) || evt.Date.ToString("MM/dd/yyyy").Contains(searchDate, StringComparison.Ordinal))
                .Where(evt => string.IsNullOrWhiteSpace(searchLocation) || evt.Location.Contains(searchLocation, StringComparison.OrdinalIgnoreCase));

            Func<Event, object> keySelector = evt => sortField switch
            {
                nameof(Event.Name) => evt.Name,
                nameof(Event.Date) => evt.Date,
                nameof(Event.Location) => evt.Location,
                _ => evt.Name
            };

            filteredEventsCache = (sortAsc
                    ? filtered.OrderBy(keySelector).ThenBy(evt => evt.Id)
                    : filtered.OrderByDescending(keySelector).ThenBy(evt => evt.Id))
                .ToList();

            lastSearchName = searchName;
            lastSearchDate = searchDate;
            lastSearchLocation = searchLocation;
            lastSortField = sortField;
            lastSortAsc = sortAsc;

            return filteredEventsCache;
        }

        private async Task OnSearchChanged((string field, string value) args)
        {
            searchDebounceCts?.Cancel();
            searchDebounceCts?.Dispose();

            var cancellationTokenSource = new CancellationTokenSource();
            searchDebounceCts = cancellationTokenSource;

            try
            {
                await Task.Delay(250, cancellationTokenSource.Token);
                await ApplySearchAsync(args);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (ReferenceEquals(searchDebounceCts, cancellationTokenSource))
                {
                    searchDebounceCts = null;
                }

                cancellationTokenSource.Dispose();
            }
        }

        private async Task ApplySearchAsync((string field, string value) args)
        {
            switch (args.field)
            {
                case nameof(Event.Name):
                    searchName = args.value;
                    break;
                case nameof(Event.Date):
                    searchDate = args.value;
                    break;
                case nameof(Event.Location):
                    searchLocation = args.value;
                    break;
            }

            InvalidateEventCache();
            await SessionService.SaveSearchCriteriaAsync(searchName, searchDate, searchLocation);
            await InvokeAsync(StateHasChanged);
        }

        private async Task ClearSearch()
        {
            searchDebounceCts?.Cancel();
            searchDebounceCts?.Dispose();
            searchDebounceCts = null;

            searchName = string.Empty;
            searchDate = string.Empty;
            searchLocation = string.Empty;
            lastSearchName = string.Empty;
            lastSearchDate = string.Empty;
            lastSearchLocation = string.Empty;

            InvalidateEventCache();
            await SessionService.SaveSearchCriteriaAsync(searchName, searchDate, searchLocation);
            StateHasChanged();
        }

        private Task OnSortBy(string field)
        {
            if (sortField == field)
            {
                sortAsc = !sortAsc;
            }
            else
            {
                sortField = field;
                sortAsc = true;
            }

            StateHasChanged();
            return Task.CompletedTask;
        }

        private string GetSortIcon(string field)
        {
            if (sortField != field)
            {
                return string.Empty;
            }

            return sortAsc ? "▲" : "▼";
        }

        private void ShowNewEventForm()
        {
            editingEvent = new Event { Date = DateTime.Today };
            dateInputText = editingEvent.Date.ToString("yyyy-MM-dd");
            showDateValidation = false;
            isDateValid = true;
            showEventForm = true;
            isEditMode = false;
        }

        private async Task HandleEventSubmit()
        {
            showDateValidation = true;
            isDateValid = TryApplyDateInput();
            if (!isDateValid || editingEvent == null || events == null)
            {
                return;
            }

            if (isEditMode)
            {
                var existing = events.FirstOrDefault(evt => evt.Id == editingEvent.Id);
                if (existing != null)
                {
                    existing.Name = editingEvent.Name;
                    existing.Date = editingEvent.Date;
                    existing.Location = editingEvent.Location;

                    await EventService.UpdateAsync(existing);
                    await RegistrationService.SyncEventAsync(existing.Id, existing.Name);
                }
            }
            else
            {
                await EventService.AddAsync(new Event
                {
                    Name = editingEvent.Name,
                    Date = editingEvent.Date,
                    Location = editingEvent.Location
                });
            }

            await ReloadEventsAsync();
            await ReloadAttendeesAsync();
            await RefreshAttendeeCountsAsync();
            InvalidateEventCache();
            ResetEventFormState();
            StateHasChanged();
        }

        private Task CancelEventForm()
        {
            ResetEventFormState();
            return Task.CompletedTask;
        }

        private Task OnEditEvent(Event evt)
        {
            editingEvent = new Event
            {
                Id = evt.Id,
                Name = evt.Name,
                Date = evt.Date,
                Location = evt.Location
            };

            dateInputText = evt.Date.ToString("yyyy-MM-dd");
            showDateValidation = false;
            isDateValid = true;
            showEventForm = true;
            isEditMode = true;

            return Task.CompletedTask;
        }

        private bool TryApplyDateInput()
        {
            if (editingEvent == null || !DateTime.TryParse(dateInputText, out var parsedDate))
            {
                return false;
            }

            editingEvent.Date = parsedDate;
            return true;
        }

        private Task ShowDeleteModal(Event evt)
        {
            eventToDelete = evt;
            showDeleteModal = true;
            return Task.CompletedTask;
        }

        private Task CloseDeleteModal()
        {
            showDeleteModal = false;
            eventToDelete = null;
            return Task.CompletedTask;
        }

        private async Task ConfirmDelete()
        {
            if (eventToDelete != null)
            {
                await RegistrationService.DeleteByEventAsync(eventToDelete.Id);
                await EventService.DeleteAsync(eventToDelete.Id);

                if (registrationEvent?.Id == eventToDelete.Id)
                {
                    CloseRegistrationModal();
                }

                if (attendeesEvent?.Id == eventToDelete.Id)
                {
                    await CloseAttendeesModal();
                }

                await ReloadEventsAsync();
                await RefreshAttendeeCountsAsync();
                InvalidateEventCache();
            }

            await CloseDeleteModal();
            StateHasChanged();
        }

        private async Task ShowRegistrationModalAsync()
        {
            if (attendeesEvent != null)
            {
                await ShowRegistrationModalAsync(attendeesEvent);
            }
        }

        private async Task ShowRegistrationModalAsync(Event evt)
        {
            await SessionService.ClearDraftRegistrationAsync();
            registrationEvent = evt;
            editingRegistration = null;
            returnToAttendeesModalAfterRegistration = attendeesEvent?.Id == evt.Id;
            showAttendeesModal = false;
            showRegistrationModal = true;
        }

        private void CloseRegistrationModal()
        {
            showRegistrationModal = false;
            registrationEvent = null;
            editingRegistration = null;

            if (returnToAttendeesModalAfterRegistration && attendeesEvent != null)
            {
                showAttendeesModal = true;
            }

            returnToAttendeesModalAfterRegistration = false;
        }

        private async Task OnRegistrationSaved()
        {
            await RefreshAttendeeCountsAsync();
            await ReloadAttendeesAsync();
            CloseRegistrationModal();
        }

        private async Task ShowAttendeesModal(Event evt)
        {
            attendeesEvent = evt;
            eventAttendees = await RegistrationService.GetByEventAsync(evt.Id);
            showAttendeesModal = true;
        }

        private Task CloseAttendeesModal()
        {
            showAttendeesModal = false;
            attendeesEvent = null;
            eventAttendees = Array.Empty<AttendeeRegistration>();
            return Task.CompletedTask;
        }

        private async Task EditAttendeeRegistrationAsync(AttendeeRegistration attendee)
        {
            if (attendeesEvent == null)
            {
                return;
            }

            registrationEvent = attendeesEvent;
            editingRegistration = attendee;
            returnToAttendeesModalAfterRegistration = true;
            showAttendeesModal = false;
            showRegistrationModal = true;
            await InvokeAsync(StateHasChanged);
        }

        private Task ShowRegistrationDeleteModal()
        {
            if (editingRegistration == null)
            {
                return Task.CompletedTask;
            }

            registrationToDelete = editingRegistration;
            showRegistrationDeleteModal = true;
            return Task.CompletedTask;
        }

        private Task ShowRegistrationDeleteModalAsync(AttendeeRegistration attendee)
        {
            registrationToDelete = attendee;
            showRegistrationDeleteModal = true;
            return Task.CompletedTask;
        }

        private Task CloseRegistrationDeleteModal()
        {
            showRegistrationDeleteModal = false;
            registrationToDelete = null;
            return Task.CompletedTask;
        }

        private async Task ConfirmRegistrationDeleteAsync()
        {
            if (registrationToDelete == null)
            {
                return;
            }

            var deletedRegistrationId = registrationToDelete.Id;
            var shouldReturnToAttendees = showRegistrationModal || showAttendeesModal || returnToAttendeesModalAfterRegistration;

            await RegistrationService.DeleteAsync(deletedRegistrationId);
            await RefreshAttendeeCountsAsync();
            await ReloadAttendeesAsync();

            if (editingRegistration?.Id == deletedRegistrationId)
            {
                editingRegistration = null;
            }

            if (showRegistrationModal)
            {
                returnToAttendeesModalAfterRegistration = shouldReturnToAttendees && attendeesEvent != null;
                CloseRegistrationModal();
            }

            await CloseRegistrationDeleteModal();
            StateHasChanged();
        }

        private int GetAttendeeCount(Event evt)
        {
            return attendeeCounts.TryGetValue(evt.Id, out var count) ? count : 0;
        }

        private async Task RefreshAttendeeCountsAsync()
        {
            attendeeCounts = (await RegistrationService.GetAllAsync())
                .GroupBy(registration => registration.EventId)
                .Where(group => group.Key > 0)
                .ToDictionary(group => group.Key, group => group.Count());
        }

        private async Task ReloadEventsAsync()
        {
            events = (await EventService.GetAllAsync()).ToList();

            if (attendeesEvent != null)
            {
                attendeesEvent = events.FirstOrDefault(evt => evt.Id == attendeesEvent.Id);
            }

            if (registrationEvent != null)
            {
                registrationEvent = events.FirstOrDefault(evt => evt.Id == registrationEvent.Id);
            }
        }

        private async Task ReloadAttendeesAsync()
        {
            if (attendeesEvent == null)
            {
                eventAttendees = Array.Empty<AttendeeRegistration>();
                return;
            }

            eventAttendees = await RegistrationService.GetByEventAsync(attendeesEvent.Id);
        }

        private void ResetEventFormState()
        {
            showEventForm = false;
            editingEvent = null;
            isEditMode = false;
            dateInputText = string.Empty;
            showDateValidation = false;
            isDateValid = true;
        }

        private void InvalidateEventCache()
        {
            filteredEventsCache = Array.Empty<Event>();
        }

        private string GetEventDeleteMessage()
        {
            return $"Are you sure you want to delete the event {eventToDelete?.Name ?? string.Empty}? This will also remove its attendee registrations.";
        }

        private string GetRegistrationDeleteMessage()
        {
            return $"Are you sure you want to delete the registration for {GetRegistrationDisplayName(registrationToDelete)}?";
        }

        private static string GetRegistrationDisplayName(AttendeeRegistration? registration)
        {
            if (registration == null)
            {
                return string.Empty;
            }

            var fullName = $"{registration.FirstName} {registration.LastName}".Trim();
            return string.IsNullOrWhiteSpace(fullName) ? registration.Email : fullName;
        }

        public void Dispose()
        {
            searchDebounceCts?.Cancel();
            searchDebounceCts?.Dispose();
        }
    }
}