using Microsoft.EntityFrameworkCore;
using ReportesLocalidadApi.Models;
using ReportesLocalidadApi.Models.Entities;

namespace ReportesLocalidadApi.Repositories;

public class Repository<T> where T : class
{
    private readonly ReportesLocalidadContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(ReportesLocalidadContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public async Task<IReadOnlyList<T>> GetCantidadAsync(int cantidad)
    {
        return await _dbSet.AsNoTracking()
            .Take(cantidad)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<T>> GetPagedAsync(int skip, int take)
    {
        skip = Math.Max(skip, 0);
        take = Math.Clamp(take, 1, 50);

        return await _dbSet.AsNoTracking()
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
