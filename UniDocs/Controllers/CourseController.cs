using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Net.WebSockets;
using UniDocs.Data;

namespace UniDocs.Controllers
{
    public class CourseController : Controller
    {
        private readonly AppDbContext _context;

        public CourseController(AppDbContext context)
        {
            _context = context;
        }


        // Danh sách các môn học
        public async Task<IActionResult> Index(string query, int page = 1)
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
            int totalCourses = await courseQuery.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCourses / pageSize);
            ViewBag.CurrentPage = page;

            var courses = await courseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(courses);

        }


        // Trang chi tiết tài liệu của 1 môn 
        public async Task<IActionResult> Detail(int id, int page = 1)
        {
            int pageSize = 5; 

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null)
            {
                return NotFound("Không tìm thấy môn học!");
            }

            // Lưu thông tin môn học vào Viewbag
            ViewBag.CourseName = course.CourseName;
            ViewBag.Department = course.Department;
            ViewBag.CourseId = course.Id;

            // Tính toán tổng số trang
            int totalDocs = await _context.Documents.CountAsync(d => d.CourseId == id && d.IsApproved == true);
            ViewBag.TotalDocuments = totalDocs;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalDocs / pageSize);
            ViewBag.CurrentPage = page;

            // Phân trang
            var documents = await _context.Documents
                .Include(d => d.User)
                .Include(d => d.Course) 
                .Where(d => d.CourseId == id && d.IsApproved == true)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(documents);

        }


    }
}
