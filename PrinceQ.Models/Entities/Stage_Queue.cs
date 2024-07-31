using System.ComponentModel.DataAnnotations;

namespace PrinceQ.Models.Entities
{
    public class Stage_Queue
    {
        [Key]
        public int StageId { get; set; }

        [Required]
        public string? StageName { get; set; }
    }
}
