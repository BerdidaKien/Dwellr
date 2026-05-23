using System.ComponentModel.DataAnnotations;

namespace Dwellr.Models
{
    public enum ReportStatus
    {
        Pending,
        Reviewed,
        Actioned
    }

    public class Report
    {
        public int ReportId { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        public ReportStatus Status { get; set; } = ReportStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string ReporterId { get; set; } = string.Empty;
        public AppUser? Reporter { get; set; }

        public string ReportedUserId { get; set; } = string.Empty;
        public AppUser? ReportedUser { get; set; }

        public int? PropertyId { get; set; }
        public Property? Property { get; set; }
    }
}