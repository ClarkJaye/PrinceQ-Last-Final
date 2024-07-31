using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PrinceQ.Models.Entities
{
    public class Queues
    {
        [Required]
        public string? QueueId { get; set; }

        [Required]
        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category? Category { get; set; }

        [Required]
        public int? StatusId { get; set; }
        [ForeignKey("StatusId")]
        [ValidateNever]
        public Queue_Status? QueueStatus { get; set; }

        [Required]
        public int? QueueNumber { get; set; }

        public int? Total_Cheques { get; set; }

        public DateTime? ForFilling_start { get; set; }
        public DateTime? ForFilling_end { get; set; }
        public DateTime? Releasing_start { get; set; }
        public DateTime? Releasing_end { get; set; }

        public DateTime? Reserve_At { get; set; }
        public DateTime? Cancelled_At { get; set; }

        public string? ClerkId { get; set; }
        [ForeignKey("ClerkId")]
        [ValidateNever]
        public User? User { get; set; }

        public int? StageId { get; set; }
        [ForeignKey("StageId")]
        [ValidateNever]
        public Stage_Queue? Stage { get; set; }
    }
}