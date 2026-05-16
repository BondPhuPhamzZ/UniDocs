using Microsoft.EntityFrameworkCore;
using UniDocs.Models;

namespace UniDocs.Data
{
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


        // Hàm khắc phục lỗi vòng lặp xóa (bảng Report) vì có quá nhiều đường dẫn đến cùng 1 hành động -> Sử dụng cấu hình Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Xóa User thì không tự động xóa Report mà User đó đã báo cáo
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany()
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict ngăn chặn xóa tự động dây chuyền -> Tắt bớt 1 đường xóa dây chuyền để giải tỏa vòng lặp
        }

    }
}
