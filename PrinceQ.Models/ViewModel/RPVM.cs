using PrinceQ.Models.Entities;

namespace PrinceQ.Models.ViewModel
{
    public class RPVM
    {
        public User? Users { get; set; }
        public IEnumerable<Category>? Categories { get; set; }
    }
}
