using MeuFinanceiro.Core.Entities;

namespace MeuFinanceiro.Core.Interfaces;

public interface ICategoriaRepository
{
    Task<List<Categoria>> GetAllAsync();
    Task<Categoria?> GetByIdAsync(Guid id);
    Task AddAsync(Categoria categoria);
    Task UpdateAsync(Categoria categoria);
    Task DeleteAsync(Categoria categoria);
}