using System.Linq.Expressions;

namespace RouteService.Application.Interfaces;

public interface IRepositoryBase<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(
        Func<IQueryable<T>, IQueryable<T>>? filter = null, int pageIndex = 0, int pageSize = 0, string? sortBy = null, 
        bool sortDesc = false, string includeProperties = "");
    Task<T?> GetByIdAsync(object id, string includeProperties = "");

    Task<TResult?> GetById<TResult>( Expression<Func<T, bool>> expression,
        Expression<Func<T, TResult>> selector);
    Task<T?> CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(object id);
    Task SoftDeleteAsync(object id);
}