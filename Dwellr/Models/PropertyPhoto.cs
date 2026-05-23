namespace Dwellr.Models
{
    public class PropertyPhoto
    {
        public int PropertyPhotoId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public bool IsCover { get; set; } = false;
        public int PropertyId { get; set; }
        public Property? Property { get; set; }
    }
}