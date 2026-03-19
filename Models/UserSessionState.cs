using System;

namespace EventEaseApp.Models
{
    public class UserSessionState
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
        public DateTimeOffset SessionStartedUtc { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset LastActivityUtc { get; set; } = DateTimeOffset.UtcNow;
        public string LastVisitedPage { get; set; } = AppRoutes.Events;
        public SearchCriteria SearchCriteria { get; set; } = new();
        public AttendeeRegistration DraftRegistration { get; set; } = new();
    }
}