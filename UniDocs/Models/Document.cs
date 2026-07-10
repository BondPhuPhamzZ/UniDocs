using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniDocs.Models
{
    public class Document
    {
        // ID
        [Key]
        public int Id { get; set; }


        // Title
        [Required(ErrorMessage = "Document title is required")]
        [MaxLength(100, ErrorMessage = "Document title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;


        // Mô tả thêm (tùy chọn)
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }


        // Loai tai lieu
        public string DocType { get; set; } = string.Empty;


        // Nam hoc
        [MaxLength(20, ErrorMessage = "Invalid academic year")]
        public string AcademicYear { get; set; }


        // Đường dẫn lưu file trên máy chủ 
        [Required]
        public string FilePath { get; set; } = string.Empty;


        // Ngay gio UP tai lieu len
        public DateTime UploadDate { get; set; } = DateTime.Now;


        // So luong user DOWLOAD docs ve
        public int DownloadCount { get; set; } = 0;


        // Trạng thái tài liệu
        public UniDocs.Models.Enums.DocumentStatusEnum Status { get; set; } = UniDocs.Models.Enums.DocumentStatusEnum.Pending;


        // FK (khóa ngoại) connect to bảng User (người dùng)
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }


        // FK connect to bảng Course (môn học)
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        // Danh sách người đã lưu tài liệu này (quan hệ ngược lại với SavedDocument)
        public ICollection<SavedDocument> SavedDocuments { get; set; } = new List<SavedDocument>();

    }
}
