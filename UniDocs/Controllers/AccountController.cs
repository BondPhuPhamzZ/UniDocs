using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniDocs.Data;
using UniDocs.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using UniDocs.ViewModels;

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
        public async Task<IActionResult> Login(LoginViewModel model)
        {

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            // Ktra tài khaonr có tồn tại, có bị khóa, pass có khớp ko
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác!");
                return View(model);
            }

            if (user.IsActive == false)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin!");
                return View(model);
            }

            // Tạo "vé thông hành" (Cookie)
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
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Check input => [Required] 
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check email 
            var emailTonTai = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailTonTai)
            {
                /*ViewBag.Error = "Email này đã được sử dụng!";*/
                ModelState.AddModelError(string.Empty, "Email này đã được sử dụng!");
                return View(model);
            }

            var newUser = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                University = model.University,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),

                // Giá trị ngầm => Người dùng ko thể can thiệp
                Role = "Student",
                IsActive = true
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Bật thông báo = TempData
            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập";
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
        public async Task<IActionResult> Profile()
        {
            // Lấy id đăng nhập từ Cookie
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdStr);

            // Tìm User trong db -> lấy các Documents đã upload của user đó
            var user = await _context.Users
                .Include(u => u.Documents!)
                    .ThenInclude(d => d.Course)
                .Include(u => u.SavedDocuments!)
                    .ThenInclude(sd => sd.Document!)
                        .ThenInclude(d => d.Course)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


    }
}
