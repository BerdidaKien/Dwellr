using Microsoft.AspNetCore.Identity;

namespace Dwellr.Models
{
    public enum VerificationStatus
    {
        Unverified,
        EmailVerified,
        IDSubmitted,
        IDApproved,
        Banned
    }

    public enum UserRole
    {
        Tenant,
        Landlord,
        Both
    }

    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public UserRole ActiveRole { get; set; } = UserRole.Tenant;
        public VerificationStatus VerificationStatus { get; set; }
            = VerificationStatus.Unverified;
        public string? IDDocumentPath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal WalletBalance { get; set; } = 0;
        public ICollection<Property> Properties { get; set; }
            = new List<Property>();
        public ICollection<Message> SentMessages { get; set; }
            = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; }
            = new List<Message>();
        public ICollection<Report> ReportsReceived { get; set; }
            = new List<Report>();
        public ICollection<Transaction> SentTransactions { get; set; }
            = new List<Transaction>();
        public ICollection<Transaction> ReceivedTransactions { get; set; }
            = new List<Transaction>();
    }
}