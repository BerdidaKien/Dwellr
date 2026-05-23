using System.ComponentModel.DataAnnotations;

namespace Dwellr.Models
{
    public class Message
    {
        public int MessageId { get; set; }

        [Required]
        public string Body { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        public string SenderId { get; set; } = string.Empty;
        public AppUser? Sender { get; set; }

        public string ReceiverId { get; set; } = string.Empty;
        public AppUser? Receiver { get; set; }

        public int PropertyId { get; set; }
        public Property? Property { get; set; }
    }
}