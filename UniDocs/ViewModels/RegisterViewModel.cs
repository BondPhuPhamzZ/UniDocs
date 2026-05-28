using System.ComponentModel.DataAnnotations;

namespace UniDocs.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage ="Vui lòng nhập tên!")]
        public string FirstName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Vui lòng nhập họ!")]
        public string LastName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email không được để trống!")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Mật khẩu không được để trống!")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu!")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp!")] 
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? University { get; set; }

    }
}
