
using System.ComponentModel.DataAnnotations;

namespace PrinceQ.Models.Entities
{
    public class IsActive
    {
        [Key]
        public int IsActiveId { get; set; }
     
        public string? Name { get; set; }



    }
}
