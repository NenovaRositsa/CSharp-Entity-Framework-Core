using Microsoft.EntityFrameworkCore;
using P03_SalesDatabase.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace P03_SalesDatabase.Data
{
   public class SalesContext : DbContext
    {
        public SalesContext()
        {

        }

        public SalesContext(DbContextOptions options)
            : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=NENOVI-PC\SQLEXPRESS;Database=Sales;Integrated Security=True;");
            }
            base.OnConfiguring(optionsBuilder); 
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(x => x.Name)
                .IsUnicode();

                entity.Property(x => x.Description)
                .HasDefaultValue("No description");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(x => x.Name)
                .IsUnicode();

                entity.Property(x => x.Email)
                .IsUnicode(false);

                entity.Property(x => x.CreditCardNumber)
                .IsUnicode(false);
            });

            modelBuilder.Entity<Store>(entity =>
            {
                entity.Property(x => x.Name)
                .IsUnicode();
            });

            modelBuilder.Entity<Sale>(entity =>
            {
                entity.Property(x => x.Date)
                .HasDefaultValueSql("GETDATE()");
            });
        }
    }
}
