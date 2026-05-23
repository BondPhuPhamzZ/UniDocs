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

        // Inject database vào Controller
        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /*public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }*/


        public IActionResult Index(string query)
        {
            var documentsQuery = _context.Documents
                .Include(d => d.Course)
                .Include(d => d.User)
                .Where(d => d.IsApproved == true);

            // Nếu người dùng nhập vào Thanh tìm kiếm
            if (!string.IsNullOrEmpty(query))
            {
                documentsQuery = documentsQuery.Where(d => d.Title.Contains(query) || d.Course.CourseName.Contains(query));

                ViewBag.SearchQuery = query;
            }

            // Sắp xếp mới nhất và chuyển thành List
            var documents = documentsQuery.OrderByDescending(d => d.UploadDate).ToList();
            return View(documents);

        }

        // ===== KO cần quan tâm =====
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
