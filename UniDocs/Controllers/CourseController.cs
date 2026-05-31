using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
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
        public IActionResult Index(string query, int page = 1)
        {
            int pageSize = 6;

            var courseQuery = _context.Courses.Include(c => c.Documents.Where(d => d.IsApproved == true)).AsQueryable();

            // Nếu User gõ vào thanh tìm kiếm
            if (!string.IsNullOrEmpty(query))
            {
                courseQuery = courseQuery.Where(c => c.CourseName.Contains(query));
                ViewBag.SearchQuery = query;
            }

            // Phân trang -> Dựa trên courseQuery đã dc lọc
            int totalCourses = courseQuery.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCourses / pageSize);
            ViewBag.CurrentPage = page;

            var courses = courseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return View(courses);

        }


        // Trang chi tiết tài liệu của 1 môn (major.html)
        public IActionResult Detail(int id, int page = 1)
        {
            int pageSize = 5; 

            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound("Không tìm thấy môn học!");
            }

            // Lưu thông tin môn học vào Viewbag
            ViewBag.CourseName = course.CourseName;
            ViewBag.Department = course.Department;
            ViewBag.CourseId = course.Id;

            // Tính toán tổng số trang
            int totalDocs = _context.Documents.Count(d => d.CourseId == id && d.IsApproved == true);
            ViewBag.TotalDocuments = totalDocs;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalDocs / pageSize);
            ViewBag.CurrentPage = page;

            // Phân trang
            var documents = _context.Documents
                .Include(d => d.User)
                .Include(d => d.Course) // Partial-View in ra tên môn
                .Where(d => d.CourseId == id && d.IsApproved == true)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return View(documents);

        }


    }
}
