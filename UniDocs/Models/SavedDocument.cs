using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniDocs.Models
{
    public class SavedDocument
    {
        [Key]
        public int Id { get; set; }

        // Svien nào lưu
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        // Lưu tài liệu nào
        [Required]
        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }

        // Ngày giờ bấm yêu thích
        public DateTime SavedDate { get; set; } = DateTime.Now;


    }
}
