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

    Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);

    Task<bool> UpdateFieldsAsync(
        Expression<Func<T, bool>> filter,
        params (Expression<Func<T, object>> field, object value)[] updates);

    Task<TResult?> GetByIdAsync<TResult>(
        Expression<Func<T, bool>> expression,
        Expression<Func<T, TResult>> selector);

    Task<List<TResult>> FindAsync<TResult>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, TResult>> selector);
}