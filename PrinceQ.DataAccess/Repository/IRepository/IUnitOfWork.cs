using PrinceQ.DataAccess.Repository.IRepository;


namespace PrinceQ.DataAccess.Repository
{
    public interface IUnitOfWork
    {
        IAuthRepo auth { get; }
        IAnnounceRepo announcement { get; }
        IDeviceRepo device { get; }
        ICategoryRepo category { get; }
        IClerkForFillingRepo clerkForFilling { get; }
        IClerkReleasingRepo clerkReleasing { get; }
        IQueueNumberRepo queueNumbers { get; }
        IServingRepo servings { get; }
        IUserCategoryRepo userCategories { get; }
        IUsersRepo users { get; }
        IActiveRepo active { get; }

        Task SaveAsync();

    }
}