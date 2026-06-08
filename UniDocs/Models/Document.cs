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
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;


        // Mô tả thêm (tùy chọn)
        public string Description { get; set; }


        // Loai tai lieu
        public string DocType { get; set; } = string.Empty;


        // Nam hoc
        public string AcademicYear { get; set; }


        // Đường dẫn lưu file trên máy chủ 
        [Required]
        public string FilePath { get; set; } = string.Empty;


        // Ngay gio UP tai lieu len
        public DateTime UploadDate { get; set; } = DateTime.Now;


        // So luong user DOWLOAD docs ve
        public int DownloadCount { get; set; } = 0;


        // Khi UPLOAD thì tài liệu cần đc Admin duyệt -> Default là FALSE -> đang chờ duyệt
        public bool IsApproved { get; set; } = false; 


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
