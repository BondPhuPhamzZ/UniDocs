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
        private readonly Services.SecurityService _securityService;
        private readonly IWebHostEnvironment _env;

        public AccountController(AppDbContext context, Services.SecurityService securityService, IWebHostEnvironment env)
        {
            _context = context;
            _securityService = securityService;
            _env = env;
        }

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
                ModelState.AddModelError(string.Empty, "Invalid email or password!");
                return View(model);
            }

            if (user.IsActive == false)
            {
                ModelState.AddModelError(string.Empty, "Your account is locked. Please contact Admin!");
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
                ModelState.AddModelError(string.Empty, "This email is already in use!");
                return View(model);
            }

            var newUser = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                University = model.University,
                PasswordHash = _securityService.HashPassword(model.Password),

                Role = Models.Enums.RoleEnum.Student,
                IsActive = true
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration successful! Please login.";
            return RedirectToAction("Login", "Account");

        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }


        // ========== User Profile ==========
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
        public async Task<IActionResult> DeleteMyDocument(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdStr);

            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (document == null)
            {
                return NotFound("The document does not exist or you do not have permission to delete it!");
            }

            // Xóa file vật lý
            if (document.FilePath != null && document.FilePath.StartsWith("/uploads/"))
            {
                string physicalPath = System.IO.Path.Combine(_env.WebRootPath, document.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                    System.IO.File.Delete(physicalPath);
            }

            document.Status = Models.Enums.DocumentStatusEnum.Deleted;

            var relatedReports = await _context.Reports.Where(r => r.DocumentId == id && r.Status == UniDocs.Models.Enums.ReportStatusEnum.Pending).ToListAsync();
            foreach (var report in relatedReports)
            {
                report.Status = Models.Enums.ReportStatusEnum.Finished; 
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Document deleted successfully!";
            return RedirectToAction("Profile");
        }

        // ========== Update Profile ==========
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid information. Please check again.";
                return RedirectToAction("Profile");
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdStr);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.University = model.University;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // ========== Change Password ==========
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid password information. Please check again.";
                return RedirectToAction("Profile");
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdStr);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (!_securityService.VerifyPassword(model.CurrentPassword, user.PasswordHash))
            {
                TempData["ErrorMessage"] = "Current password is incorrect!";
                return RedirectToAction("Profile");
            }

            user.PasswordHash = _securityService.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("Profile");
        }

        [AllowAnonymous] 
        public async Task<IActionResult> PublicProfile(int id)
        {
            var user = await _context.Users
                .Include(u => u.Documents.Where(d => d.Status == Models.Enums.DocumentStatusEnum.Approved))
                    .ThenInclude(d => d.Course)
                .AsNoTracking() 
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            return View(user);
        }

    }
}
