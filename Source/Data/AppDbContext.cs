using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using FuriaKYFApi.Source.Models;

namespace FuriaKYFApi.Source.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Fan> Fans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Fan>()
                .HasIndex(f => f.DocumentNumber)
                .IsUnique();

            modelBuilder.Entity<Fan>()
                .HasIndex(f => f.Email)
                .IsUnique();               
            
            modelBuilder.Entity<Fan>()
                .HasIndex(f => f.RedditAccessToken)
                .IsUnique();            
            
            modelBuilder.Entity<Fan>()
                .Property(f => f.AboutYou)
                .HasMaxLength(255);

            modelBuilder.Entity<Fan>()
                .Property(f => f.Name)
                .HasMaxLength(255);

            modelBuilder.Entity<Fan>()
                .Property(f => f.Document)
                .HasMaxLength(255);            
            
            modelBuilder.Entity<Fan>()
                .Property(f => f.DocumentNumber)
                .HasMaxLength(255);       
            
            modelBuilder.Entity<Fan>()
                .Property(f => f.Email)
                .HasMaxLength(255);            
            
            modelBuilder.Entity<Fan>()
                .Property(f => f.RedditAccessToken)
                .HasMaxLength(2048);

        }
    }
}