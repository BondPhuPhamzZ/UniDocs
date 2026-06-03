using System.ComponentModel.DataAnnotations;

namespace UniDocs.Models
{
    // ===== Bang mon hoc =====
    public class Course
    {
        // ID
        [Key]
        public int Id { get; set; }


        // Ma mon hoc
        [Required]
        [StringLength(20)]
        public string CourseCode { get; set; } = string.Empty; // IT101


        // Ten mon hoc
        [Required]
        [StringLength(200)]
        public string CourseName { get; set; } = string.Empty;


        // Khoa
        public string? Department { get; set; }


        // 1 mon hoc có nhiều tai lieu
        public ICollection<Document> Documents { get; set; } = new List<Document>();


    }
}
