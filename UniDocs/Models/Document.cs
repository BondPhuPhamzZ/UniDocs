using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniDocs.Models
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Document title is required")]
        [MaxLength(100, ErrorMessage = "Document title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        public string DocType { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "Invalid academic year")]
        public string AcademicYear { get; set; }

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public DateTime UploadDate { get; set; } = DateTime.Now;

        public int DownloadCount { get; set; } = 0;

        public Enums.DocumentStatusEnum Status { get; set; } = Enums.DocumentStatusEnum.Pending;

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }


        public ICollection<SavedDocument> SavedDocuments { get; set; } = new List<SavedDocument>();

    }
}
