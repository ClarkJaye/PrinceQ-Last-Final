using PrinceQ.DataAccess.Data.Context;
using PrinceQ.DataAccess.Repository.IRepository;
using PrinceQ.Models.Entities;

namespace PrinceQ.DataAccess.Repository
{
    public class ClerkForFillingRepo : Repository<Clerk_Serve_ForFilling>, IClerkForFillingRepo
    {
        public ClerkForFillingRepo(AppDbContext db) : base(db)
        {
        }

    }
}
