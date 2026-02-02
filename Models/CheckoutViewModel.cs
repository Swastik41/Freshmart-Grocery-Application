using System.ComponentModel.DataAnnotations;

namespace FreshMart.Models
{
    public class CheckoutViewModel : IValidatableObject
    {
        public int UserId { get; set; }

        // FULL NAME
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 40 characters.")]
        public string FullName { get; set; } = string.Empty;

        // EMAIL
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        // ADDRESS
        [Required(ErrorMessage = "Address is required.")]
        [MinLength(5, ErrorMessage = "Address must be at least 5 characters.")]
        public string Address { get; set; } = string.Empty;

        // PHONE
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter a valid 10-digit phone number.")]
        public string Phone { get; set; } = string.Empty;

        // POSTAL CODE (CANADA)
        [Required(ErrorMessage = "Postal code is required.")]
        [RegularExpression(@"^[A-Za-z]\d[A-Za-z]\s?\d[A-Za-z]\d$",
            ErrorMessage = "Enter a valid postal code (A1B 2C3).")]
        public string PostalCode { get; set; } = string.Empty;

        // PAYMENT METHOD
        [Required(ErrorMessage = "Please select a payment method.")]
        public string PaymentMethod { get; set; } = string.Empty;

        // CARD NUMBER (ONLY FOR CREDIT / DEBIT)
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must be 16 digits.")]
        public string? CardNumber { get; set; }

        // EXPIRY MM/YY
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Expiry must be in MM/YY format.")]
        public string? Expiry { get; set; }

        // CVV
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV must be 3 digits.")]
        public string? CVV { get; set; }

        // 🔐 CONDITIONAL SERVER-SIDE VALIDATION
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PaymentMethod == "Credit Card" || PaymentMethod == "Debit Card")
            {
                if (string.IsNullOrWhiteSpace(CardNumber))
                    yield return new ValidationResult("Card number is required.", new[] { nameof(CardNumber) });

                if (string.IsNullOrWhiteSpace(Expiry))
                    yield return new ValidationResult("Expiry date is required.", new[] { nameof(Expiry) });

                if (string.IsNullOrWhiteSpace(CVV))
                    yield return new ValidationResult("CVV is required.", new[] { nameof(CVV) });
            }
        }
    }
}