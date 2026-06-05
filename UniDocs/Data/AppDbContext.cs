using Microsoft.EntityFrameworkCore;
using UniDocs.Models;

namespace UniDocs.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<SavedDocument> SavedDocuments { get; set; }


        // Xử lý lỗi Handle (1 hành động mang nhiều ý nghĩa) -> Sử dụng cấu hình Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Report 
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany()
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            // SavedDocument
            modelBuilder.Entity<SavedDocument>()
                .HasOne(s => s.User)
                .WithMany(u => u.SavedDocuments)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

        }

    }
}
