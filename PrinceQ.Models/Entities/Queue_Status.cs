using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrinceQ.Models.Entities
{
    public class Queue_Status
    {
        [Key]
        public int? StatusId { get; set; }

        public string? StatusName { get; set; }

        public DateTime Created_At { get; set; }

    }
}
