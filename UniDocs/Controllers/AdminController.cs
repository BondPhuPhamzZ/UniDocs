using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using UniDocs.Data;
using UniDocs.Models;

namespace UniDocs.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ============ Index của Admin -> Các tài liệu bị báo cáo ==========
        public async Task<IActionResult> Dashboard()
        {
            var reports = await _context.Reports
                .Include(r => r.Document)
                .Include(r => r.Reporter)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
            return View(reports);
        }
        // Xóa tài liệu vi phạm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound();

            // Delete doc in o cung
            string physicalPath = Path.Combine(_env.WebRootPath, document.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);

            // Delete doc in db
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa tài liệu vi phạm thành công!";

            return RedirectToAction("Dashboard");

        }
        // Bỏ qua báo cáo (ko có vi phạm) -> đánh dấu "đã xử lý"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DismissReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);

            if (report == null)
                return NotFound();

            report.Status = "Đã xử lý";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã bỏ qua báo cáo!";
            return RedirectToAction("Dashboard");
        }


        // ========== Quản lý người dùng ==========
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.Documents)
                .OrderBy(u => u.LastName)
                .ToListAsync();

            return View(users);
        }
        // Mở/ khóa tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            string msg = user.IsActive ? "Đã mở khóa tài khoản!" : "Đã khóa tài khoản!";
            TempData["SuccessMessage"] = msg;

            return RedirectToAction("Users");
        }


        // ========== Tab Quản lý môn học ==========
        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses
                .Include(c => c.Documents)
                .ToListAsync();
            return View(courses);
        }
        // Thêm môn học mới 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(Course model)
        {
            if (ModelState.IsValid)
            {
                _context.Courses.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thêm môn học \"{model.CourseName}\" thành công!";
            }
            return RedirectToAction("Courses");
        }
        // Xóa môn học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) 
                return NotFound();

            if (course.Documents.Count > 0)
            {
                TempData["ErrorMessage"] = "Không thể xóa môn học đang có tài liệu!";
                return RedirectToAction("Courses");
            }
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa môn học \"{course.CourseName}\"!";
            return RedirectToAction("Courses");
        }
    }
}
