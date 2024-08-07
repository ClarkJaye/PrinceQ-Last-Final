using PrinceQ.DataAccess.Data.Context;
using PrinceQ.DataAccess.Repository.IRepository;
using PrinceQ.Models.Entities;

namespace PrinceQ.DataAccess.Repository
{
    public class ForFillingRepo : Repository<Serve_ForFilling>, IForFillingRepo
    {
        public ForFillingRepo(AppDbContext db) : base(db)
        {
        }

    }
}
