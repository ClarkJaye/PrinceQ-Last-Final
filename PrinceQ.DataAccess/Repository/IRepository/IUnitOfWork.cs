using PrinceQ.DataAccess.Repository.IRepository;


namespace PrinceQ.DataAccess.Repository
{
    public interface IUnitOfWork
    {
        IAuthRepo auth { get; }
        IAnnounceRepo announcement { get; }
        IDeviceRepo device { get; }
        ICategoryRepo category { get; }
        IForFillingRepo forFilling { get; }
        IReleasingRepo releasing { get; }
        IQueueNumberRepo queueNumbers { get; }
        IServingRepo servings { get; }
        IUserCategoryRepo userCategories { get; }
        IUsersRepo users { get; }
        //IActiveRepo active { get; }

        Task SaveAsync();

    }
}