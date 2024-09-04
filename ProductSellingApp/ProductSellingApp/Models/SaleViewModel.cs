using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProductSellingApp.Models
{
    public class SaleViewModel
    {
        public Product Product { get; set; }

        public int CustomerId { get; set; }
        public List<SelectListItem> Customers { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }
    }
}
