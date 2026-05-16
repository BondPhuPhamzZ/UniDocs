using Microsoft.AspNetCore.Mvc;

namespace UniDocs.Controllers
{
    // ===== Đóng góp tài liệu =====
    public class DocumentController : Controller
    {
        public IActionResult Upload()
        {
            return View();
        }
    }
}
