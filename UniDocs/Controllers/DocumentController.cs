using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting; 
using Microsoft.AspNetCore.Authorization;
using UniDocs.Data;
using UniDocs.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace UniDocs.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; // Trỏ tới thư mục wwwroot (xử lý upload và get)
        private readonly IConfiguration _configuration; // Tích hợp call API

        public DocumentController(AppDbContext context, IWebHostEnvironment env, IConfiguration configuration)
        {
            _context = context;
            _env = env;
            _configuration = configuration;
        }


        // ===== Lưu tài liệu =====
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SaveDocument(int documentId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Id User đang đăng nhập

            if (string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdString);

            // Check lưu chưa
            var existingSave = await _context.SavedDocuments
                .FirstOrDefaultAsync(s => s.UserId == userId && s.DocumentId == documentId);

            if (existingSave != null)
            {
                _context.SavedDocuments.Remove(existingSave);
                TempData["SuccessMessage"] = "Đã bỏ lưu tài liệu khỏi danh sách yêu thích!";
            }
            else
            {
                var newSave = new SavedDocument()
                {
                    UserId = userId,
                    DocumentId = documentId,
                    SavedDate = DateTime.Now
                };
                _context.SavedDocuments.Add(newSave);
                TempData["SuccessMessage"] = "Đã lưu tài liệu vào danh sách yêu thích thành công! ";
            }
            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (referer.Contains("Profile"))
                return RedirectToAction("Profile", "Account");
            return RedirectToAction("Detail", new { id = documentId });

        }


        // ===== DOWNLOAD =====
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

            // Khi tải -> Dựa theo đường dẫn này để lấy file ra
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


        // ===== UI UPLOAD (Get) =====
        public async Task<IActionResult> Upload()
        {
            ViewBag.Courses = await _context.Courses.ToListAsync();
            return View();
        }


        // ===== UPLOAD =====
        [HttpPost]
        public async Task<IActionResult> Upload(Document model, IFormFile uploadedFile)
        {
            // Check người dùng đã chọn file để up lên chưa
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Vui lòng chọn 1 file để tải lên!");
                ViewBag.Courses = await _context.Courses.ToListAsync();
                return View(model);
            }

            // Check đuôi file đúng định dạng
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".zip", ".rar" };
            var extension = Path.GetExtension(uploadedFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(string.Empty, "Chỉ cho phép tải lên file PDF, Word, PowerPoint hoặc Zip/ Rar!");
                ViewBag.Courses = await _context.Courses.ToListAsync();
                return View(model);
            }

            string uniqueFileName = DateTime.Now.Ticks.ToString() + "_" + uploadedFile.FileName;

            // Thư mục lưu những tài liệu được User upload lên
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

            // Cập nhật các thông tin còn thiếu cho Object Document
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

            TempData["SuccessMessage"] = "Tải tài liệu lên thành công!";
            return RedirectToAction("Profile", "Account");

        }


        // Trang chi tiết -> khi bấm vào tài liệu
        [AllowAnonymous]
        public async Task<IActionResult> Detail(int id)
        {
            var document = await _context.Documents
                .Include(d => d.User)
                .Include(d => d.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound("Không tìm thấy tài liệu này!");
            }

            return View(document);

        }

        // ===== Báo cáo vi phạm =====
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Report(int documentId, string reason)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return RedirectToAction("Login", "Account");

            // Kiểm tra đã báo cáo tài liệu này chưa
            var existed = await _context.Reports
                .AnyAsync(r => r.DocumentId == documentId && r.ReporterId == userId);
            if (existed)
            {
                TempData["ErrorMessage"] = "Bạn đã báo cáo tài liệu này rồi!";
                return RedirectToAction("Detail", new { id = documentId });
            }

            var report = new Report
            {
                DocumentId = documentId,
                ReporterId = userId,
                Reason = reason,
                ReportDate = DateTime.Now,
                Status = "Đang xử lý"
            };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã gửi báo cáo thành công! Admin sẽ xem xét sớm.";
            return RedirectToAction("Detail", new { id = documentId });
        }


        // ===== AI Summary (Tích hợp AI) =====
        [AllowAnonymous]
        public async Task<IActionResult> AiSummary(int id)
        {
            var doc = await _context.Documents
                .Include(d => d.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null)
                return Json(new { summary = "Không tìm thấy tài liệu." });

            string prompt = $"Hãy tóm tắt ngắn gọn (3-5 câu) bằng tiếng Việt về tài liệu học tập sau: " +
                            $"Tên: {doc.Title}. Môn học: {doc.Course?.CourseName}. " +
                            $"Loại: {doc.DocType}. Mô tả: {doc.Description ?? "Không có mô tả"}. " +
                            $"Hãy trả lời tự nhiên như một trợ lý học tập thân thiện.";
            try
            {
                using var client = new HttpClient();

                var apiKey = _configuration["GeminiApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                    return Json(
                        new 
                        { 
                            summary = "Chưa cấu hình API Key. Vui lòng liên hệ Admin." 
                        }
                    );

                client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

                var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

                var body = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    }
                };

                var response = await client.PostAsJsonAsync(url, body);
                var rawJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return Json(
                        new 
                        { 
                            summary = "Tính năng AI tóm tắt hiện không khả dụng. Vui lòng thử lại sau." 
                        }
                    );

                var json = System.Text.Json.JsonDocument.Parse(rawJson).RootElement;
                string summary = json
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "Không thể tóm tắt.";

                return Json(new { summary });
            }
            catch (Exception)
            {
                return Json(
                    new 
                    { 
                        summary = "Tính năng AI tóm tắt hiện không khả dụng. Vui lòng thử lại sau." 
                    }
                );
            }
        }

    }
}
