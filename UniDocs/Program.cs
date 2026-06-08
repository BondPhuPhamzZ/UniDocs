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

            // ===== Khai báo Database =====
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


            // ===== Seed Data =====
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    UniSeedData.Seed(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Đã xảy ra lỗi khi Seed Data!");
                }
            }


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
            app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");


            // Phải được kích hoạt để có thể đọc được style.css và main.js trong thư mục wwwroot
            app.UseStaticFiles(); 


            app.UseRouting();

            // Kích hoạt xác thực và phân quyền
            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
