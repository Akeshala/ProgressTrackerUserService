using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using ProgressTrackerUserService.Models;

namespace ProgressTrackerUserService.Data
{
    public class DbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbContext(DbContextOptions<DbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}