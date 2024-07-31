using PrinceQ.DataAccess.Data.Context;
using PrinceQ.DataAccess.Repository.IRepository;
using PrinceQ.Models.Entities;

namespace PrinceQ.DataAccess.Repository
{
    public class UserCategory : Repository<User_Category>, IUserCategoryRepo
    {
        public UserCategory(AppDbContext db) : base(db)
        {
        }

    }
}
