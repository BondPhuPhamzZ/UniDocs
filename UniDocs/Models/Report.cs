using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniDocs.Models
{
    // ===== Bang bao cao vi pham =====
    public class Report
    {
        // 1. ID
        [Key]
        public int Id { get; set; }


        // 2. Ly do
        [Required]
        public string Reason { get; set; } = string.Empty;


        // 3. Ngay gio gui report
        public DateTime ReportDate { get; set; } = DateTime.Now;


        // 4. Trang thai xu ly
        public string Status { get; set; } = "Đang xử lý";


        // 5. FK connect to bảng Document (tài liệu bị báo cáo)
        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }


        // 6. FK connect to bảng User (người gửi báo cáo)
        public int ReporterId { get; set; }
        [ForeignKey("ReporterId")]
        public User? Reporter { get; set; }


    }
}
