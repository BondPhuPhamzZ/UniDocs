using System.ComponentModel.DataAnnotations;

namespace UniDocs.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current Password is required.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "New Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Confirm New Password is required.")]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
