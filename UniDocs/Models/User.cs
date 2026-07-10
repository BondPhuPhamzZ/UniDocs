using System.ComponentModel.DataAnnotations;
using UniDocs.Models.Enums;

namespace UniDocs.Models
{
    // ===== Bang nguoi dung =====
    public class User
    {
        // ID
        [Key]
        public int Id { get; set; }

        // Input tên
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;


        // Input họ
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;


        // Email
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;


        // PasswordHash
        [Required]
        public string PasswordHash { get; set; } = string.Empty;


        // Ten truong
        public string? University { get; set; }


        // Default ai dky cx la Sinh Vien => Student/ Admin
        public RoleEnum Role { get; set; } = RoleEnum.Student;


        // Trang thai tkhoan => TRUE là hoạt động, FALSE là bị KHÓA
        public bool IsActive { get; set; } = true;


        // 1 User có thể đăng nhiều document
        public ICollection<Document> Documents { get; set; } = new List<Document>();

        // Danh sách tài liệu đã lưu vào yêu thích
        public ICollection<SavedDocument> SavedDocuments { get; set; } = new List<SavedDocument>();

    }
}
