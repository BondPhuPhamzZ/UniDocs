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


        // Tải file và lượt tải => ===== DOWNLOAD =====
        [AllowAnonymous]
        public async Task<IActionResult> Download(int id)
        {
            // 1. Tìm tài liệu theo ID
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound("Tài liệu không tồn tại!");
            }

            // 2. Số lượt tải tăng lên và lưu lại
            document.DownloadCount += 1;
            await _context.SaveChangesAsync();

            // 3. Tìm file vật lý trong ổ cứng
            string physicalPath = Path.Combine(_env.WebRootPath, document.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound("Lỗi: Không tìm thấy file trong máy chủ!");
            }

            // 4. Trả file về cho trình duyệt kèm tên gốc
            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
            string downloadName = document.Title + Path.GetExtension(physicalPath);

            return File(fileBytes, "application/octet-stream", downloadName);

        }



        // ===== Giao diện trang UPLOAD (Get) =====
        public ViewResult Upload()
        {
            ViewBag.Courses = _context.Courses.ToList();
            return View();
        }


        // Khi người dùng bấm Submit (Post) => ===== UPLOAD =====
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

            // Check thư mục uploads cos tồn tại
            if (!Directory.Exists(uploadsFolder)) 
            { 
                Directory.CreateDirectory(uploadsFolder); 
            }

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }

            /*string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 5. Copy file (Bơm file vào ổ cứng) từ trình duyệt của người dùng vào ổ cứng máy chủ
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }*/

            // 6. Cập nhật các thông tin còn thiếu cho Object Document
            model.FilePath = "/uploads/" + uniqueFileName; 
            model.UploadDate = DateTime.Now;
            model.DownloadCount = 0;
            model.IsApproved = true; // => Đang test

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
