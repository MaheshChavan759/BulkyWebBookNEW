using BulkyWebBook.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BulkyWebBook.DataAccess.Data
{
    public class ApplicationDbContext:IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        public DbSet<ApplicationUser> applicationUsers{ get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Action", DisplaeyCategoryOrder = 1 },
                new Category { CategoryId = 2, CategoryName = "Sc-fi", DisplaeyCategoryOrder = 2 },
                new Category { CategoryId = 3, CategoryName = "Drama", DisplaeyCategoryOrder = 13 }
                );

            modelBuilder.Entity<Product>().HasData(
               new Product { Id=1,
                   Title="Powerful",
                   Description="At main man",
                   Author= "The Don",
                   ListPrise= 10000,
                   Prise50 = 8700,
                   Prise100=5000,
                   CategoryId=1,
                   ImageUrl = ""
               },
               new Product
               {
                   Id = 3,
                   Title = "Powerful",
                   Description = "At main man",
                   Author = "The Don",
                   ListPrise = 10000,
                   Prise50 = 8700,
                   Prise100 = 5000,
                   CategoryId = 2,
                   ImageUrl=""
                   
               }
                );
        }
    }
}
