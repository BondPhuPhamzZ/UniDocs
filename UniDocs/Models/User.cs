using System.ComponentModel.DataAnnotations;

namespace UniDocs.Models
{
    // ===== Bang nguoi dung =====
    public class User
    {
        // 1. ID
        [Key]
        public int Id { get; set; }

        // 2. Input tên
        [Required(ErrorMessage = "Vui lòng nhập Tên")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;


        // 3. Input họ
        [Required(ErrorMessage = "Vui lòng nhập Họ")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;


        // 4. Email
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;


        // 5. PasswordHash
        [Required]
        public string PasswordHash { get; set; } = string.Empty;


        // 6. Ten truong
        public string? University { get; set; }


        // 7. Default ai dky cx la Sinh Vien => Student/ Admin
        public string Role { get; set; } = "Student";


        // 8. Trang thai tkhoan => TRUE là hoạt động, FALSE là bị KHÓA
        public bool IsActive { get; set; } = true;


        // 9. 1 User có thể đăng nhiều document
        public ICollection<Document>? Documents { get; set; } 

        // 10. Danh sách tài liệu đã lưu vào yêu thích
        public ICollection<SavedDocument>? SavedDocuments { get; set; }

    }
}
