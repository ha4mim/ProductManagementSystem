using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProductSellingApp.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        [Display(Name = "Price Per Unit")]
        public decimal PricePerUnit { get; set; }

        [Required(ErrorMessage = "The Category field is required.")]
        public int CategoryId { get; set; } // This is what should be required, not the Categories list

        public List<SelectListItem> Categories { get; set; }

        [Required(ErrorMessage = "Please upload a photo.")]
        [Display(Name = "Photo")]
        public IFormFile Photo { get; set; }
        public byte[] ExistingPhoto { get; set; } // For displaying existing photo

        public string ExistingPhotoBase64
        {
            get
            {
                if (ExistingPhoto != null && ExistingPhoto.Length > 0)
                {
                    return "data:image/jpeg;base64," + Convert.ToBase64String(ExistingPhoto);
                }
                return null;
            }
        }
    }
}
