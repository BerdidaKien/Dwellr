using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dwellr.Models
{
    public enum PropertyType
    {
        House,
        Room,
        Bedspace
    }

    public enum PropertyStatus
    {
        Draft,
        Published,
        Rented,
        Hidden
    }

    public class Property
    {
        public int PropertyId { get; set; }

        [Required]
        [MinLength(10)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(50)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        public PropertyType Type { get; set; }

        public PropertyStatus Status { get; set; } = PropertyStatus.Draft;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string District { get; set; } = string.Empty;

        public string? Address { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public double AreaSqM { get; set; }
        public DateTime AvailableFrom { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string LandlordId { get; set; } = string.Empty;
        public AppUser? Landlord { get; set; }

        public ICollection<PropertyPhoto> Photos { get; set; }
            = new List<PropertyPhoto>();
        public ICollection<Message> Messages { get; set; }
            = new List<Message>();
    }
}