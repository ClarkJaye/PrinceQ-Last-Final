using Microsoft.AspNetCore.Identity;
using PrinceQ.DataAccess.Data.Context;
using PrinceQ.DataAccess.Repository.IRepository;
using PrinceQ.Models.Entities;

namespace PrinceQ.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;
        private RoleManager<IdentityRole> _roleManager;

        public IAuthRepo auth{ get; private set; }
        public IAnnounceRepo announcement { get; private set; }
        public IDeviceRepo device { get; private set; }
        public ICategoryRepo category { get; private set; }
        public IClerkForFillingRepo clerkForFilling { get; private set; }
        public IClerkReleasingRepo clerkReleasing { get; private set; }
        public IQueueNumberRepo queueNumbers { get; private set; }
        public IServingRepo servings{ get; private set; }
        public IUserCategoryRepo userCategories { get; private set; }
        public IUsersRepo users { get; private set; }
        public IActiveRepo active { get; private set; }


        public UnitOfWork(AppDbContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            auth = new AuthRepo(_db, _userManager, _roleManager);
            announcement = new AnnounceRepo(_db);
            device = new DeviceRepo(_db);
            category = new CategoryRepo(_db);
            queueNumbers = new QueueNumberRepo(_db);
            servings = new ServingRepo(_db);
            userCategories = new UserCategory(_db);
            users = new UsersRepo(_db);
            clerkForFilling = new ClerkForFillingRepo(_db);
            clerkReleasing = new ClerkReleasingRepo(_db);
            active = new ActiveRepo(_db);

        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}