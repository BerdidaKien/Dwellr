namespace Dwellr.Models
{
    public class SavedListing
    {
        public int SavedListingId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public AppUser? User { get; set; }
        public int PropertyId { get; set; }
        public Property? Property { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}