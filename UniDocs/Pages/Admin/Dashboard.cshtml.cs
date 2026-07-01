using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniDocs.Data;
using UniDocs.Models;

namespace UniDocs.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DashboardModel(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IList<Report> Reports { get; set; } = new List<Report>();

        public async Task OnGetAsync()
        {
            Reports = await _context.Reports
                .Include(r => r.Document)
                .Include(r => r.Reporter)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }

        // Xóa tài liệu vi phạm (Soft Delete)
        public async Task<IActionResult> OnPostDeleteDocumentAsync(int id, [FromServices] UniDocs.Services.ICloudinaryService cloudinaryService)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound();

            // Xóa file vật lý cũ (nếu có) -> KO QUAN TRỌNG
            if (document.FilePath != null && document.FilePath.StartsWith("/uploads/"))
            {
                string physicalPath = Path.Combine(_env.WebRootPath, document.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                    System.IO.File.Delete(physicalPath);
            }

            // Xóa file trên Cloudinary
            if (!string.IsNullOrEmpty(document.CloudinaryPublicId))
            {
                await cloudinaryService.DeleteDocumentAsync(document.CloudinaryPublicId);
            }

            // Soft Delete
            document.IsApproved = false;
            if (!document.Title.StartsWith("[Đã xóa]"))
            {
                document.Title = "[Đã xóa] " + document.Title;
            }
            
            // Đánh dấu -> đã xóa
            var relatedReports = await _context.Reports.Where(r => r.DocumentId == id).ToListAsync();
            foreach(var report in relatedReports)
            {
                report.Status = UniDocs.Models.Enums.ReportStatusEnum.Valid; // Hợp lệ (vi phạm thật) => đã xóa
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa tài liệu vi phạm thành công!";
            return RedirectToPage("./Dashboard");
        }

        // Bỏ qua -> Hợp lệ
        public async Task<IActionResult> OnPostDismissReportAsync(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                return NotFound();

            report.Status = UniDocs.Models.Enums.ReportStatusEnum.Rejected;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã đánh dấu tài liệu là Hợp lệ!";
            return RedirectToPage("./Dashboard");
        }
    }
}
