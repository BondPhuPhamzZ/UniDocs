using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniDocs.Data;

namespace UniDocs.ViewComponents
{
    /*public class CourseSidebarViewComponent
    {
    }*/

    // Kế thừa ViewComponent
    public class CourseSidebarViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        // Inject db
        public CourseSidebarViewComponent(AppDbContext context)
        {
            _context = context;
        }

        // Side bar view dc gọi
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy dsach môn học, kèm theo đếm số lượng tài liệu đã dc duyệt của môn đó
            var courses = await _context.Courses
                .Include(c => c.Documents)
                .ToListAsync();

            return View(courses);
        }

    }

}
