using System.Linq.Expressions;

namespace POS_ModernUI.Database.Repository.IRepository;
public interface IRepository<T> where T : class
{
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
    T Get(Expression<Func<T, bool>> filter, string? includeProp = null);
    IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProp = null);
}
