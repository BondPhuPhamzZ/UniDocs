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
        private readonly IWebHostEnvironment _env; 
        private readonly IConfiguration _configuration;

        public DocumentController(AppDbContext context, IWebHostEnvironment env, IConfiguration configuration)
        {
            _context = context;
            _env = env;
            _configuration = configuration;
        }

        // Save document
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDocument(int documentId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); 

            if (string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdString);

            var existingSave = await _context.SavedDocuments
                .FirstOrDefaultAsync(s => s.UserId == userId && s.DocumentId == documentId);

            if (existingSave != null)
            {
                _context.SavedDocuments.Remove(existingSave);
                TempData["SuccessMessage"] = "Removed document from favorites!";
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
                TempData["SuccessMessage"] = "Document added to favorites successfully!";
            }
            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (referer.Contains("Profile"))
                return RedirectToAction("Profile", "Account");
            return RedirectToAction("Detail", new { id = documentId });

        }

        // Download
        public async Task<IActionResult> Download(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound("Document does not exists");
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) 
                return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdStr);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            bool isVip = user.VipExpirationDate.HasValue && user.VipExpirationDate.Value > DateTime.Now;

            if (isVip)
            {
                if (!user.LastDownloadDate.HasValue || user.LastDownloadDate.Value.Date != DateTime.Now.Date)
                {
                    user.DailyDownloadCount = 0; 
                }
            }

            // check 
            bool canDownload = false;
            if (isVip && user.DailyDownloadCount < 10)
            {
                user.DailyDownloadCount += 1;
                user.LastDownloadDate = DateTime.Now;
                canDownload = true;
            }
            else if (user.Credits > 0)
            {
                user.Credits -= 1;
                canDownload = true;
            }

            if (!canDownload)
            {
                return RedirectToAction("Detail", new { id = document.Id }); 
            }

            string physicalPath = Path.Combine(_env.WebRootPath, document.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound("Error: File not found on the server!");
            }

            document.DownloadCount += 1;
            await _context.SaveChangesAsync();

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
            string downloadName = document.Title + Path.GetExtension(physicalPath);

            return File(fileBytes, "application/octet-stream", downloadName);

        }

        // Upload page
        public async Task<IActionResult> Upload()
        {
            ViewBag.Courses = await _context.Courses.ToListAsync();
            return View();
        }
        // Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(Document model, IFormFile uploadedFile)
        {
            ModelState.Remove("FilePath");
            ModelState.Remove("User");
            ModelState.Remove("Course");
            ModelState.Remove("Description");
            ModelState.Remove("AcademicYear");

            if (!ModelState.IsValid)
            {
                ViewBag.Courses = await _context.Courses.ToListAsync();
                return View(model);
            }

            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a file to upload!");
                ViewBag.Courses = await _context.Courses.ToListAsync();
                return View(model);
            }

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx" };
            var extension = Path.GetExtension(uploadedFile.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(string.Empty, "Only PDF, Word, PowerPoint, Excel files are allowed!");
                ViewBag.Courses = await _context.Courses.ToListAsync();
                return View(model);
            }

            var uniqueId = DateTime.Now.Ticks.ToString();
            var fileName = $"{uniqueId}_{uploadedFile.FileName}";
            var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);

            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }

            model.FilePath = "/uploads/" + fileName;
            model.UploadDate = DateTime.Now;
            model.DownloadCount = 0;
            model.Status = Models.Enums.DocumentStatusEnum.Approved;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.UserId = int.Parse(userIdStr);

            _context.Documents.Add(model);
            await _context.SaveChangesAsync();

            var currentUser = await _context.Users.FindAsync(model.UserId);
            if (currentUser != null)
            {
                currentUser.Credits += 2;
                await _context.SaveChangesAsync();
            }
            TempData["SuccessMessage"] = "Document uploaded successfully! You earned 2 Credits.";
            return RedirectToAction("Profile", "Account");

        }

        // Detail page
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
                return NotFound("This document could not be found!");
            }

            if (document.Status != Models.Enums.DocumentStatusEnum.Approved)
            {
                bool isAdmin = User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Admin");
                bool isOwner = User.Identity != null && User.Identity.IsAuthenticated && document.UserId.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (!isAdmin && !isOwner)
                {
                    return NotFound("This document does not exist or has been removed.");
                }
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdStr))
            {
                var currentUser = await _context.Users.FindAsync(int.Parse(userIdStr));
                ViewBag.UserCredits = currentUser?.Credits ?? 0;
                
                bool isVip = currentUser.VipExpirationDate.HasValue && currentUser.VipExpirationDate.Value > DateTime.Now;
                bool hasVipDownloadsLeft = isVip && (!currentUser.LastDownloadDate.HasValue || currentUser.LastDownloadDate.Value.Date != DateTime.Now.Date || currentUser.DailyDownloadCount < 10);
                
                ViewBag.CanDownload = (ViewBag.UserCredits > 0) || hasVipDownloadsLeft;
            }
            else
            {
                ViewBag.UserCredits = 0; 
                ViewBag.CanDownload = false;
            }

            return View(document);
        }

        // Report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(ViewModels.ReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Reason is required!";
                return RedirectToAction("Detail", new { id = model.DocumentId });
            }

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return RedirectToAction("Login", "Account");

            var existed = await _context.Reports
                .AnyAsync(r => r.DocumentId == model.DocumentId && r.ReporterId == userId);
            if (existed)
            {
                TempData["ErrorMessage"] = "You have already reported this document!";
                return RedirectToAction("Detail", new { id = model.DocumentId });
            }

            var report = new Report
            {
                DocumentId = model.DocumentId,
                ReporterId = userId,
                Reason = model.Reason,
                ReportDate = DateTime.Now,
                Status = Models.Enums.ReportStatusEnum.Pending
            };
            _context.Reports.Add(report);

            var reportCount = await _context.Reports.CountAsync(r => r.DocumentId == model.DocumentId && r.Status == Models.Enums.ReportStatusEnum.Pending) + 1;
            if (reportCount >= 3)
            {
                var docToHide = await _context.Documents.FindAsync(model.DocumentId);
                if (docToHide != null && docToHide.Status == Models.Enums.DocumentStatusEnum.Approved)
                {
                    docToHide.Status = Models.Enums.DocumentStatusEnum.Pending;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Report submitted successfully! Admin will review it soon.";
            return RedirectToAction("Detail", new { id = model.DocumentId });
        }

        // AI Summary
        [AllowAnonymous]
        public async Task<IActionResult> AiSummary(int id)
        {
            var doc = await _context.Documents
                .Include(d => d.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null)
                return Json(new { summary = "Document not found." });

            string prompt = $"Please provide a brief summary (3-5 sentences, each on a new line with bullet points) in English about the following study material: " +
                            $"Name: {doc.Title}. Course: {doc.Course?.CourseName}. " +
                            $"Type: {doc.DocType}. Description: {doc.Description ?? "No description"}. " +
                            $"Please respond naturally and briefly outline the concept (if any) of the topic as a friendly study assistant.";
            try
            {
                using var client = new HttpClient();

                var apiKey = _configuration["GeminiApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                    return Json(
                        new 
                        { 
                            summary = "API key not configured. Please contact the admin."
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
                            summary = "The AI summarization feature is currently unavailable. Please try again later."
                        }
                    );

                var json = System.Text.Json.JsonDocument.Parse(rawJson).RootElement;
                string summary = json
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "Cannot be summarized.";

                return Json(new { summary });
            }
            catch (Exception)
            {
                return Json(
                    new 
                    { 
                        summary = "The AI summarization feature is currently unavailable. Please try again later."
                    }
                );
            }
        }

    }
}
