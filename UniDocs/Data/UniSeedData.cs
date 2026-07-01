using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UniDocs.Models;

namespace UniDocs.Data
{
    public class UniSeedData
    {
        public static async Task SeedAsync(AppDbContext context, IServiceProvider serviceProvider)
        {
            var securityService = serviceProvider.GetRequiredService<UniDocs.Services.SecurityService>();

            context.Database.EnsureCreated();

            // TK Admin 
            if (!await context.Users.AnyAsync(u => u.Email == "adminunidocs@gmail.com"))
            {
                var adminUser = new User()
                {
                    FirstName = "Admin",
                    LastName = "System",
                    Email = "adminunidocs@gmail.com",
                    PasswordHash = securityService.HashPassword("admin123"), 
                    Role = UniDocs.Models.Enums.RoleEnum.Admin,
                    IsActive = true
                };
                await context.Users.AddAsync(adminUser);
            }

            // Course
            if (!await context.Courses.AnyAsync(c => c.CourseCode == "IT04"))
            {
                await context.Courses.AddRangeAsync(
                    new Course { CourseCode = "IT04", CourseName = "Lập trình Web nâng cao", Department = "CNTT"},
                    new Course { CourseCode = "IT05", CourseName = "Công nghệ phần mềm", Department = "CNTT" },
                    new Course { CourseCode = "IT06", CourseName = "Mạng máy tính", Department = "CNTT" }
                );
            }

            await context.SaveChangesAsync();

        }
    }
}
