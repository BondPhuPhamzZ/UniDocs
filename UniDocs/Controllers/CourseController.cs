using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using UniDocs.Data;

namespace UniDocs.Controllers
{
    // ===== Quản lý môn học =====
    public class CourseController : Controller
    {
        private readonly AppDbContext _context;

        public CourseController(AppDbContext context)
        {
            _context = context;
        }

        // Trang danh sách các môn học
        public IActionResult Index()
        {
            var courses = _context.Courses.Include(c => c.Documents).ToList();
            return View(courses);
        }


        // Trang chi tiết tài liệu của 1 môn (major.html)
        public IActionResult Detail(int id)
        {
            var course = _context.Courses
                .Include(c => c.Documents.Where(d => d.IsApproved == true))
                .ThenInclude(d => d.User)
                .FirstOrDefault(c => c.Id == id);

            if (course == null)
            {
                return NotFound("Không tìm thấy môn học!");
            }

            return View(course);
        }


    }
}
