using System.ComponentModel.DataAnnotations;

namespace UniDocs.Models
{
    // ===== Bang mon hoc =====
    public class Course
    {
        // 1. ID
        [Key]
        public int Id { get; set; }


        // 2. Ma mon hoc
        [Required]
        [StringLength(20)]
        public string CourseCode { get; set; } = string.Empty; // IT101


        // 3. Ten mon hoc
        [Required]
        [StringLength(200)]
        public string CourseName { get; set; } = string.Empty;


        // 4. Khoa
        public string? Department { get; set; }


        // 5. 1 mon hoc có nhiều tai lieu
        public ICollection<Document>? Documents { get; set; }


    }
}
