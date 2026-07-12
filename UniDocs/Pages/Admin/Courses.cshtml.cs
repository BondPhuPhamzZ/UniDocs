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
        public Course CourseInput { get; set; } = new Course();

        public List<string> Departments { get; set; } = new List<string>();


        // Phân trang
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public const int PageSize = 6;


        public async Task OnGetAsync(int p = 1)
        {
            CurrentPage = p;
            var query = _context.Courses.AsQueryable();
            int totalCourses = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCourses / (double)PageSize);

            CourseList = await query
                .Include(c => c.Documents)
                .OrderByDescending(c => c.Id)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            Departments = await _context.Courses
                .Select(c => c.Department)
                .Where(d => !string.IsNullOrEmpty(d))
                .Distinct()
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddCourseAsync()
        {
            ModelState.Remove("CourseInput.Id");
            ModelState.Remove("CourseInput.Documents");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid course information!";
                return RedirectToPage("./Courses");
            }

            if (await _context.Courses.AnyAsync(c => c.CourseCode == CourseInput.CourseCode))
            {
                TempData["ErrorMessage"] = $"Course code \"{CourseInput.CourseCode}\" already exists!";
                return RedirectToPage("./Courses");
            }

            _context.Courses.Add(CourseInput);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Course \"{CourseInput.CourseName}\" added successfully!";
            
            return RedirectToPage("./Courses");
        }

        public async Task<IActionResult> OnPostEditCourseAsync(int id)
        {
            ModelState.Remove("CourseInput.Id");
            ModelState.Remove("CourseInput.Documents");

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid course information!";
                return RedirectToPage("./Courses");
            }

            if (await _context.Courses.AnyAsync(c => c.CourseCode == CourseInput.CourseCode && c.Id != id))
            {
                TempData["ErrorMessage"] = $"Course code \"{CourseInput.CourseCode}\" already exists in another course!";
                return RedirectToPage("./Courses");
            }

            course.CourseName = CourseInput.CourseName;
            course.CourseCode = CourseInput.CourseCode;
            course.Department = CourseInput.Department;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Course \"{course.CourseName}\" updated successfully!";
            
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
                TempData["ErrorMessage"] = "Cannot delete a course that has documents!";
                return RedirectToPage("./Courses");
            }
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Course \"{course.CourseName}\" deleted!";
            return RedirectToPage("./Courses");
        }
    }
}
