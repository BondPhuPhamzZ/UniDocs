using Microsoft.EntityFrameworkCore;
using UniDocs.Models;

namespace UniDocs.Data
{
    public class UniSeedData
    {
        public static async Task Seed(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // TK Admin 
            if (!await context.Users.AnyAsync(u => u.Email == "adminunidocs@gmail.com"))
            {
                var adminUser = new User()
                {
                    FirstName = "Admin",
                    LastName = "System",
                    Email = "adminunidocs@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), 
                    Role = "Admin",
                    IsActive = true
                };
                await context.Users.AddAsync(adminUser);
            }

            // Course
            if (!await context.Courses.AnyAsync(c => c.CourseCode == "IT04"))
            {
                await context.Courses.AddRangeAsync(
                    new Course { CourseCode = "IT04", CourseName = "Lập trình Web nâng cao", Department = "CNTT"}
                );
            }

            await context.SaveChangesAsync();

        }
    }
}
