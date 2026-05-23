using System.ComponentModel.DataAnnotations;

namespace Dwellr.Models.ViewModels
{
    public class TopUpViewModel
    {
        [Required]
        [Range(100, 100000,
            ErrorMessage = "Amount must be between ₱100 and ₱100,000")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Card number is required")]
        [StringLength(16, MinimumLength = 16,
            ErrorMessage = "Card number must be 16 digits")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Card holder name is required")]
        public string CardHolder { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry date is required")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$",
            ErrorMessage = "Use format MM/YY")]
        public string ExpiryDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "CVV is required")]
        [StringLength(3, MinimumLength = 3,
            ErrorMessage = "CVV must be 3 digits")]
        public string CVV { get; set; } = string.Empty;
    }
}