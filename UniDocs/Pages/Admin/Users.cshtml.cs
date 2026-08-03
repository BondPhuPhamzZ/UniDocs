using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniDocs.Data;
using UniDocs.Models;

namespace UniDocs.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly AppDbContext _context;

        public UsersModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<User> UserList { get; set; } = new List<User>();

        public async Task OnGetAsync()
        {
            UserList = await _context.Users
                .Include(u => u.Documents)
                .OrderBy(u => u.LastName)
                .ToListAsync();
        }

        // Lock/ Unlock
        public async Task<IActionResult> OnPostToggleLockAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            if (user.Role == Models.Enums.RoleEnum.Admin)
            {
                TempData["ErrorMessage"] = "Cannot lock/unlock an Admin account!";
                return RedirectToPage("./Users");
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            string msg = user.IsActive ? "Account unlocked!" : "Account locked!";
            TempData["SuccessMessage"] = msg;

            return RedirectToPage("./Users");
        }
    }
}
