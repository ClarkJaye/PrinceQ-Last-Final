using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace PrinceQ.Models.Entities
{
    public class User : IdentityUser
    {
        [Range(1, 2, ErrorMessage = "IsActive is required.")]
        public int IsActiveId { get; set; }
        [ForeignKey("IsActiveId")]
        [ValidateNever]
        public IsActive? IsActive { get; set; }

        public DateTime Created_At { get; set; }

    }
}