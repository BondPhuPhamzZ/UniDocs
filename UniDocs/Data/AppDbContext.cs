using Microsoft.EntityFrameworkCore;
using UniDocs.Models;

namespace UniDocs.Data
{
    // ========== DATABASE CHÍNH ==========

    // ===== Cây cầu nối -> Lấy 4 class trong thư mục Data tạo thành 4 cái bảng trong CSDL =====
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Khai báo 4 bảng Database (Tên các thuộc tính là tên bảng: VD -> Users, Courses,...)
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<SavedDocument> SavedDocuments { get; set; }



        // Hàm khắc phục lỗi vòng lặp xóa (bảng Report) vì có quá nhiều đường dẫn đến cùng 1 hành động -> Sử dụng cấu hình Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Report 
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany()
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Lưu tài liệu
            modelBuilder.Entity<SavedDocument>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

        }

    }
}
