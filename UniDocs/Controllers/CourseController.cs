using Microsoft.AspNetCore.Mvc;

namespace UniDocs.Controllers
{
    // ===== Quản lý môn học =====
    public class CourseController : Controller
    {
        // Trang danh sách các môn học
        public IActionResult Index()
        {
            return View();
        }


        // Trang chi tiết tài liệu của 1 môn (major.html)
        public IActionResult Detail()
        {
            return View();
        }


    }
}
