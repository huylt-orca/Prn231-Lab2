using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GrpcServiceDemo.Models
{
    public class MyDBContext : DbContext
    {
        public MyDBContext(DbContextOptions<MyDBContext> options) : base(options) {
        
        }

        #region DBSet
        public DbSet<Book> Books { get; set; }
        public DbSet<Press> Presss { get; set; }
        public DbSet<User> Users { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>().OwnsOne(c => c.Location);

            modelBuilder.Entity<Press>().HasData(
              new Press { Id = 1, Name = "Beverages" , Category = Category.Book},
              new Press { Id = 2, Name = "Condiments", Category = Category.Book },
              new Press { Id = 3, Name = "Confections", Category = Category.Book },
              new Press { Id = 4, Name = "Dairy Products", Category = Category.Book },
              new Press { Id = 5, Name = "Grains/Cereals", Category = Category.Book },
              new Press { Id = 6, Name = "Meat/Cereals", Category = Category.Book },
              new Press { Id = 7, Name = "Produce", Category = Category.Book },
              new Press { Id = 8, Name = "Seafood", Category = Category.Book }
               );

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Name = "Admin", Username = "Admin", Password = "x3Xnt1ft5jDNCqERO9ECZhqziCnKUqZCKreChi8mhkY=", Role = Role.ADMIN },
                new User { Id = 2, Name = "User", Username = "User", Password = "x3Xnt1ft5jDNCqERO9ECZhqziCnKUqZCKreChi8mhkY=", Role = Role.USER }
                );
        }
    }
}
