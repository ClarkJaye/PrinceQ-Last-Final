using PrinceQ.DataAccess.Data.Context;
using PrinceQ.DataAccess.Repository.IRepository;
using PrinceQ.Models.Entities;

namespace PrinceQ.DataAccess.Repository
{
    public class DeviceRepo : Repository<ClerkIPAddress>, IDeviceRepo
    {
        public DeviceRepo(AppDbContext db) : base(db)
        {
        }

    }
}
