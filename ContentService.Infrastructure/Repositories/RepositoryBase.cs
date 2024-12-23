using System.Linq.Expressions;
using ContentService.Application.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ContentService.Infrastructure.Repositories;

public class RepositoryBase<T>(IMongoDatabase database, string collectionName) : IRepositoryBase<T>
    where T : class
{
    protected readonly IMongoCollection<T> Collection = database.GetCollection<T>(collectionName);

    public async Task<TResult?> GetByIdAsync<TResult>(
        Expression<Func<T, bool>> expression,
        Expression<Func<T, TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(selector);

        var result = await Collection
            .Find(expression)
            .Project(selector)
            .FirstOrDefaultAsync();

        return result;
    }


    public async Task<List<TResult>> FindAsync<TResult>(Expression<Func<T, bool>> filter,
        Expression<Func<T, TResult>> selector)
    {
        return await Collection.Find(filter).Project(selector).ToListAsync();
    }

    public async Task<List<TResult>> FindAsyncWithPagingAndSorting<TResult>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, TResult>> selector,
        int pageIndex,
        int pageSize,
        Expression<Func<T, object>>? sortBy = null,
        bool ascending = true)
    {
        var skip = (pageIndex - 1) * pageSize;

        var query = Collection.Find(filter);

        if (sortBy != null)
        {
            query = ascending
                ? query.SortBy(sortBy)
                : query.SortByDescending(sortBy);
        }

        query = query.Skip(skip).Limit(pageSize);

        var result = await query.Project(selector).ToListAsync();

        return result;
    }


    public async Task InsertAsync(T entity)
    {
        await Collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(string id, T entity)
    {
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        await Collection.ReplaceOneAsync(filter, entity);
    }

    public async Task DeleteAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        await Collection.DeleteOneAsync(filter);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
    {
        return await Collection.Find(filter).AnyAsync();
    }
    
    public async Task<long> CountAsync(Expression<Func<T, bool>> filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return await Collection.CountDocumentsAsync(filter);
    }
}