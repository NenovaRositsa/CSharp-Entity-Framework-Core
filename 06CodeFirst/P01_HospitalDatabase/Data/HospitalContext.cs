using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using P01_HospitalDatabase.Data.Models;

namespace P01_HospitalDatabase.Data
{
   public class HospitalContext : DbContext
    {
        public HospitalContext()
        {

        }

        public HospitalContext(DbContextOptions options)
           : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=NENOVI-PC\SQLEXPRESS;Database=HospitalDatabase;Integrated Security=True;");
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.Property(x => x.FirstName)
                .IsUnicode();

                entity.Property(x => x.LastName)
                .IsUnicode();

                entity.Property(x => x.Address)
                .IsUnicode();

                entity.Property(x => x.Email)
                .IsUnicode(false);
            });

            modelBuilder.Entity<Visitation>(entity =>
            {
                entity.Property(x => x.Comments)
                .IsUnicode();
            });

            modelBuilder.Entity<Diagnose>(entity =>

            {
                entity.Property(x => x.Name)
                .IsUnicode();
                entity.Property(x => x.Comments)
                .IsUnicode();
            });

            modelBuilder.Entity<Medicament>(entity =>
            {
                entity.Property(x => x.Name)
                .IsUnicode();

            });

            modelBuilder.Entity<PatientMedicament>(entity =>
            {
                entity.HasKey(x => new { x.PatientId, x.MedicamentId });

            });

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.Property(x => x.Name)
                .IsUnicode();

                entity.Property(x => x.Specialty)
                .IsUnicode();
            });
        }


    }
}
