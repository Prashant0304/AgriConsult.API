using AgriConsult.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriConsult.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        

        public DbSet<Expert> Experts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Consultation> Consultations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Expert>()
                .Property(e => e.Fee)
                .HasPrecision(10, 2); 
        }

    }
}
