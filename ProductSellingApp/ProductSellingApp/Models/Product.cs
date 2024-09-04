using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProductSellingApp.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price per unit cannot be negative.")]
        public decimal PricePerUnit { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public byte[]? Photo { get; set; }

        public string? AddedBy { get; set; }

        public DateTime? AddedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        
        public virtual Category? Category { get; set; }

    }
}
