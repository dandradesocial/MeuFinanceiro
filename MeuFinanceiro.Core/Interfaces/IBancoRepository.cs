using MeuFinanceiro.Core.Entities;

namespace MeuFinanceiro.Core.Interfaces;

public interface IBancoRepository
{
    Task<List<Banco>> GetAllAsync();
    Task<Banco?> GetByIdAsync(Guid id);
    Task AddAsync(Banco banco);
    Task UpdateAsync(Banco banco);
    Task DeleteAsync(Banco banco);
}