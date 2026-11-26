using System.ComponentModel.DataAnnotations;

namespace Dwellr.Models
{
    public enum LeadStatus
    {
        New,
        Contacted,
        IdChecked,
        Listed,
        Declined
    }

    public class ListingRequest
    {
        public int ListingRequestId { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Area { get; set; } = string.Empty;

        public PropertyType Type { get; set; }
        public int? ExpectedPrice { get; set; }
        public string? Notes { get; set; }

        public LeadStatus Status { get; set; } = LeadStatus.New;
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}