using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniDocs.Models
{
    public class Document
    {
        // 1. ID
        [Key]
        public int Id { get; set; }


        // 2. Title
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;


        // 3. Mô tả thêm (tùy chọn)
        public string Description { get; set; }


        // 4. Loai tai lieu
        public string DocType { get; set; } = string.Empty;


        // 5. Nam hoc
        public string AcademicYear { get; set; }


        // 6. Đường dẫn lưu file trên máy chủ (.pdf ; .docx ;...)
        [Required]
        public string FilePath { get; set; } = string.Empty;


        // 7. Ngay gio UP tai lieu len
        public DateTime UploadData { get; set; } = DateTime.Now;


        // 8. So luong user DOWLOAD docs ve
        public int DowloadCount { get; set; } = 0;


        // 9. Khi UPLOAD thì tài liệu cần đc Admin duyệt -> Default là FALSE -> đang chờ duyệt
        public bool IsApproved { get; set; } = false; 


        // 10. FK (khóa ngoại) connect to bảng User (người dùng)
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }


        //11.  FK connect to bảng Course (môn học)
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }



    }
}
