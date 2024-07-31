using PrinceQ.DataAccess.Data.Context;
using PrinceQ.DataAccess.Repository.IRepository;
using PrinceQ.Models.Entities;

namespace PrinceQ.DataAccess.Repository
{
    public class ClerkReleasingRepo : Repository<Clerk_Serve_Releasing>, IClerkReleasingRepo
    {
        public ClerkReleasingRepo(AppDbContext db) : base(db)
        {
        }

    }
}
