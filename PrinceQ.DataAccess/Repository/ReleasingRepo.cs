using PrinceQ.DataAccess.Data.Context;
using PrinceQ.DataAccess.Repository.IRepository;
using PrinceQ.Models.Entities;

namespace PrinceQ.DataAccess.Repository
{
    public class ReleasingRepo : Repository<Serve_Releasing>, IReleasingRepo
    {
        public ReleasingRepo(AppDbContext db) : base(db)
        {
        }

    }
}
