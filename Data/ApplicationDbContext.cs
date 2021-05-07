using InventoryApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Prefix>().HasMany(x => x.Products)
                   .WithOne(p => p.Prefix)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Suffix>().HasMany(x => x.Products)
                   .WithOne(s => s.Suffix)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Product>().HasMany(x => x.Receives)
                   .WithOne(p => p.Product)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>().HasMany(x => x.PurchaseReturns)
                   .WithOne(p => p.Product)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>().HasMany(x => x.Issues)
                    .WithOne(p => p.Product)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>().HasMany(x => x.SaleReturns)
                    .WithOne(p => p.Product)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>().HasOne(x => x.Stock)
                    .WithOne(p => p.Product)
                    .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(builder);
        }

        public DbSet<Country> Countries { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<Prefix> Prefixes { get; set; }
        public DbSet<Suffix> Suffixes { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Receive> Receives { get; set; }
        public DbSet<PurchaseReturn> PurchaseReturns { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<SaleReturn> SaleReturns { get; set; }

    }
}
