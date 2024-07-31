using PrinceQ.DataAccess.Data.Context;
using PrinceQ.DataAccess.Repository.IRepository;
using PrinceQ.Models.Entities;

namespace PrinceQ.DataAccess.Repository
{
    public class CategoryRepo : Repository<Category>, ICategoryRepo
    {
        public CategoryRepo(AppDbContext db) : base(db)
        {
        }

    }
}
