using System;
using System.ComponentModel.DataAnnotations;

namespace ProductSellingApp.Models
{
    public class Sale
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Total Amount must be greater than or equal to zero.")]
        public decimal TotalAmount { get; set; }

        [Required]
        public DateTime SaleDate { get; set; }
    }
}
