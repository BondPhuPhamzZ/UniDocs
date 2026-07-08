using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniDocs.Data;

namespace UniDocs.ViewComponents
{
    public class CourseSidebarViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public CourseSidebarViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var courses = await _context.Courses
                .Include(c => c.Documents.Where(d => d.Status == UniDocs.Models.Enums.DocumentStatusEnum.Approved))
                .ToListAsync();

            return View(courses);
        }

    }

}
