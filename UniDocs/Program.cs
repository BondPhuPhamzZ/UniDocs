using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using UniDocs.Data;

namespace UniDocs
{
    // Đăng ký DbContext vào hệ thống
    // ===== Thêm kết nối Database =====
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();


            // ===== -> Cho phép toàn bộ Web sử dụng được AppDbContext =====
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles(); // Phải được kích hoạt để có thể đọc được style.css và main.js trong thư mục wwwroot

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
