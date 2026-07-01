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
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UniDocs.Services.SecurityService _securityService;

        public AccountController(AppDbContext context, UniDocs.Services.SecurityService securityService)
        {
            _context = context;
            _securityService = securityService;
        }

        // ========== Đăng nhập ===========
        public ViewResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Password = "";
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !_securityService.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác!");
                return View(model);
            }

            if (user.IsActive == false)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin!");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()) 
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (user.Role == UniDocs.Models.Enums.RoleEnum.Admin)
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Password = "";
                return View(model);
            }

            // Check email 
            var emailTonTai = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailTonTai)
            {
                ModelState.AddModelError(string.Empty, "Email này đã được sử dụng!");
                return View(model);
            }

            var newUser = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                University = model.University,
                PasswordHash = _securityService.HashPassword(model.Password),

                Role = UniDocs.Models.Enums.RoleEnum.Student,
                IsActive = true
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập";
            return RedirectToAction("Login", "Account");

        }


        // ===== Xử lý đăng xuất =====
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }


        // ========== User Profile -> Trang cá nhân ==========
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdStr);

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


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMyDocument(int id, [FromServices] UniDocs.Services.ICloudinaryService cloudinaryService)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdStr);

            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (document == null)
            {
                return NotFound("Tài liệu không tồn tại hoặc bạn không có quyền xóa!");
            }


            // Xóa file vật lý (giữ lại để tương thích ngược với file cũ nếu có)
            if (document.FilePath.StartsWith("/uploads/"))
            {
                // TODO: Dọn dẹp đoạn code này sau nếu hệ thống đã lên Cloudinary hoàn toàn
            }


            // Xóa file trên Cloudinary
            if (!string.IsNullOrEmpty(document.CloudinaryPublicId))
            {
                await cloudinaryService.DeleteDocumentAsync(document.CloudinaryPublicId);
            }

            // Soft delete
            document.IsApproved = false;
            if (!document.Title.StartsWith("[Đã xóa]"))
            {
                document.Title = "[Đã xóa] " + document.Title;
            }

            // Đánh dấu các report thành đã xóa
            var relatedReports = await _context.Reports.Where(r => r.DocumentId == id).ToListAsync();
            foreach (var report in relatedReports)
            {
                report.Status = UniDocs.Models.Enums.ReportStatusEnum.Valid; // Đã xóa tài liệu (Hợp lệ)
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa tài liệu thành công!";
            return RedirectToAction("Profile");
        }

    }
}
