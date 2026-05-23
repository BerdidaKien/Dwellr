using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dwellr.Models
{
    public enum ApplicationStatus
    {
        Pending,
        Paid,
        Cancelled,
        Refunded
    }

    public class RentalApplication
    {
        public int RentalApplicationId { get; set; }
        public ApplicationStatus Status { get; set; }
            = ApplicationStatus.Pending;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DepositAmount { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        public string TenantId { get; set; } = string.Empty;
        public AppUser? Tenant { get; set; }

        public int PropertyId { get; set; }
        public Property? Property { get; set; }

        public int? TransactionId { get; set; }
        public Transaction? Transaction { get; set; }
    }
}