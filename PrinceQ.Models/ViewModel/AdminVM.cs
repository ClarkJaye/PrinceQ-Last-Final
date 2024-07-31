using PrinceQ.Models.Entities;

namespace PrinceQ.Models.ViewModel
{
    public class AdminVM
    {
        public List<Queues>? QueueNumbers { get; set; }
        public List<User_Category>? UserCategories { get; set; }

        public IEnumerable<Category>? categories { get; set; }

        public List<User>? Users { get; set; }

    }
}
