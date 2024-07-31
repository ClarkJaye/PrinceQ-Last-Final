using PrinceQ.DataAccess.Data.Context;
using PrinceQ.DataAccess.Repository.IRepository;
using PrinceQ.Models.Entities;

namespace PrinceQ.DataAccess.Repository
{
    public class QueueNumberRepo : Repository<Queues>, IQueueNumberRepo
    {
        public QueueNumberRepo(AppDbContext db) : base(db)
        {
        }

    }

}
