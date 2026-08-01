using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniDocs.Models.Enums;

namespace UniDocs.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        public DateTime ReportDate { get; set; } = DateTime.Now;

        public ReportStatusEnum Status { get; set; } = ReportStatusEnum.Pending;

        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }

        public int ReporterId { get; set; }
        [ForeignKey("ReporterId")]
        public User? Reporter { get; set; }


    }
}
