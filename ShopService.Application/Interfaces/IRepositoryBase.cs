using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace ShopService.Application.Interfaces;

public interface IRepositoryBase<T> where T : class
{
    Task<TResult?> GetByIdAsync<TResult>( Expression<Func<T, bool>> expression,
        Expression<Func<T, TResult>> selector);
    
    Task<List<TResult>> FindAsync<TResult>(Expression<Func<T, bool>> filter,
        Expression<Func<T, TResult>> selector,
        params Expression<Func<T, object>>[] includes);
    
    Task<List<TResult>> FindAsyncWithPagingAndSorting<TResult>(
        Expression<Func<T, bool>> filter, 
        Expression<Func<T, TResult>> selector,
        int pageIndex, 
        int pageSize,
        Expression<Func<T, object>>? sortBy = null,
        bool ascending = true,
        params Expression<Func<T, object>>[] includes);

    Task<bool> UpdateFieldsAsync(Expression<Func<T, bool>> filter, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateExpression);
    
    Task<bool> CreateAsync(T entity);
    
    Task<bool> UpdateAsync(int id, T entity);
    
    Task<bool> DeleteAsync(int id);
    
    Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
    
    Task<long> CountAsync(Expression<Func<T, bool>> filter);
}