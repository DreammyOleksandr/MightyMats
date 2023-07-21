using System.Linq.Expressions;

namespace MightyMatsData.Repository.IRepository;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>> filter = null, string? includeProps = null);
    Task<T> Get(Expression<Func<T, bool>> filter, string? includeProps = null);
    Task Add(T entity);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    Task Save();
}