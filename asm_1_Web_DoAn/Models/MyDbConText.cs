using Microsoft.EntityFrameworkCore;
using static NuGet.Packaging.PackagingConstants;

namespace asm_1_Web_DoAn.Models
{
    public class MyDbConText : DbContext
    {
        public MyDbConText(DbContextOptions<MyDbConText> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ComboFoodItem> ComboFoodItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<FoodItem>().HasKey(f => f.FoodItemId);
            modelBuilder.Entity<Combo>().HasKey(c => c.ComboId);
            modelBuilder.Entity<Order>().HasKey(o => o.OrderId);
            modelBuilder.Entity<OrderDetail>().HasKey(oct => oct.OrderDetailId);

            modelBuilder.Entity<Order>()
                        .HasOne(o => o.User)
                        .WithMany(u => u.Orders)
                        .HasForeignKey(o => o.UserId);


            modelBuilder.Entity<OrderDetail>()
                        .HasOne(od => od.Order)
                        .WithMany(o => o.OrderDetails)
                        .HasForeignKey(od => od.OrderId);


            modelBuilder.Entity<OrderDetail>()
                        .HasOne(od => od.FoodItem)
                        .WithMany()
                        .HasForeignKey(od => od.FoodItemId);

            modelBuilder.Entity<ComboFoodItem>()
                        .HasKey(cfi => new { cfi.ComboId, cfi.FoodItemId });

            modelBuilder.Entity<ComboFoodItem>()
                        .HasOne(cfi => cfi.Combo)
                        .WithMany(c => c.ComboFoodItems)
                        .HasForeignKey(cfi => cfi.ComboId);

            modelBuilder.Entity<ComboFoodItem>()
                        .HasOne(cfi => cfi.FoodItem)
                        .WithMany(fi => fi.ComboFoodItems)
                        .HasForeignKey(cfi => cfi.FoodItemId);


            modelBuilder.Entity<Combo>()
                        .Property(c => c.ComboPrice)
                        .HasPrecision(18, 2);

            modelBuilder.Entity<FoodItem>()
                        .Property(f => f.Price)
                        .HasPrecision(18, 2);

            modelBuilder.Entity<Cart>()
                        .HasMany(c => c.CartItems)
                        .WithOne(ci => ci.Cart)
                        .HasForeignKey(ci => ci.CartId);

            modelBuilder.Entity<CartItem>()
                        .HasKey(cit => cit.CartItemId); 

            modelBuilder.Entity<CartItem>()
                        .HasOne(ci => ci.FoodItems)
                        .WithMany()
                        .HasForeignKey(ci => ci.FoodItemId)
                        .OnDelete(DeleteBehavior.Cascade);
         
            modelBuilder.Entity<CartItem>()
                        .HasOne(ci => ci.Combos)
                        .WithMany()
                        .HasForeignKey(ci => ci.ComboId)
                        .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<OrderDetail>()
                        .HasKey(od => od.OrderDetailId); 

            modelBuilder.Entity<OrderDetail>()
                        .HasOne(od => od.FoodItem)
                        .WithMany() 
                        .HasForeignKey(od => od.FoodItemId)
                        .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<OrderDetail>()
                        .HasOne(od => od.Combos)
                        .WithMany() 
                        .HasForeignKey(od => od.ComboId)
                        .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
