using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProductSellingApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Mobile number must be 11 digits.")]
        [RegularExpression("^01[0-9]{9}$", ErrorMessage = "Mobile number must start with 01.")]
        public string Mobile { get; set; }

        [Required]
        public string Gender { get; set; }


        [Required]
        [StringLength(100, MinimumLength = 6)]
        [RegularExpression("^(?=.*[A-Z])(?=.*[!@#$&*]).{6,}$", ErrorMessage = "Password must be at least 6 characters long and contain at least one uppercase letter and one special character.")]
        public string Password { get; set; }

        // This property will be used only for validation
        [NotMapped]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
