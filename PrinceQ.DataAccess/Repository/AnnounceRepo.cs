using PrinceQ.DataAccess.Data.Context;
using PrinceQ.DataAccess.Repository.IRepository;
using PrinceQ.Models.Entities;

namespace PrinceQ.DataAccess.Repository
{
    public class AnnounceRepo : Repository<Announcement>, IAnnounceRepo
    {
        public AnnounceRepo(AppDbContext db) : base(db)
        {
        }
    }
}
