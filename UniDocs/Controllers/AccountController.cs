using Microsoft.AspNetCore.Mvc;

namespace UniDocs.Controllers
{
    // ===== Quản lý đăng nhập/ đăng ký =====
    public class AccountController : Controller
    {
        // Đăng nhập
        public IActionResult Login()
        {
            return View();
        }


        // Đăng ký
        public IActionResult Register()
        {
            return View();
        }


        // User Profile
        public IActionResult Profile()
        {
            return View();
        }


    }
}
