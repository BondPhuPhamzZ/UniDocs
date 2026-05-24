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

            // Add services to the container.
            //builder.Services.AddControllersWithViews();


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

            // Configure the HTTP request pipeline. -> Catch 500
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            // -> Catch 404, 404,...
            app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");


            app.UseHttpsRedirection();

            app.UseStaticFiles(); // Phải được kích hoạt để có thể đọc được style.css và main.js trong thư mục wwwroot

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
