using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniDocs.Data;
using UniDocs.Models.Enums;

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
                .Select(c => new
                {
                    Course = c,
                    DocCount = c.Documents.Count(d => d.Status == DocumentStatusEnum.Approved)
                })
                .ToListAsync();

            var groupedCourses = courses
                .GroupBy(c => c.Course.Department)
                .ToList();

            return View(groupedCourses);
        }

    }

}
