using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<LocalBook> LocalBooks { get; set; } = null!;
        public DbSet<BookLoan> BookLoans { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<WishlistItem> WishlistItems { get; set; } = null!;
        public DbSet<SearchHistory> SearchHistories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<LocalBook>().HasKey(b => b.Id);
            modelBuilder.Entity<BookLoan>().HasKey(l => l.Id);
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<WishlistItem>().HasKey(w => w.Id);
            modelBuilder.Entity<SearchHistory>().HasKey(s => s.Id);
        }
    }
}
