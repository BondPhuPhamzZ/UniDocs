using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting; // Libary to connect wwwroot path
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
        private readonly IWebHostEnvironment _env; // Biến này giúp tìm ra thư mục root
        private readonly IConfiguration _configuration; // AI

        public DocumentController(AppDbContext context, IWebHostEnvironment env, IConfiguration configuration)
        {
            _context = context;
            _env = env;
            _configuration = configuration;
        }


        // ===== Lưu tài liệu ===== -> Login mới lưu đc
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
                // Nếu đã lưu -> Bấm lần nữa -> Hủy lưu (xóa khỏi DB)
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
        public async Task<IActionResult> Upload()
        {
            ViewBag.Courses = await _context.Courses.ToListAsync();
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
                ViewBag.Courses = await _context.Courses.ToListAsync();
                return View(model);
            }

            // Check đuôi file đúng định dạng
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".ppt", ".zip", ".rar" };
            var extension = Path.GetExtension(uploadedFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(string.Empty, "Chỉ cho phép tải lên file PDF, Word, PowerPoint hoặc Zip/ Rar!");
                ViewBag.Courses = await _context.Courses.ToListAsync();
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
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null)
                return Json(new { summary = "Không tìm thấy tài liệu." });

            // Tạo prompt gửi lên Gemini
            string prompt = $"Hãy tóm tắt ngắn gọn (3-5 câu) bằng tiếng Việt về tài liệu học tập sau: " +
                            $"Tên tài liệu: {doc.Title}. " +
                            $"Môn học: {doc.Course?.CourseName}. " +
                            $"Loại tài liệu: {doc.DocType}. " +
                            $"Mô tả: {doc.Description ?? "Không có mô tả"}. " +
                            $"Hãy trả lời tự nhiên như một trợ lý học tập thân thiện.";
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10); // Timeout nhanh để fallback kịp thời
                var apiKey = _configuration["GeminiApiKey"];

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";

                var body = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var response = await client.PostAsJsonAsync(url, body);
                var rawJson = await response.Content.ReadAsStringAsync();

                // Nếu API thành công → trả về kết quả thật
                if (response.IsSuccessStatusCode)
                {
                    var json = System.Text.Json.JsonDocument.Parse(rawJson).RootElement;
                    string aiResult = json
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString() ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(aiResult))
                        return Json(new { summary = aiResult });
                }

                // Nếu API lỗi → dùng tóm tắt thông minh từ dữ liệu tài liệu
                return Json(new { summary = GenerateLocalSummary(doc) });
            }
            catch (Exception)
            {
                // Timeout hoặc lỗi mạng → fallback ngay
                return Json(new { summary = GenerateLocalSummary(doc) });
            }
        }

        /// <summary>
        /// Tạo tóm tắt thông minh từ thông tin tài liệu khi Gemini API không khả dụng.
        /// </summary>
        private string GenerateLocalSummary(Models.Document doc)
        {
            var courseName = doc.Course?.CourseName ?? "chưa phân loại";
            var docType = doc.DocType ?? "tài liệu";
            var title = doc.Title ?? "Không có tiêu đề";
            var description = doc.Description;
            var year = doc.AcademicYear ?? "";
            var uploader = doc.UploadedByName ?? "người dùng";

            // Dòng mở đầu
            var intro = $"📄 \"{title}\" là {docType.ToLower()} thuộc môn học **{courseName}**";
            if (!string.IsNullOrEmpty(year))
                intro += $", được biên soạn cho năm học {year}";
            intro += ".";

            // Dòng mô tả nếu có
            var descLine = !string.IsNullOrWhiteSpace(description)
                ? $" Tài liệu này {description.TrimEnd('.')}."
                : $" Đây là tài liệu học tập hữu ích dành cho sinh viên theo học môn {courseName}.";

            // Dòng gợi ý sử dụng theo loại tài liệu
            var usageLine = docType.ToLower() switch
            {
                "slide" or "bài giảng" => " Sinh viên có thể dùng để ôn tập theo từng buổi học và nắm bắt các kiến thức trọng tâm.",
                "đề thi" or "đề kiểm tra" => " Đây là tài liệu luyện tập rất tốt, giúp sinh viên làm quen với cấu trúc đề và cách phân bổ thời gian khi thi.",
                "bài tập" or "bài lab" => " Tài liệu cung cấp các bài tập thực hành giúp củng cố lý thuyết và rèn luyện kỹ năng giải quyết vấn đề.",
                "báo cáo" or "luận văn" => " Tài liệu có thể là nguồn tham khảo quý giá cho các nghiên cứu và báo cáo liên quan.",
                _ => " Đây là nguồn tài liệu tham khảo chất lượng, phù hợp để bổ sung kiến thức và hỗ trợ việc học tập."
            };

            // Dòng kết
            var outro = $" Tài liệu được chia sẻ bởi {uploader} trên hệ thống UniDocs.";

            return intro + descLine + usageLine + outro;
        }

    }
}
