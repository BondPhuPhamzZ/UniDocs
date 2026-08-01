using System.ComponentModel.DataAnnotations;
using UniDocs.Models.Enums;

namespace UniDocs.Models
{
    // ===== Bang nguoi dung =====
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string? University { get; set; }

        public RoleEnum Role { get; set; } = RoleEnum.Student;

        public bool IsActive { get; set; } = true;

        public int Credits { get; set; } = 3;

        public DateTime? VipExpirationDate { get; set; }

        public int DailyDownloadCount { get; set; } = 0;

        public DateTime? LastDownloadDate { get; set; }


        public ICollection<Document> Documents { get; set; } = new List<Document>();

        public ICollection<SavedDocument> SavedDocuments { get; set; } = new List<SavedDocument>();

    }
}
