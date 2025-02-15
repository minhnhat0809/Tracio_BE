using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces;
using UserService.Domain;

namespace UserService.Infrastructure.Repositories;


public class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    protected readonly TracioUserDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public RepositoryBase(TracioUserDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync(
        Func<IQueryable<T>, IQueryable<T>>? filter = null,
        int pageIndex = 0,
        int pageSize = 0,
        string? sortBy = null,
        bool sortDesc = false,
        string includeProperties = "")
    {
        IQueryable<T> query = _dbSet;

        // Apply filtering
        if (filter != null)
        {
            query = filter(query);
        }

        // Include navigation properties
        foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = sortDesc
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        }

        // Apply paging
        if (pageIndex > 0 && pageSize > 0)
        {
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        return await query.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(object id, string includeProperties = "")
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));

        IQueryable<T> query = _dbSet;

        // Include navigation properties
        foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        // Try to use FindAsync if possible (Primary Key Lookup)
        var entity = await _dbSet.FindAsync(id);
    
        // If FindAsync doesn't work, fall back to query filtering
        if (entity == null)
        {
            var primaryKeyName = _context.Model
                .FindEntityType(typeof(T))
                ?.FindPrimaryKey()
                ?.Properties
                .FirstOrDefault()
                ?.Name;

            if (string.IsNullOrEmpty(primaryKeyName))
                throw new InvalidOperationException($"No primary key defined for {typeof(T).Name}");

            entity = await query.FirstOrDefaultAsync(e => EF.Property<object>(e, primaryKeyName).Equals(id));
        }

        return entity;
    }

    public async Task<TResult?> GetById<TResult>(
        Expression<Func<T, bool>> expression,
        Expression<Func<T, TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(selector);

        return await _context.Set<T>()
            .Where(expression)
            .Select(selector)
            .FirstOrDefaultAsync();
    }


    // Create
    public async Task<T?> CreateAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // Update
    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    // Hard Delete
    public async Task DeleteAsync(object id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    // Soft Delete (assumes the entity has a "IsDeleted" property)
    public async Task SoftDeleteAsync(object id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            var isDeletedProperty = entity.GetType().GetProperty("IsDeleted");
            if (isDeletedProperty != null)
            {
                isDeletedProperty.SetValue(entity, true);
                await UpdateAsync(entity);
            }
            else
            {
                throw new InvalidOperationException("Entity does not support soft delete.");
            }
        }
    }
}
