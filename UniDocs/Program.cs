using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using UniDocs.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace UniDocs
{
    // Đăng ký DbContext vào hệ thống
    // ===== Thêm kết nối Database =====
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===== -> Khai báo Database =====
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            // ===== Khai báo Cookie Authentication (cấu hình mới) =====
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login"; // Nếu chưa login mà đòi vào trang cấm, sẽ bị đuổi về đây
                    options.AccessDeniedPath = "/Account/Login";
                });

            builder.Services.AddControllersWithViews();


            var app = builder.Build();

           // ===== Cấu hình Middleware =====
            if (!app.Environment.IsDevelopment())
            {
                // Catch 500
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }
            // Status code != truyền vào
            app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");


            // Phải được kích hoạt để có thể đọc được style.css và main.js trong thư mục wwwroot
            app.UseStaticFiles(); 


            app.UseRouting();

            // Kích hoạt xác thực và phân quyền
            app.UseAuthentication();

            app.UseAuthorization();

            // Default khi chạy Web
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
