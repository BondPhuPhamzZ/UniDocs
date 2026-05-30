using System.ComponentModel.DataAnnotations;

namespace UniDocs.ViewModels
{
    public class RegisterViewModel
    {
        [Required] 
        public string FirstName { get; set; }

        [Required] 
        public string LastName { get; set; }

        [Required, EmailAddress] 
        public string Email { get; set; }

        [Required, DataType(DataType.Password)] 
        public string Password { get; set; }

        [Compare("Password")] 
        public string ConfirmPassword { get; set; }

        public string? University { get; set; }

    }
}
