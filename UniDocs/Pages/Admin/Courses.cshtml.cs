using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniDocs.Data;
using UniDocs.Models;

namespace UniDocs.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CoursesModel : PageModel
    {
        private readonly AppDbContext _context;

        public CoursesModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Course> CourseList { get; set; } = new List<Course>();

        [BindProperty]
        public Course NewCourse { get; set; } = new Course();

        public async Task OnGetAsync()
        {
            CourseList = await _context.Courses
                .Include(c => c.Documents)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddCourseAsync()
        {
            if (!string.IsNullOrEmpty(NewCourse.CourseName) && !string.IsNullOrEmpty(NewCourse.CourseCode))
            {
                _context.Courses.Add(NewCourse);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thêm môn học \"{NewCourse.CourseName}\" thành công!";
            }
            return RedirectToPage("./Courses");
        }

        public async Task<IActionResult> OnPostEditCourseAsync(int id, string courseName, string courseCode, string department)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            if (!string.IsNullOrEmpty(courseName) && !string.IsNullOrEmpty(courseCode))
            {
                course.CourseName = courseName;
                course.CourseCode = courseCode;
                course.Department = department;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật môn học \"{courseName}\" thành công!";
            }
            return RedirectToPage("./Courses");
        }

        public async Task<IActionResult> OnPostDeleteCourseAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) 
                return NotFound();

            if (course.Documents.Count > 0)
            {
                TempData["ErrorMessage"] = "Không thể xóa môn học đang có tài liệu!";
                return RedirectToPage("./Courses");
            }
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa môn học \"{course.CourseName}\"!";
            return RedirectToPage("./Courses");
        }
    }
}
