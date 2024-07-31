using System.ComponentModel.DataAnnotations;

namespace PrinceQ.Models.ViewModel
{
    public class LoginVM
    {
        //[Required(ErrorMessage = "Email is required.")]
        //[EmailAddress]
        //[DataType(DataType.EmailAddress)]
        //public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
