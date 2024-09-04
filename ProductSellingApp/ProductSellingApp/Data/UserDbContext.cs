using Microsoft.EntityFrameworkCore;
using ProductSellingApp.Models;
using System.Collections.Generic;

namespace ProductSellingApp.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
