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
                .Where(d => d.IsApproved == true);

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

        // UI xử lý lỗi ko mong muốn
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error (int? statusCode = null)
        {
            ViewBag.ErrorCode = statusCode ?? 500;
            ViewBag.ErrorTitle = "Đã xảy ra lỗi hệ thống!";
            ViewBag.ErrorMessage = "Chúng tôi đang khác phục sự cố. Vui lòng quay lại sau!";
            ViewBag.ErrorIcon = "bi-exclamation-triangle text-danger";

            if (statusCode == 404)
            {
                ViewBag.ErrorCode = 404;
                ViewBag.ErrorTitle = "Không tìm thấy trang!";
                ViewBag.ErrorMessage = "Đường dẫn bạn nhập không tồn tại, hoặc tài liệu đã bị xóa khỏi hệ thống.";
                ViewBag.ErrorIcon = "bi-search text-warning";
            }

            return View();
        }

    }
}
