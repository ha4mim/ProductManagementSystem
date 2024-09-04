using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductSellingApp.Data;
using ProductSellingApp.Models;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ProductSellingApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View("ProductList", products); // Make sure this file exists
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            var viewModel = new ProductViewModel
            {
                Categories = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };

            return View("AddProduct", viewModel);
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel viewModel)
        {
            viewModel.Categories = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
            byte[] photoData = null;
            if (viewModel.Photo != null && viewModel.Photo.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await viewModel.Photo.CopyToAsync(memoryStream);
                    photoData = memoryStream.ToArray();
                }
            }
            var product = new Product
            {
                Name = viewModel.Name,
                Quantity = viewModel.Quantity,
                PricePerUnit = viewModel.PricePerUnit,
                CategoryId = viewModel.CategoryId,
                Photo = photoData,
                AddedBy = User.Identity.Name,
                AddedDate = DateTime.Now,
                UpdatedBy = User.Identity.Name, 
                UpdatedDate = DateTime.Now
            };

            
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            
            return RedirectToAction(nameof(Index));
        }
        // GET: Products/Edit/5
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            var viewModel = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Quantity = product.Quantity,
                PricePerUnit = product.PricePerUnit,
                CategoryId = product.CategoryId,
                Categories = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList(),
                ExistingPhoto = product.Photo 
            };

            return View(viewModel);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel viewModel)
        {
                var product = await _context.Products.FindAsync(viewModel.Id);
                if (product == null)
                {
                    return NotFound();
                }
                product.Name = viewModel.Name;
                product.Quantity = viewModel.Quantity;
                product.PricePerUnit = viewModel.PricePerUnit;
                product.CategoryId = viewModel.CategoryId;
                if (viewModel.Photo != null && viewModel.Photo.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await viewModel.Photo.CopyToAsync(memoryStream);
                        product.Photo = memoryStream.ToArray();
                    }
                }

                _context.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
        }
        // GET: Products/Delete/5
        public IActionResult Delete(int id)
        {
            var product = _context.Products
                .Include(p => p.Category) // Ensure the Category is included
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        // POST: Products/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        private async Task ManageProductAsync(Product product, string action, string user)
        {
            var productIdParam = new SqlParameter("@ProductId", product.Id == 0 ? (object)DBNull.Value : product.Id);
    var nameParam = new SqlParameter("@Name", product.Name);
    var quantityParam = new SqlParameter("@Quantity", product.Quantity);
    var priceParam = new SqlParameter("@PricePerUnit", product.PricePerUnit);
    var categoryIdParam = new SqlParameter("@CategoryId", product.CategoryId);
    var photoParam = new SqlParameter("@Photo", product.Photo ?? (object)DBNull.Value);
    var actionParam = new SqlParameter("@Action", action);
    var userParam = new SqlParameter("@User", user);

    await _context.Database.ExecuteSqlRawAsync("EXEC ManageProduct @ProductId, @Name, @Quantity, @PricePerUnit, @CategoryId, @Photo, @Action, @User",
        productIdParam, nameParam, quantityParam, priceParam, categoryIdParam, photoParam, actionParam, userParam);
        }

        
        private async Task<bool> IsDuplicateProductName(string name, int? productId = null)
        {
            if (productId == null)
            {
                return await _context.Products.AnyAsync(p => p.Name == name);
            }
            else
            {
                return await _context.Products.AnyAsync(p => p.Name == name && p.Id != productId.Value);
            }
        }
        // GET: Products/Sell/5
        public async Task<IActionResult> Sell(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var customers = await _context.Customers
    .Select(c => new SelectListItem
    {
        Value = c.Id.ToString(),
        Text = c.Name
    }).ToListAsync();

            ViewBag.Customers = new SelectList(customers, "Value", "Text");

            return View("SellProduct", new SaleViewModel
            {
                Product = product,
                Customers = customers
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sell(SaleViewModel saleViewModel)
        {
            var product = await _context.Products.FindAsync(saleViewModel.Product.Id);

            if (product == null)
            {
                return NotFound();
            }

            if (saleViewModel.Quantity > product.Quantity)
            {
                ModelState.AddModelError("", "Quantity exceeds available stock.");

                var customers = await _context.Customers
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToListAsync();

                var viewModel = new SaleViewModel
                {
                    Product = product,
                    Quantity = saleViewModel.Quantity,
                    Customers = customers,
                    CustomerId = saleViewModel.CustomerId
                };

                return View("SellProduct", viewModel);
            }

            var sale = new Sale
            {
                ProductId = product.Id,
                CustomerId = saleViewModel.CustomerId,
                Quantity = saleViewModel.Quantity,
                TotalAmount = saleViewModel.Quantity * product.PricePerUnit,
                SaleDate = DateTime.Now
            };

            _context.Sales.Add(sale);

            product.Quantity -= saleViewModel.Quantity;
            _context.Products.Update(product);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PrintInvoice), new { id = sale.Id });
        }
        // GET: Products/PrintInvoice/5
        public async Task<IActionResult> PrintInvoice(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Product)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }
    }
}
