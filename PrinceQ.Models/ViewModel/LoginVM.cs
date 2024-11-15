using System.ComponentModel.DataAnnotations;

namespace PrinceQ.Models.ViewModel
{
    public class LoginVM
    {
        public string UserCode { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool UseADAuthentication { get; set; } = false;
    }
}
