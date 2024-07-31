using PrinceQ.DataAccess.Data.Context;
using PrinceQ.DataAccess.Repository.IRepository;
using PrinceQ.Models.Entities;

namespace PrinceQ.DataAccess.Repository
{
    public class UsersRepo : Repository<User>, IUsersRepo
    {
        public UsersRepo(AppDbContext db) : base(db)
        {
        }
    }
}
