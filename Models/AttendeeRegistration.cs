using System;
using System.ComponentModel.DataAnnotations;

namespace EventEaseApp.Models
{
    public class AttendeeRegistration
    {
        public Guid Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A valid event selection is required.")]
        public int EventId { get; set; }

        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string TicketType { get; set; } = string.Empty;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the registration terms.")]
        public bool AcceptTerms { get; set; }

        public DateTimeOffset RegisteredAtUtc { get; set; }
    }
}