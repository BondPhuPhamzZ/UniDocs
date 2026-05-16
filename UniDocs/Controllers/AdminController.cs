using Microsoft.AspNetCore.Mvc;

namespace UniDocs.Controllers
{
    // Admin
    public class AdminController : Controller
    {
        // Quản lý môn học
        public IActionResult Dashboard()
        {
            return View();
        }
        // Quản lý người dùng (trạng thái tài khoản)
        public IActionResult Users()
        {
            return View();
        }
        // Tài liệu bị báo cáo
        public IActionResult Courses()
        {
            return View();
        }
    }
}
