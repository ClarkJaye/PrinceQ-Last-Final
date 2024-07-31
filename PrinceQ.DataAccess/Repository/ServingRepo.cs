using PrinceQ.DataAccess.Data.Context;
using PrinceQ.DataAccess.Repository.IRepository;
using PrinceQ.Models.Entities;

namespace PrinceQ.DataAccess.Repository
{
    public class ServingRepo : Repository<Serving>, IServingRepo
    {
        public ServingRepo(AppDbContext db) : base(db)
        {
        }

    }

}
