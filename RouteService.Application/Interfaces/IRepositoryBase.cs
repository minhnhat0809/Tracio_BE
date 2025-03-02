using System.Linq.Expressions;

namespace RouteService.Application.Interfaces;

public interface IRepositoryBase<T> where T : class
{
    Task<(IEnumerable<T> Items, int TotalCount)> GetAllAsync(
        int pageIndex, int pageSize, string? sortBy, bool sortDesc, Dictionary<string, string>? filters = null, string includeProperties = "");
    Task<T?> GetByIdAsync(object id, string includeProperties = "");
    Task<T?> GetByFilterAsync(Dictionary<string, string>? filters, string includeProperties = "");
    Task<TResult?> GetById<TResult>( Expression<Func<T, bool>> expression,
        Expression<Func<T, TResult>> selector);
    Task<T?> CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(object id);
    Task SoftDeleteAsync(object id);
    Task<bool> ExistsAsync(int id);
}
