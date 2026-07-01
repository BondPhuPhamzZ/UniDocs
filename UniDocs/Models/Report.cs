using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniDocs.Models.Enums;

namespace UniDocs.Models
{
    public class Report
    {
        // ID
        [Key]
        public int Id { get; set; }


        // Ly do
        [Required]
        public string Reason { get; set; } = string.Empty;


        // Ngay gio gui report
        public DateTime ReportDate { get; set; } = DateTime.Now;


        // Trang thai xu ly
        public ReportStatusEnum Status { get; set; } = ReportStatusEnum.Pending;


        // FK connect to bảng Document (tài liệu bị báo cáo)
        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }


        // FK connect to bảng User (người gửi báo cáo)
        public int ReporterId { get; set; }
        [ForeignKey("ReporterId")]
        public User? Reporter { get; set; }


    }
}
