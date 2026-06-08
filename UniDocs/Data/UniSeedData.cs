using Microsoft.EntityFrameworkCore;
using UniDocs.Models;

namespace UniDocs.Data
{
    public class UniSeedData
    {
        public static void Seed(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // TK Admin 
            if (!context.Users.Any(u => u.Email == "adminunidocs@gmail.com"))
            {
                var adminUser = new User()
                {
                    FirstName = "Admin",
                    LastName = "System",
                    Email = "adminunidocs@gmail.com",
                    PasswordHash = "admin123", 
                    Role = "Admin",
                    IsActive = true
                };
                context.Users.Add(adminUser);
            }

            // Course
            if (!context.Courses.Any(c => c.CourseCode == "IT04"))
            {
                context.Courses.AddRange(
                    new Course { CourseCode = "IT04", CourseName = "Lập trình Web nâng cao", Department = "CNTT"}
                );
            }

            context.SaveChanges();

        }
    }
}
