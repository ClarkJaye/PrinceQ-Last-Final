using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PrinceQ.Models.ViewModel
{
    public class UserVM
    {
        public string? Id { get; set; }

        public bool IsActive { get; set; }
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;
        [ValidateNever]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        //[Required]
        [ValidateNever]
        public string? Role { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem>? RoleList { get; set; }
    }
}
