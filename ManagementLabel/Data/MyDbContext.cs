using ManagementLabel.ProductsF;
using Microsoft.EntityFrameworkCore;
using ManagementLabel.LogIn;
using ManagementLabel.ManufacturerF;
namespace ManagementLabel.Data
{
    public class MyDbContext : DbContext
    {
        public DbSet<Products> products { get; set; }
        public DbSet<Categories> categories { get; set; }
        public DbSet<Users> users { get; set; }
        public DbSet<Manufacturer> manufacturer { get; set; }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
    }
}