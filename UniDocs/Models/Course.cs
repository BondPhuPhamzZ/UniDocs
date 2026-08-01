using System.ComponentModel.DataAnnotations;

namespace UniDocs.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string CourseCode { get; set; } = string.Empty; // IT101

        [Required]
        [StringLength(200)]
        public string CourseName { get; set; } = string.Empty;

        public string? Department { get; set; }

        public ICollection<Document> Documents { get; set; } = new List<Document>();


    }
}
