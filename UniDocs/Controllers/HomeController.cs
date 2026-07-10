using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UniDocs.Models;
using Microsoft.EntityFrameworkCore;
using UniDocs.Data; // Dùng cho Include

namespace UniDocs.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context; 
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<IActionResult> Index(string query)
        {
            var documentsQuery = _context.Documents
                .Include(d => d.Course)
                .Include(d => d.User)
                .Where(d => d.Status == UniDocs.Models.Enums.DocumentStatusEnum.Approved);

            if (!string.IsNullOrEmpty(query))
            {
                documentsQuery = documentsQuery.Where(d => d.Title.Contains(query) || d.Course.CourseName.Contains(query));
                ViewBag.SearchQuery = query;
            }

            var documents = await documentsQuery
                .OrderByDescending(d => d.UploadDate)
                .Take(8)
                .ToListAsync();

            return View(documents);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error (int? statusCode = null)
        {
            ViewBag.ErrorCode = statusCode ?? 500;
            ViewBag.ErrorTitle = "System Error Occurred!";
            ViewBag.ErrorMessage = "We are fixing the issue. Please check back later!";
            ViewBag.ErrorIcon = "bi-exclamation-triangle text-danger";

            if (statusCode == 404)
            {
                ViewBag.ErrorCode = 404;
                ViewBag.ErrorTitle = "Page Not Found!";
                ViewBag.ErrorMessage = "The requested page does not exist, or the document has been deleted.";
                ViewBag.ErrorIcon = "bi-search text-warning";
            }

            return View();
        }

    }
}
