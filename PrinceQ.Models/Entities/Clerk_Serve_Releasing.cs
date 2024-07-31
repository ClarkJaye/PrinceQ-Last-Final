using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrinceQ.Models.Entities
{
    public class Clerk_Serve_Releasing
    {

        [Required]
        public string? GenerateDate { get; set; }

        [Required]
        public string? ClerkId { get; set; }
        [ForeignKey("ClerkId")]
        [ValidateNever]
        public User? User { get; set; }

        [Required]
        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category? Category { get; set; }

        [Required]
        public int? QueueNumber { get; set; }

        public DateTime? Serve_start { get; set; }

        public DateTime? Serve_end { get; set; }

    }
}
