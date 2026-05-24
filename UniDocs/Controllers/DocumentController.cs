using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting; // Libary to connect wwwroot path
using Microsoft.AspNetCore.Authorization;
using UniDocs.Data;
using UniDocs.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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
            // Tìm tài liệu theo ID
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound("Tài liệu không tồn tại!");
            }

            // Số lượt tải tăng lên và lưu lại
            document.DownloadCount += 1;
            await _context.SaveChangesAsync();

            // Tìm file vật lý trong ổ cứng
            string physicalPath = Path.Combine(_env.WebRootPath, document.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound("Lỗi: Không tìm thấy file trong máy chủ!");
            }

            // Trả file về cho trình duyệt kèm tên gốc
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
            // Check người dùng đã chọn file để up lên chưa
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Vui lòng chọn 1 file để tải lên!");
                ViewBag.Courses = _context.Courses.ToList();
                return View(model);
            }

            // Check đuôi file đúng định dạng
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".ppt", ".zip", ".rar" };
            var extension = Path.GetExtension(uploadedFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(string.Empty, "Chỉ cho phép tải lên file PDF, Word, PowerPoint hoặc Zip/ Rar!");
                ViewBag.Courses = _context.Courses.ToList();
                return View(model);
            }

            string uniqueFileName = DateTime.Now.Ticks.ToString() + "_" + uploadedFile.FileName;

            // Đường dẫn tới thư mục cần lưu (wwwroot/ uploads)
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

            // Copy file (Bơm file vào ổ cứng) từ trình duyệt của người dùng vào ổ cứng máy chủ
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }*/

            // Cập nhật các thông tin còn thiếu cho Object Document => Lưu vào DB
            model.FilePath = "/uploads/" + uniqueFileName; 
            model.UploadDate = DateTime.Now;
            model.DownloadCount = 0;
            model.IsApproved = true; // => Đang test

            // Lấy ID của người dùng đang đăng nhập từ Cookie
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.UserId = int.Parse(userIdStr);

            // Lưu vào DB
            _context.Documents.Add(model);
            await _context.SaveChangesAsync();

            // Tạm thời quay về trang chủ (Dự định tạo thêm 1 trang thông báo "Thành Công!"
            return RedirectToAction("Index", "Home");

        }


        // Trang chi tiết -> khi bấm vào tài liệu
        [AllowAnonymous]
        public async Task<IActionResult> Detail(int id)
        {
            var document = await _context.Documents
                .Include(d => d.User)
                .Include(d => d.Course)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound("Không tìm thấy tài liệu này!");
            }

            return View(document);

        }



    }
}
