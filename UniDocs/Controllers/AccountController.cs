using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniDocs.Data;
using UniDocs.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace UniDocs.Controllers
{
    // ===== Quản lý đăng nhập/ đăng ký =====
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;


        // Tiêm (Inject) Database vào Controller để sử dụng
        public AccountController(AppDbContext context)
        {
            _context = context;
        }


        // ========== Đăng nhập ===========
        public IActionResult Login()
        {
            return View();
        }

        // === Xử lý đăng nhập ===
        [HttpPost]
        // public async Task<IActionResult> Login(string email, string password)
        public async Task<IActionResult> Login(User model)
        {
            // 1. Tìm user trong Database theo Email
            /*var user = _context.Users.FirstOrDefault(u => u.Email == email);*/
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            // 2. Ktra tài khaonr có tồn tại, có bị khóa, pass có khớp ko
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.PasswordHash, user.PasswordHash))
            {
                ViewBag.Error = "Email hoặc mật khẩu không chính xác!";
                return View(model);
            }
            if (user.IsActive == false)
            {
                ViewBag.Error = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin.";
                return View(model);
            }
            // 3. Tạo "vé thông hành" (Cookie)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role) // Quan trọng để phân biệt Admin/ Sinh viên
            };

            // Cần xem lại
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Cấp phát Cookie cho trình duyệt
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // 4. Phân luồng -> Admin về Dashboard -> Sinh viên thì về Trang Chủ
            if (user.Role == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            return RedirectToAction("Index", "Home");
        }


        // ========== Đăng ký ==========
        public IActionResult Register()
        {
            return View();
        }
        // === Xử lý đăng ký ===
        [HttpPost]
        public IActionResult Register(string firstName, string lastName, string email, string password, string university)
        {
            // 1. Check email tồn tại trong db chưa
            var emailTonTai = _context.Users.Any(u => u.Email == email);
            if (emailTonTai)
            {
                ViewBag.Error = "Email này đã được sử dụng!";
                return View();
            }
            // 2. Mã hóa mật khẩu
            string hashedPass = BCrypt.Net.BCrypt.HashPassword(password);
            // 3. Tạo Object User mới và lưu vào DB
            var newUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = hashedPass,
                University = university,
                Role = "Student",
                IsActive = true
            };
            _context.Users.Add(newUser);
            _context.SaveChanges();
            return RedirectToAction("Login", "Account");
        }


        // ===== Xử lý đăng xuất =====
        public async Task<IActionResult> Logout()
        {
            // "Xé vé" -> Xóa Cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }


        // User Profile -> Trang cá nhân
        public IActionResult Profile()
        {
            return View();
        }


    }
}
