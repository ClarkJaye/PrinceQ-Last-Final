using System.Linq.Expressions;

namespace PrinceQ.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {

        Task<T?> Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false);

        Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);

        void Add(T entity);

        void Update(T entity);

        void Remove(T entity);

        bool Any(Expression<Func<T, bool>> filter);

        Task<int> Count(Expression<Func<T, bool>>? filter = null);

    }


}
