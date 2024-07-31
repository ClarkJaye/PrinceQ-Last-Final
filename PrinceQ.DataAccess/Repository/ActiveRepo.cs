using PrinceQ.DataAccess.Data.Context;
using PrinceQ.DataAccess.Repository.IRepository;
using PrinceQ.Models.Entities;

namespace PrinceQ.DataAccess.Repository
{
    public class ActiveRepo : Repository<IsActive>, IActiveRepo
    {
        public ActiveRepo(AppDbContext db) : base(db)
        {
        }
    }
}
