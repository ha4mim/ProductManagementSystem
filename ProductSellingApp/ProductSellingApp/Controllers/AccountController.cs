using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductSellingApp.Data;
using ProductSellingApp.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ProductSellingApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserDbContext _context;

        public AccountController(UserDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            if (ModelState.IsValid)
            {
                
                var existingUser = await _context.Users
                    .AnyAsync(u => u.Email == model.Email);

                if (existingUser)
                {
                    ModelState.AddModelError("", "An account with this email already exists.");
                    return View(model);
                }

                
                model.Password = EncryptPassword(model.Password);

                
                await RegisterUserAsync(model);

                return RedirectToAction("Login");
            }
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var encryptedPassword = EncryptPassword(model.Password);
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == encryptedPassword);

                if (user != null)
                {
                    
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email)
                
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    return RedirectToAction("Index", "Products");
                }
                ModelState.AddModelError("", "Invalid login attempt.");
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (await CheckUserExistenceAsync(email))
            {
                
                return RedirectToAction("ChangePassword"); 
            }
            else
            {
                TempData["ErrorMessage"] = "No account found with this email address. Please sign up.";
                return RedirectToAction("Register"); 
            }
        }

        // GET: /Account/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword(string email)
        {
            var model = new ChangePasswordViewModel { Email = email };
            return View(model);
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null)
                {
                   
                    var encryptedOldPassword = EncryptPassword(model.OldPassword);

                    // Validate the old password
                    if (user.Password == encryptedOldPassword)
                    {
                        
                        var encryptedNewPassword = EncryptPassword(model.NewPassword);

                        
                        await ChangeUserPasswordAsync(user.Email, encryptedNewPassword);

                        return RedirectToAction("Login");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Old password is incorrect.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "User not found.");
                }
            }
            return View(model);
        }

        private async Task RegisterUserAsync(User model)
        {
            var parameters = new[]
            {
            new SqlParameter("@Name", model.Name),
            new SqlParameter("@Email", model.Email),
            new SqlParameter("@Mobile", model.Mobile),
            new SqlParameter("@Gender", model.Gender),
            new SqlParameter("@Password", model.Password)
        };

            await _context.Database.ExecuteSqlRawAsync("EXEC RegisterUser @Name, @Email, @Mobile, @Gender, @Password", parameters);
        }

        private async Task ChangeUserPasswordAsync(string email, string newPassword)
        {
            var emailParam = new SqlParameter("@Email", email);
            var newPasswordParam = new SqlParameter("@NewPassword", newPassword);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.ChangeUserPassword @Email, @NewPassword",
                emailParam, newPasswordParam);
        }

        private async Task<bool> CheckUserExistenceAsync(string email)
        {
            
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "dbo.CheckUserExistence";
                command.CommandType = System.Data.CommandType.StoredProcedure;

                
                var emailParam = command.CreateParameter();
                emailParam.ParameterName = "@Email";
                emailParam.Value = email;
                command.Parameters.Add(emailParam);

                
                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }

                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var result = reader.GetInt32(reader.GetOrdinal("UserExists"));
                        return result == 1;
                    }
                }
            }

            return false;
        }
        private string EncryptPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}

