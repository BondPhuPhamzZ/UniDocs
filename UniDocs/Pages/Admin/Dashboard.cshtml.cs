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

        public async Task<IActionResult> OnPostDeleteDocumentAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound();

            if (document.FilePath != null && document.FilePath.StartsWith("/uploads/"))
            {
                string physicalPath = Path.Combine(_env.WebRootPath, document.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                    System.IO.File.Delete(physicalPath);
            }

            // Xóa mềm tài liệu
            document.Status = UniDocs.Models.Enums.DocumentStatusEnum.Deleted;
            
            // Chuyển toàn bộ Report Pending của tài liệu này thành Finished
            var pendingReports = await _context.Reports.Where(r => r.DocumentId == id && r.Status == UniDocs.Models.Enums.ReportStatusEnum.Pending).ToListAsync();
            foreach(var report in pendingReports)
            {
                report.Status = UniDocs.Models.Enums.ReportStatusEnum.Finished; 
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Violating document deleted successfully!";
            return RedirectToPage("./Dashboard");
        }

        // Bỏ qua -> Tài liệu được phục hồi
        public async Task<IActionResult> OnPostDismissReportAsync(int id)
        {
            var report = await _context.Reports.Include(r => r.Document).FirstOrDefaultAsync(r => r.Id == id);
            if (report == null)
                return NotFound();

            // Chuyển toàn bộ Report Pending của tài liệu này thành Finished
            var pendingReports = await _context.Reports.Where(r => r.DocumentId == report.DocumentId && r.Status == UniDocs.Models.Enums.ReportStatusEnum.Pending).ToListAsync();
            foreach (var r in pendingReports)
            {
                r.Status = UniDocs.Models.Enums.ReportStatusEnum.Finished;
            }

            // Phục hồi tài liệu (nếu nó đang bị Pending)
            if (report.Document != null && report.Document.Status == UniDocs.Models.Enums.DocumentStatusEnum.Pending)
            {
                report.Document.Status = UniDocs.Models.Enums.DocumentStatusEnum.Approved;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Report dismissed and document restored successfully!";
            return RedirectToPage("./Dashboard");
        }
    }
}
