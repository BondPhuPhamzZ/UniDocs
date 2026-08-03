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

        // Index page
        public async Task<IActionResult> Index(string query, int page = 1)
        {
            int pageSize = 6;

            var courseQuery = _context.Courses.Include(c => c.Documents.Where(d => d.Status == Models.Enums.DocumentStatusEnum.Approved)).AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                courseQuery = courseQuery.Where(c => c.CourseName.Contains(query));
                ViewBag.SearchQuery = query;
            }

            int totalCourses = await courseQuery.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCourses / pageSize);
            ViewBag.CurrentPage = page;

            var courses = await courseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(courses);

        }

        // Detail page
        public async Task<IActionResult> Detail(int id, string docQuery = null, int page = 1)
        {
            int pageSize = 6; 

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null)
            {
                return NotFound("Courses cannor found!");
            }

            ViewBag.CourseName = course.CourseName;
            ViewBag.Department = course.Department;
            ViewBag.CourseId = course.Id;
            ViewBag.DocQuery = docQuery;

            var docsQuery = _context.Documents
                .Include(d => d.User)
                .Include(d => d.Course) 
                .Where(d => d.CourseId == id && d.Status == Models.Enums.DocumentStatusEnum.Approved);

            if (!string.IsNullOrEmpty(docQuery))
            {
                docsQuery = docsQuery.Where(d => d.Title.Contains(docQuery));
            }

            int totalDocs = await docsQuery.CountAsync();
            ViewBag.TotalDocuments = totalDocs;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalDocs / pageSize);
            ViewBag.CurrentPage = page;

            var documents = await docsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(documents);

        }

        // Searching
        public async Task<IActionResult> SearchSidebar(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return RedirectToAction("Index");

            var matchedCourse = await _context.Courses
                .Where(c => c.CourseName.Contains(query))
                .FirstOrDefaultAsync();

            if (matchedCourse != null)
            {
                return RedirectToAction("Detail", new { id = matchedCourse.Id });
            }

            var matchedDoc = await _context.Documents
                .Include(d => d.Course)
                .Where(d => d.Title.Contains(query) && d.Status == Models.Enums.DocumentStatusEnum.Approved)
                .FirstOrDefaultAsync();

            if (matchedDoc != null && matchedDoc.Course != null)
            {
                return RedirectToAction("Detail", new { id = matchedDoc.Course.Id, docQuery = query });
            }

            TempData["ErrorMessage"] = "No matching results found!";
            return RedirectToAction("Index");
        }


    }
}
