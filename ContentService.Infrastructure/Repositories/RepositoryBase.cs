﻿using System.Linq.Expressions;
using ContentService.Application.Interfaces;
using ContentService.Domain;
using ContentService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace ContentService.Infrastructure.Repositories;

public class RepositoryBase<T>(TracioContentDbContext context) : IRepositoryBase<T>
    where T : class
{
    private readonly TracioContentDbContext _context = context;

    public async Task<TResult?> GetByIdAsync<TResult>(
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

    public async Task<List<TResult>> FindAsync<TResult>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(selector);

        return await _context.Set<T>()
            .Where(filter)
            .Select(selector)
            .ToListAsync();
    }

    public async Task<List<TResult>> FindAsyncWithPagingAndSorting<TResult>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, TResult>> selector,
        int pageIndex,
        int pageSize,
        Expression<Func<T, object>>? sortBy = null,
        bool ascending = true,
        params Expression<Func<T, object>>[] includes)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(selector);

        var query = _context.Set<T>().Where(filter);

        // Apply Includes
        query = includes.Aggregate(query, (current, include) => current.Include(include));

        // Apply Sorting
        if (sortBy != null)
        {
            query = ascending
                ? query.OrderBy(sortBy)
                : query.OrderByDescending(sortBy);
        }

        return await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(selector)
            .ToListAsync();
    }
    
    public async Task UpdateRangeAsync(Expression<Func<T, bool>> filter, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateExpression)
    {
        await _context.Set<T>()
            .Where(filter)
            .ExecuteUpdateAsync(updateExpression);
    }


    public async Task<bool> CreateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await _context.Set<T>().AddAsync(entity);
        
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(int id, T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var dbEntity = await _context.Set<T>().FindAsync(id);
        if (dbEntity == null)
        {
            throw new Exception($"Entity with ID {id} not found.");
        }

        _context.Entry(dbEntity).CurrentValues.SetValues(entity);
        
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var dbEntity = await _context.Set<T>().FindAsync(id);
        if (dbEntity == null)
        {
            throw new Exception($"Entity with ID {id} not found.");
        }

        _context.Set<T>().Remove(dbEntity);
        
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return await _context.Set<T>().AnyAsync(filter);
    }

    public async Task<long> CountAsync(Expression<Func<T, bool>> filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return await _context.Set<T>().LongCountAsync(filter);
    }
}
