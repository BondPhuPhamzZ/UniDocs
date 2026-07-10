using System.ComponentModel.DataAnnotations;

namespace UniDocs.ViewModels
{
    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        public string LastName { get; set; } = null!;

        [StringLength(100, ErrorMessage = "University name cannot exceed 100 characters.")]
        public string? University { get; set; }
    }
}
