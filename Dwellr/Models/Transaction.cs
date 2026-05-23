using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dwellr.Models
{
    public enum TransactionType
    {
        TopUp,
        Deposit,
        Refund
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }

    public class Transaction
    {
        public int TransactionId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; }
            = TransactionStatus.Completed;

        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? SenderId { get; set; }
        public AppUser? Sender { get; set; }

        public string? ReceiverId { get; set; }
        public AppUser? Receiver { get; set; }

        public int? PropertyId { get; set; }
        public Property? Property { get; set; }
    }
}