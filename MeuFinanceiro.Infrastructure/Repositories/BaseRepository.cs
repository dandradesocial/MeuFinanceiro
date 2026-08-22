using Microsoft.EntityFrameworkCore;
using MeuFinanceiro.Infrastructure.Data;

namespace MeuFinanceiro.Infrastructure.Repositories;

public abstract class BaseRepository<T> where T : class
{
    protected readonly FinanceContext _context;
    protected readonly DbSet<T> _dbSet;

    protected BaseRepository(FinanceContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<List<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public virtual async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);
    public virtual async Task AddAsync(T entity) { await _dbSet.AddAsync(entity); await _context.SaveChangesAsync(); }
    public virtual async Task UpdateAsync(T entity) { _dbSet.Update(entity); await _context.SaveChangesAsync(); }
    public virtual async Task DeleteAsync(T entity) { _dbSet.Remove(entity); await _context.SaveChangesAsync(); }
}