using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; // Add this line


namespace API.Data
{
    public class StoreContext: IdentityDbContext<User>
    {
        public StoreContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Basket> Baskets { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(
                //Set the Roles
                new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Name = "Member", NormalizedName = "MEMBER" }
            );
        }
    }
}