using Bags_Shop_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Bags_Shop_API.ContextFile
{
	public class Context:DbContext
	{
		public Context(DbContextOptions options):base(options)
        {
			
		}
		public DbSet<Product>  Products { get; set; }
		public DbSet<Collection>  Collections { get; set; }
		public DbSet<Image>  Images { get; set; }
		public DbSet<Payment>  Payments { get; set; }
		public DbSet<PaymentWebhook>  PaymentWebhooks { get; set; }
		public DbSet<Discount>  Discounts { get; set; }
		public DbSet<Order>  Orders { get; set; }
		public DbSet<OrderItem>  OrderItems { get; set; }
		public DbSet<PaymentWebhook>   paymentWebhooks { get; set; }
		override protected void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Product>().HasMany(p => p.Images).WithOne(i => i.Product).HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<Product>().HasMany(p => p.orderItems).WithOne(i => i.Product).HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<Collection>().HasMany(p => p.Images).WithOne(i => i.Collection).HasForeignKey(i => i.CollectionId).OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<Product>().HasOne(p => p.Collection).WithMany(i => i.Products).HasForeignKey(i => i.CollectionId).OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<OrderItem>().HasKey(o => new { o.OrderId, o.ProductId });
			modelBuilder.Entity<Discount>().HasMany(d => d.Products).WithOne(p => p.Discount).HasForeignKey(p => p.DiscountId).OnDelete(DeleteBehavior.SetNull);
			modelBuilder.Entity<Payment>().HasOne(p => p.Order).WithMany(o => o.Payments).HasForeignKey(p => p.OrderId);
            modelBuilder.Entity<PaymentWebhook>()
        .Property(pw => pw.AmountCents)
        .HasPrecision(18, 2);
            modelBuilder.Entity<Discount>()
                .Property(d => d.DiscountPercentage)
                .HasPrecision(5, 2);
            modelBuilder.Entity<Product>()
        .Property(p => p.Price)
        .HasPrecision(18, 2);


        }
    }
}
