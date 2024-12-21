using System.Linq.Expressions;

namespace ContentService.Application.Interfaces;

public interface IRepositoryBase<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    
    Task<List<T>> FindAsync(Expression<Func<T, bool>> filter);
    
    Task<List<TResult>> FindAsyncWithPagingAndSorting<TResult>(
        Expression<Func<T, bool>> filter, 
        Expression<Func<T, TResult>> selector,
        int pageIndex, 
        int pageSize,
        Expression<Func<T, object>>? sortBy = null,
        bool ascending = true);
    
    Task InsertAsync(T entity);
    
    Task UpdateAsync(string id, T entity);
    
    Task DeleteAsync(string id);
}