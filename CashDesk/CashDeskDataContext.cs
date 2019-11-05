using CashDesk.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CashDesk
{
    public class CashDeskDataContext : DbContext
    {
        public DbSet<Member> Members { get; set; }

        public DbSet<Membership> Memberships { get; set; }

        public DbSet<Deposit> Deposits { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("myDatabase");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Member>()
                .HasMany(m => m.Memberships)
                .WithOne(m => m.Member)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Membership>()
                .HasMany(m => m.Deposits)
                .WithOne(d => d.Membership)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
