using System.Linq.Expressions;

namespace NotificationService.Application.Interfaces;

public interface IRepositoryBase<T> where T : class
{
    Task<List<TResult>> FindAsyncWithPagingAndSorting<TResult>(
        Expression<Func<T, bool>> filter, 
        Expression<Func<T, TResult>> selector,
        int pageIndex, 
        int pageSize,
        Expression<Func<T, object>>? sortBy = null,
        bool ascending = true);
    
    Task<bool> CreateAsync(T entity);

    Task<bool> DeleteAsync(string id);
    
    Task<long> CountAsync(Expression<Func<T, bool>> filter);
}