using Microsoft.AspNetCore.Mvc;

namespace UniDocs.Controllers
{
    // Admin
    public class AdminController : Controller
    {
        // Bảng điều khiển (Xem các tài liệu bị báo cáo)
        public IActionResult Dashboard()
        {
            return View();
        }
        // Quản lý người dùng (trạng thái tài khoản -> khóa/ mở)
        public IActionResult Users()
        {
            return View();
        }
        // Quản lý môn học => Thêm/ Xóa/ Sửa -> danh mục môn
        public IActionResult Courses()
        {
            return View();
        }
    }
}
