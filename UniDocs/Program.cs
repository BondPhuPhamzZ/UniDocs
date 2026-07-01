using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using UniDocs.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;

namespace UniDocs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===== Dki Database =====
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            // ===== Dki Cookie Authentication =====
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login"; // Nếu chưa login mà đòi vào trang cấm, sẽ bị đuổi về đây
                    options.AccessDeniedPath = "/Account/Login";
                });

            builder.Services.AddControllersWithViews();


            // Dki Cloudinary
            builder.Services.AddScoped<UniDocs.Services.ICloudinaryService, UniDocs.Services.CloudinaryService>();

            // Dki Security Service
            builder.Services.AddSingleton<UniDocs.Services.SecurityService>();


            builder.Services.AddRazorPages();


            var app = builder.Build();


            // ===== Seed Data =====
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    await UniSeedData.SeedAsync(context, services);
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
            app.MapRazorPages();

            app.Run();
        }
    }
}
