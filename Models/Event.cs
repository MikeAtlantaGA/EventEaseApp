using System.ComponentModel.DataAnnotations;

namespace EventEaseApp.Models
{
    public class Event : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Date == DateTime.MinValue)
            {
                yield return new ValidationResult("A valid event date is required.", new[] { nameof(Date) });
            }
        }
    }
}
