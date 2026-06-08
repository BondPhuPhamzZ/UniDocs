using Microsoft.EntityFrameworkCore;
using UniDocs.Models;

namespace UniDocs.Data
{
    public class UniSeedData
    {
        public static void Seed(AppDbContext context)
        {
            if (context.Users.Any())
            {
                return;
            }

            var adminUser = new User()
            {
                FirstName = "Admin",
                LastName = "System",
                Email = "adminunidocs@gmail.com",
                PasswordHash = "admin123",
                Role = "Admin",
                IsActive = true
            };

            var defaultCourse = new Course()
            {
                CourseCode = "IT04",
                CourseName = "Công nghệ phần mềm",
                Department = "CNTT"
            };

            context.Users.Add(adminUser);
            context.Courses.Add(defaultCourse);

            context.SaveChanges();
        }
    }
}
