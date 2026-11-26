using System.ComponentModel.DataAnnotations;

namespace Dwellr.Models
{
    public class WaitlistEntry
    {
        public int WaitlistEntryId { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? FullName { get; set; }
        public string? Occupation { get; set; }     // Student / Young professional
        public string? PreferredArea { get; set; }  // e.g. near USC, IT Park
        public int? Budget { get; set; }
        public string? Source { get; set; }         // facebook, campus-gc, etc.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}