using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniDocs.Data;
using UniDocs.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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
        public ViewResult Login()
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
                /*ViewBag.Error = "Email hoặc mật khẩu không chính xác!";*/
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác!");
                return View(model);
            }

            if (user.IsActive == false)
            {
                /*ViewBag.Error = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin.";*/
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin!");
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
        public ViewResult Register()
        {
            return View();
        }
        // === Xử lý đăng ký ===
        [HttpPost]
        public IActionResult Register(User model)
        {
            // Check input => [Required] trong Models
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check email tồn tại trong db chưa
            var emailTonTai = _context.Users.Any(u => u.Email == model.Email);
            if (emailTonTai)
            {
                /*ViewBag.Error = "Email này đã được sử dụng!";*/
                ModelState.AddModelError(string.Empty, "Email này đã được sử dụng!");
                return View(model);
            }

            /*var emailTonTai = _context.Users.Any(u => u.Email == email);
            if (emailTonTai)
            {
                ViewBag.Error = "Email này đã được sử dụng!";
                return View();
            }*/


            // Mã hóa mật khẩu
            /*string hashedPass = BCrypt.Net.BCrypt.HashPassword(password);*/
            model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);


            // Tạo Object User mới và lưu vào DB
            /*var newUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = hashedPass,
                University = university,
                Role = "Student",
                IsActive = true
            };*/

            model.Role = "Student";
            model.IsActive = true;

            // Lưu vào db
            _context.Users.Add(model);
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
        [Authorize]
        public IActionResult Profile()
        {
            // Lấy id đăng nhập từ Cookie
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdStr);

            // Tìm User trong db -> lấy các Documents đã upload của user đó
            var user = _context.Users.Include(u => u.Documents)
                .ThenInclude(d => d.Course).FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


    }
}
