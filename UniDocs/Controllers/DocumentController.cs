using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting; // Libary to connect wwwroot path
using Microsoft.AspNetCore.Authorization;
using UniDocs.Data;
using UniDocs.Models;
using System.Security.Claims;

namespace UniDocs.Controllers
{
    // Must login to join in this Controller
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; // Biến này giúp tìm ra thư mục root

        public DocumentController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }


        // ===== Giao diện trang Upload (Get) =====
        public IActionResult Upload()
        {
            ViewBag.Courses = _context.Courses.ToList();
            return View();
        }


        // Khi người dùng bấm Submit (Post)
        [HttpPost]
        public async Task<IActionResult> Upload(Document model, IFormFile uploadedFile)
        {
            // 1. Kiểm tra xem người dùng chọn File chưa
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                ViewBag.Error = "Vui lòng chọn 1 file để tải lên!";
                ViewBag.Courses = _context.Courses.ToList();
                return View(model);
            }

            // 2. Kiểm tra đuôi file (chỉ cho phép 1 số định dạng)
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".ppt", ".zip", ".rar" };
            var extension = Path.GetExtension(uploadedFile.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                ViewBag.Error = "Chỉ cho phép tải lên file PDF, Word, PowerPoint hoặc Zip/ Rar!";
                ViewBag.Courses = _context.Courses.ToList();
                return View(model);
            }

            // 3. Tạo tên file mới để tránh bị trùng lặp (vd: 123456_dethi.pdf)
            string uniqueFileName = DateTime.Now.Ticks.ToString() + "_" + uploadedFile.FileName;
            // 4. Tìm đường dẫn tới thư mục wwwroot/ uploads
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 5. Copy file (Bơm file vào ổ cứng) từ trình duyệt của người dùng vào ổ cứng máy chủ
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }

            // 6. Cập nhật các thông tin còn thiếu cho Object Document
            model.FilePath = "/uploads/" + uniqueFileName; // Lưu đường dẫn tương đối vào DB
            model.UploadDate = DateTime.Now;
            model.DownloadCount = 0;
            model.IsApproved = false; // Mặc định là chờ duyệt -> hệ thống Auto-Moderation AI sẽ can thiệp sau 

            // Lấy ID của người dùng đang đăng nhập từ Cookie
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.UserId = int.Parse(userIdStr);

            // 7. Lưu vào DB
            _context.Documents.Add(model);
            await _context.SaveChangesAsync();

            // Tạm thời quay về trang chủ (sau này có thể tạo thêm 1 trang thông báo "Thành Công!"
            return RedirectToAction("Index", "Home");

        }

    }
}
